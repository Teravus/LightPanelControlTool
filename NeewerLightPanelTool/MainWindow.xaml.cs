using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using NeewerLightControlBT;
using NeewerLightPanelTool.Services;
using NeewerLightPanelTool.ViewModels;
using Forms = System.Windows.Forms;

namespace NeewerLightPanelTool;

public partial class MainWindow : Window
{
    private static readonly string[] SceneNames =
    [
        "Ambulance",
        "Candlelight",
        "Fire Engine",
        "Fireworks",
        "Lightning",
        "Party",
        "Paparazzi",
        "Screen",
        "Squard Car"
    ];

    private readonly NeewerLightDiscoveryService _lights = new();
    private readonly ObservableCollection<NeewerLightDeviceViewModel> _discoveredLights = [];
    private readonly ObservableCollection<NeewerLightGroupViewModel> _lightGroups = [];
    private readonly DispatcherTimer _applyTimer;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly StreamDeckHttpServer _streamDeckServer = new();
    private readonly string _lastStateFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RebootTech",
        "NeewerLightPanelTool",
        "last-state.json");
    private bool _isUpdatingControls;
    private bool _isWindowLoaded;
    private bool _isLoadingLastState;
    private bool _isChangingTargetSelection;
    private string _selectedSceneName = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
        LightsListBox.ItemsSource = _discoveredLights;
        GroupsListBox.ItemsSource = _lightGroups;
        SceneItemsControl.ItemsSource = SceneNames;
        _applyTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(180)
        };
        _applyTimer.Tick += ApplyTimer_Tick;
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _isWindowLoaded = true;
        UpdateColorPreview();
        ToneValueTextBlock.Text = $"{GetTone()} K";
        BrightnessValueTextBlock.Text = $"{GetBrightness():0}%";
        await LoadLastStateAsync().ConfigureAwait(true);
        UpdateStreamDeckRequestText();
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        await ScanAsync().ConfigureAwait(true);
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        await ConnectSelectedAsync().ConfigureAwait(true);
    }

    private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        await DisconnectSelectedAsync().ConfigureAwait(true);
    }

    private async void StartStopServerButton_Click(object sender, RoutedEventArgs e)
    {
        await ToggleStreamDeckServerAsync().ConfigureAwait(true);
    }

    private async void OffButton_Click(object sender, RoutedEventArgs e)
    {
        await TurnOffTargetAsync().ConfigureAwait(true);
    }

    private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
    {
        CreateGroupFromSelectedLights();
    }

    private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
    {
        DeleteSelectedGroup();
    }

    private async void SaveGroupsButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveGroupsAsync().ConfigureAwait(true);
    }

    private async void LoadGroupsButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadGroupsAsync().ConfigureAwait(true);
    }

    private void UseSelectedLightsButton_Click(object sender, RoutedEventArgs e)
    {
        GroupsListBox.SelectedItem = null;
        UpdateTargetStatus();
    }

    private void ServerAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateStreamDeckRequestText();
    }

    private void LightsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isChangingTargetSelection)
        {
            return;
        }

        if (LightsListBox.SelectedItems.Count > 0 && GroupsListBox?.SelectedItem is not null)
        {
            _isChangingTargetSelection = true;
            GroupsListBox.SelectedItem = null;
            _isChangingTargetSelection = false;
        }

        if (GroupsListBox?.SelectedItem is null)
        {
            UpdateTargetStatus();
        }

        UpdateStreamDeckRequestText();
    }

    private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isChangingTargetSelection)
        {
            return;
        }

        if (GroupsListBox.SelectedItem is NeewerLightGroupViewModel group)
        {
            _isChangingTargetSelection = true;
            LightsListBox.SelectedItems.Clear();
            _isChangingTargetSelection = false;
            ApplyGroupStateToControls(group.State);
        }

        UpdateTargetStatus();
        UpdateStreamDeckRequestText();
    }

    private async Task ScanAsync()
    {
        SetBusy(true, "Scanning Bluetooth for NEEWER panels...");
        try
        {
            IReadOnlyList<NeewerLightDeviceViewModel> lights = await _lights.ScanAsync().ConfigureAwait(true);
            _discoveredLights.Clear();
            foreach (NeewerLightDeviceViewModel light in lights)
            {
                _discoveredLights.Add(light);
            }

            RefreshGroupStatuses();
            StatusTextBlock.Text = lights.Count == 0
                ? "Scan complete. No NEEWER-compatible Bluetooth panels were found."
                : $"Scan complete. Found {lights.Count} NEEWER-compatible panel(s).";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Scan failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ConnectSelectedAsync()
    {
        IReadOnlyList<NeewerLightDeviceViewModel> selected_lights = GetConnectTargetLights();
        if (selected_lights.Count == 0)
        {
            StatusTextBlock.Text = GroupsListBox.SelectedItem is NeewerLightGroupViewModel group
                ? $"None of the lights in group {group.Name} are currently discovered. Click Scan Bluetooth first."
                : "Select one or more discovered lights first.";
            return;
        }

        SetBusy(true, "Connecting selected lights...");
        try
        {
            foreach (NeewerLightDeviceViewModel light in selected_lights)
            {
                light.Status = "Connecting...";
                bool connected = await _lights.ConnectAsync(light.DisplayId).ConfigureAwait(true);
                light.IsConnected = connected;
                light.Status = connected ? "Connected" : "Connect failed";
            }

            RefreshGroupStatuses();
            await ApplyCurrentInstructionAsync().ConfigureAwait(true);
            StatusTextBlock.Text = "Selected lights connected.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Connect failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task DisconnectSelectedAsync()
    {
        IReadOnlyList<NeewerLightDeviceViewModel> selected_lights = GetSelectedLights();
        if (selected_lights.Count == 0)
        {
            StatusTextBlock.Text = "Select one or more connected lights first.";
            return;
        }

        SetBusy(true, "Disconnecting selected lights...");
        try
        {
            foreach (NeewerLightDeviceViewModel light in selected_lights)
            {
                await _lights.DisconnectAsync(light.DisplayId).ConfigureAwait(true);
                light.IsConnected = false;
                light.Status = "Disconnected";
            }

            RefreshGroupStatuses();
            StatusTextBlock.Text = "Selected lights disconnected.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Disconnect failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ToggleStreamDeckServerAsync()
    {
        if (_streamDeckServer.IsRunning)
        {
            await _streamDeckServer.StopAsync().ConfigureAwait(true);
            StartStopServerButton.Content = "Start HTTP";
            InterfaceIpTextBox.IsEnabled = true;
            HttpPortTextBox.IsEnabled = true;
            StatusTextBlock.Text = "StreamDeck HTTP listener stopped.";
            UpdateStreamDeckRequestText();
            await SaveLastStateAsync().ConfigureAwait(true);
            return;
        }

        string ip_address = InterfaceIpTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(ip_address))
        {
            StatusTextBlock.Text = "Enter a listening interface IP address.";
            return;
        }

        if (!int.TryParse(HttpPortTextBox.Text.Trim(), out int port) || port <= 0 || port > 65535)
        {
            StatusTextBlock.Text = "Enter a valid HTTP port from 1 to 65535.";
            return;
        }

        try
        {
            await _streamDeckServer.StartAsync(ip_address, port, HandleStreamDeckRequestOnUiAsync).ConfigureAwait(true);
            StartStopServerButton.Content = "Stop HTTP";
            InterfaceIpTextBox.IsEnabled = false;
            HttpPortTextBox.IsEnabled = false;
            StatusTextBlock.Text = $"StreamDeck HTTP listener started on http://{ip_address}:{port}.";
            UpdateStreamDeckRequestText();
            await SaveLastStateAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"HTTP listener failed to start: {ex.Message}";
        }
    }

    private Task<StreamDeckLightResponse> HandleStreamDeckRequestOnUiAsync(StreamDeckLightRequest request)
    {
        return Dispatcher.InvokeAsync(() => HandleStreamDeckRequestAsync(request)).Task.Unwrap();
    }

    private async Task<StreamDeckLightResponse> HandleStreamDeckRequestAsync(StreamDeckLightRequest request)
    {
        try
        {
            if (string.Equals(request.Action, "connect", StringComparison.OrdinalIgnoreCase))
            {
                return await ConnectRequestTargetAsync(request).ConfigureAwait(true);
            }

            IReadOnlyList<string> connected_light_ids = GetConnectedLightIdsForRequest(request);
            if (connected_light_ids.Count == 0)
            {
                return new StreamDeckLightResponse(false, "No connected lights matched the requested group or light.");
            }

            StreamDeckLightResponse apply_response = await ApplyStreamDeckRequestToControlsAsync(request, connected_light_ids).ConfigureAwait(true);
            if (apply_response.Success)
            {
                StoreStateForRequestGroup(request);
                UpdateStreamDeckRequestText();
            }

            return apply_response;
        }
        catch (Exception ex)
        {
            return new StreamDeckLightResponse(false, ex.Message);
        }
    }

    private async Task<StreamDeckLightResponse> ConnectRequestTargetAsync(StreamDeckLightRequest request)
    {
        IReadOnlyList<NeewerLightDeviceViewModel> lights = GetLightsForRequest(request);
        if (lights.Count == 0)
        {
            return new StreamDeckLightResponse(false, "No discovered lights matched the requested group or light.");
        }

        foreach (NeewerLightDeviceViewModel light in lights)
        {
            light.Status = "Connecting...";
            bool connected = await _lights.ConnectAsync(light.DisplayId).ConfigureAwait(true);
            light.IsConnected = connected;
            light.Status = connected ? "Connected" : "Connect failed";
        }

        RefreshGroupStatuses();
        return new StreamDeckLightResponse(true, $"Connected {lights.Count} light(s).");
    }

    private async Task<StreamDeckLightResponse> ApplyStreamDeckRequestToControlsAsync(StreamDeckLightRequest request, IReadOnlyList<string> connectedLightIds)
    {
        if (string.Equals(request.Action, "power", StringComparison.OrdinalIgnoreCase))
        {
            return await ApplyPowerRequestAsync(request, connectedLightIds).ConfigureAwait(true);
        }

        _isUpdatingControls = true;
        try
        {
            if (request.Brightness.HasValue)
            {
                BrightnessSlider.Value = Math.Clamp(request.Brightness.Value, 0, 100);
            }

            if (string.Equals(request.Action, "rgb", StringComparison.OrdinalIgnoreCase))
            {
                if (!request.Red.HasValue || !request.Green.HasValue || !request.Blue.HasValue)
                {
                    return new StreamDeckLightResponse(false, "RGB requests require r, g, and b.");
                }

                RedSlider.Value = Math.Clamp(request.Red.Value, 0, 255);
                GreenSlider.Value = Math.Clamp(request.Green.Value, 0, 255);
                BlueSlider.Value = Math.Clamp(request.Blue.Value, 0, 255);
                RgbModeRadioButton.IsChecked = true;
            }
            else if (string.Equals(request.Action, "cct", StringComparison.OrdinalIgnoreCase))
            {
                if (!request.Tone.HasValue)
                {
                    return new StreamDeckLightResponse(false, "CCT requests require tone.");
                }

                ToneSlider.Value = Math.Clamp(request.Tone.Value, 3200, 5600);
                CctModeRadioButton.IsChecked = true;
            }
            else if (string.Equals(request.Action, "scene", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.SceneName) || !SceneNames.Contains(request.SceneName, StringComparer.OrdinalIgnoreCase))
                {
                    return new StreamDeckLightResponse(false, "Scene requests require a valid scenename.");
                }

                _selectedSceneName = request.SceneName;
                SceneModeRadioButton.IsChecked = true;
            }
            else if (!string.Equals(request.Action, "brightness", StringComparison.OrdinalIgnoreCase))
            {
                return new StreamDeckLightResponse(false, $"Unknown action {request.Action}.");
            }

            RgbGroupBox.Visibility = RgbModeRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            CctGroupBox.Visibility = CctModeRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            SceneGroupBox.Visibility = SceneModeRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            UpdateColorPreview();
            ToneValueTextBlock.Text = $"{GetTone()} K";
            BrightnessValueTextBlock.Text = $"{GetBrightness():0}%";
        }
        finally
        {
            _isUpdatingControls = false;
        }

        if (string.Equals(request.Action, "scene", StringComparison.OrdinalIgnoreCase))
        {
            await ExecuteSceneInstructionAsync(connectedLightIds, _selectedSceneName).ConfigureAwait(true);
            return new StreamDeckLightResponse(true, $"Applied scene {_selectedSceneName} to {connectedLightIds.Count} light(s).");
        }

        NeewerLightInstruction instruction = CreateCurrentInstruction();
        await _lights.ExecuteAsync(connectedLightIds, instruction).ConfigureAwait(true);
        return new StreamDeckLightResponse(true, $"Applied {instruction.LightMode} to {connectedLightIds.Count} light(s).");
    }

    private async Task<StreamDeckLightResponse> ApplyPowerRequestAsync(StreamDeckLightRequest request, IReadOnlyList<string> connectedLightIds)
    {
        bool turn_on = string.Equals(request.Power, "on", StringComparison.OrdinalIgnoreCase) || request.Power == "1";
        if (!turn_on)
        {
            _applyTimer.Stop();
            _isUpdatingControls = true;
            BrightnessSlider.Value = 0;
            BrightnessValueTextBlock.Text = "0%";
            _isUpdatingControls = false;
            await _lights.ExecuteAsync(connectedLightIds, new NeewerLightInstruction { LightMode = nLightMode.OFF }).ConfigureAwait(true);
            return new StreamDeckLightResponse(true, $"Turned off {connectedLightIds.Count} light(s).");
        }

        if (request.Brightness.HasValue || GetBrightness() <= 0)
        {
            _isUpdatingControls = true;
            BrightnessSlider.Value = Math.Clamp(request.Brightness ?? 39, 1, 100);
            BrightnessValueTextBlock.Text = $"{GetBrightness():0}%";
            _isUpdatingControls = false;
        }

        NeewerLightInstruction instruction = CreateCurrentInstruction();
        await _lights.ExecuteAsync(connectedLightIds, instruction).ConfigureAwait(true);
        return new StreamDeckLightResponse(true, $"Turned on {connectedLightIds.Count} light(s).");
    }

    private void ModeRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingControls || !_isWindowLoaded)
        {
            return;
        }

        RgbGroupBox.Visibility = RgbModeRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        CctGroupBox.Visibility = CctModeRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        SceneGroupBox.Visibility = SceneModeRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        QueueApply();
        UpdateStreamDeckRequestText();
    }

    private void RgbSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingControls || !_isWindowLoaded)
        {
            return;
        }

        UpdateColorPreview();
        QueueApply();
        UpdateStreamDeckRequestText();
    }

    private void ToneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingControls || !_isWindowLoaded)
        {
            return;
        }

        ToneValueTextBlock.Text = $"{GetTone()} K";
        QueueApply();
        UpdateStreamDeckRequestText();
    }

    private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isUpdatingControls || !_isWindowLoaded)
        {
            return;
        }

        BrightnessValueTextBlock.Text = $"{GetBrightness():0}%";
        QueueApply();
        UpdateStreamDeckRequestText();
    }

    private async void ControlSlider_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _applyTimer.Stop();
        await ApplyCurrentInstructionAsync().ConfigureAwait(true);
    }

    private void PickColorButton_Click(object sender, RoutedEventArgs e)
    {
        using Forms.ColorDialog dialog = new()
        {
            Color = System.Drawing.Color.FromArgb(GetRed(), GetGreen(), GetBlue()),
            FullOpen = true
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
        {
            return;
        }

        _isUpdatingControls = true;
        RedSlider.Value = dialog.Color.R;
        GreenSlider.Value = dialog.Color.G;
        BlueSlider.Value = dialog.Color.B;
        _isUpdatingControls = false;

        UpdateColorPreview();
        QueueApply();
        UpdateStreamDeckRequestText();
    }

    private async void ApplyRgbButton_Click(object sender, RoutedEventArgs e)
    {
        await ApplyCurrentInstructionAsync().ConfigureAwait(true);
    }

    private async void SceneButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Content: string scene_name })
        {
            _selectedSceneName = scene_name;
            SceneModeRadioButton.IsChecked = true;
            UpdateStreamDeckRequestText();
            await ApplySceneInstructionAsync(scene_name).ConfigureAwait(true);
        }
    }

    private async void ApplyTimer_Tick(object? sender, EventArgs e)
    {
        _applyTimer.Stop();
        await ApplyCurrentInstructionAsync().ConfigureAwait(true);
    }

    private async Task ApplyCurrentInstructionAsync()
    {
        IReadOnlyList<string> connected_light_ids = GetTargetConnectedLightIds();
        if (connected_light_ids.Count == 0)
        {
            return;
        }

        try
        {
            NeewerLightInstruction instruction = CreateCurrentInstruction();
            await _lights.ExecuteAsync(connected_light_ids, instruction).ConfigureAwait(true);
            StoreCurrentStateForSelectedGroup();
            StatusTextBlock.Text = $"Applied {instruction.LightMode} to {connected_light_ids.Count} selected light(s).";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Light command failed: {ex.Message}";
        }
    }

    private async Task ApplySceneInstructionAsync(string sceneName)
    {
        IReadOnlyList<string> connected_light_ids = GetTargetConnectedLightIds();
        if (connected_light_ids.Count == 0)
        {
            return;
        }

        float brightness = GetBrightness();
        if (brightness <= 0)
        {
            brightness = 39;
            _isUpdatingControls = true;
            BrightnessSlider.Value = brightness;
            BrightnessValueTextBlock.Text = $"{brightness:0}%";
            _isUpdatingControls = false;
        }

        try
        {
            await ExecuteSceneInstructionAsync(connected_light_ids, sceneName).ConfigureAwait(true);
            StoreCurrentStateForSelectedGroup();
            UpdateStreamDeckRequestText();
            StatusTextBlock.Text = $"Applied {sceneName} scene at {brightness:0}% to {connected_light_ids.Count} selected light(s).";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Scene command failed: {ex.Message}";
        }
    }

    private async Task ExecuteSceneInstructionAsync(IReadOnlyList<string> connectedLightIds, string sceneName)
    {
        float brightness = GetBrightness();
        if (brightness <= 0)
        {
            brightness = 39;
            _isUpdatingControls = true;
            BrightnessSlider.Value = brightness;
            BrightnessValueTextBlock.Text = $"{brightness:0}%";
            _isUpdatingControls = false;
        }

        NeewerLightInstruction brightness_instruction = new()
        {
            LightMode = nLightMode.CCTMode,
            CCT = GetTone(),
            brightness = brightness
        };

        NeewerLightInstruction scene_instruction = new()
        {
            LightMode = nLightMode.SCEMode,
            SceneName = sceneName,
            brightness = brightness
        };

        await _lights.ExecuteAsync(connectedLightIds, brightness_instruction).ConfigureAwait(true);
        await _lights.ExecuteAsync(connectedLightIds, scene_instruction).ConfigureAwait(true);
    }

    private async Task TurnOffTargetAsync()
    {
        IReadOnlyList<string> connected_light_ids = GetTargetConnectedLightIds();
        if (connected_light_ids.Count == 0)
        {
            StatusTextBlock.Text = "Select connected lights or a group with connected lights before turning off.";
            return;
        }

        _applyTimer.Stop();
        _isUpdatingControls = true;
        BrightnessSlider.Value = 0;
        BrightnessValueTextBlock.Text = "0%";
        _isUpdatingControls = false;

        try
        {
            await _lights.ExecuteAsync(connected_light_ids, new NeewerLightInstruction { LightMode = nLightMode.OFF }).ConfigureAwait(true);
            StoreCurrentStateForSelectedGroup();
            UpdateStreamDeckRequestText();
            StatusTextBlock.Text = $"Turned off {connected_light_ids.Count} selected light(s).";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Off command failed: {ex.Message}";
        }
    }

    private NeewerLightInstruction CreateCurrentInstruction()
    {
        float brightness = GetBrightness();
        if (brightness <= 0)
        {
            return new NeewerLightInstruction { LightMode = nLightMode.OFF };
        }

        if (RgbModeRadioButton.IsChecked == true)
        {
            return new NeewerLightInstruction
            {
                LightMode = nLightMode.HSIMode,
                RGB = new NeewerColor { R = GetRed(), G = GetGreen(), B = GetBlue() },
                brightness = brightness
            };
        }

        if (SceneModeRadioButton.IsChecked == true)
        {
            return new NeewerLightInstruction
            {
                LightMode = nLightMode.SCEMode,
                SceneName = string.IsNullOrWhiteSpace(_selectedSceneName) ? SceneNames[0] : _selectedSceneName,
                brightness = brightness
            };
        }

        return new NeewerLightInstruction
        {
            LightMode = nLightMode.CCTMode,
            CCT = GetTone(),
            brightness = brightness
        };
    }

    private void QueueApply()
    {
        if (!_isWindowLoaded)
        {
            return;
        }

        _applyTimer.Stop();
        _applyTimer.Start();
    }

    private void UpdateColorPreview()
    {
        byte r = GetRed();
        byte g = GetGreen();
        byte b = GetBlue();
        RedValueTextBlock.Text = r.ToString();
        GreenValueTextBlock.Text = g.ToString();
        BlueValueTextBlock.Text = b.ToString();
        ColorPreviewBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
    }

    private IReadOnlyList<NeewerLightDeviceViewModel> GetSelectedLights()
    {
        return LightsListBox.SelectedItems.Cast<NeewerLightDeviceViewModel>().ToList();
    }

    private IReadOnlyList<NeewerLightDeviceViewModel> GetConnectTargetLights()
    {
        IReadOnlyList<NeewerLightDeviceViewModel> selected_lights = GetSelectedLights();
        if (selected_lights.Count > 0)
        {
            return selected_lights;
        }

        if (GroupsListBox.SelectedItem is not NeewerLightGroupViewModel group)
        {
            return [];
        }

        return group.LightIds
            .Select(FindDiscoveredLight)
            .Where(light => light is not null)
            .Select(light => light!)
            .ToList();
    }

    private IReadOnlyList<string> GetTargetConnectedLightIds()
    {
        NeewerLightGroupViewModel? selected_group = GroupsListBox.SelectedItem as NeewerLightGroupViewModel;
        if (selected_group is not null)
        {
            return selected_group.LightIds
                .Select(FindDiscoveredLight)
                .Where(light => light?.IsConnected == true)
                .Select(light => light!.DisplayId)
                .ToList();
        }

        return GetSelectedLights()
            .Where(light => light.IsConnected)
            .Select(light => light.DisplayId)
            .ToList();
    }

    private IReadOnlyList<string> GetConnectedLightIdsForRequest(StreamDeckLightRequest request)
    {
        return GetLightsForRequest(request)
            .Where(light => light.IsConnected)
            .Select(light => light.DisplayId)
            .ToList();
    }

    private IReadOnlyList<NeewerLightDeviceViewModel> GetLightsForRequest(StreamDeckLightRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.GroupName))
        {
            NeewerLightGroupViewModel? group = FindGroup(request.GroupName);
            return group is null
                ? []
                : group.LightIds.Select(FindDiscoveredLight).Where(light => light is not null).Select(light => light!).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.LightId))
        {
            NeewerLightDeviceViewModel? light = FindDiscoveredLight(request.LightId);
            return light is null ? [] : [light];
        }

        if (GroupsListBox.SelectedItem is NeewerLightGroupViewModel selected_group)
        {
            return selected_group.LightIds.Select(FindDiscoveredLight).Where(light => light is not null).Select(light => light!).ToList();
        }

        return GetSelectedLights();
    }

    private void CreateGroupFromSelectedLights()
    {
        IReadOnlyList<NeewerLightDeviceViewModel> selected_lights = GetSelectedLights();
        if (selected_lights.Count == 0)
        {
            StatusTextBlock.Text = "Select one or more discovered lights before creating a group.";
            return;
        }

        string group_name = GroupNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(group_name))
        {
            group_name = $"Group {_lightGroups.Count + 1}";
        }

        if (_lightGroups.Any(group => string.Equals(group.Name, group_name, StringComparison.OrdinalIgnoreCase)))
        {
            StatusTextBlock.Text = $"A group named {group_name} already exists.";
            return;
        }

        List<string> light_ids = selected_lights
            .Select(light => light.DisplayId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        NeewerLightGroupViewModel group = new(group_name, light_ids);
        group.State = CaptureCurrentState();
        _lightGroups.Add(group);
        GroupsListBox.SelectedItem = group;
        RefreshGroupStatus(group);
        StatusTextBlock.Text = $"Created group {group.Name} with {light_ids.Count} light(s).";
        _ = SaveLastStateAsync();
    }

    private void DeleteSelectedGroup()
    {
        if (GroupsListBox.SelectedItem is not NeewerLightGroupViewModel group)
        {
            StatusTextBlock.Text = "Select a group to delete.";
            return;
        }

        _lightGroups.Remove(group);
        GroupsListBox.SelectedItem = null;
        StatusTextBlock.Text = $"Deleted group {group.Name}.";
        UpdateTargetStatus();
        _ = SaveLastStateAsync();
    }

    private void SelectGroupLights(NeewerLightGroupViewModel group)
    {
        LightsListBox.SelectedItems.Clear();
        foreach (NeewerLightDeviceViewModel light in _discoveredLights.Where(light => group.LightIds.Contains(light.DisplayId, StringComparer.OrdinalIgnoreCase)))
        {
            LightsListBox.SelectedItems.Add(light);
        }
    }

    private void RefreshGroupStatuses()
    {
        foreach (NeewerLightGroupViewModel group in _lightGroups)
        {
            RefreshGroupStatus(group);
        }
    }

    private void RefreshGroupStatus(NeewerLightGroupViewModel group)
    {
        int connected_count = group.LightIds
            .Select(FindDiscoveredLight)
            .Count(light => light?.IsConnected == true);

        group.Status = $"{group.LightIds.Count} light(s), {connected_count} connected";
    }

    private void UpdateTargetStatus()
    {
        if (!_isWindowLoaded)
        {
            return;
        }

        if (GroupsListBox.SelectedItem is NeewerLightGroupViewModel group)
        {
            HeaderStatusTextBlock.Text = $"Target group: {group.Name}";
            return;
        }

        int selected_count = GetSelectedLights().Count;
        HeaderStatusTextBlock.Text = selected_count == 0
            ? "Target: selected lights."
            : $"Target: {selected_count} selected light(s).";
    }

    private NeewerLightDeviceViewModel? FindDiscoveredLight(string displayId)
    {
        return _discoveredLights.FirstOrDefault(light => string.Equals(light.DisplayId, displayId, StringComparison.OrdinalIgnoreCase));
    }

    private NeewerLightGroupViewModel? FindGroup(string groupName)
    {
        return _lightGroups.FirstOrDefault(group => string.Equals(group.Name, groupName, StringComparison.OrdinalIgnoreCase));
    }

    private void StoreStateForRequestGroup(StreamDeckLightRequest request)
    {
        NeewerLightGroupViewModel? group = string.IsNullOrWhiteSpace(request.GroupName)
            ? GroupsListBox.SelectedItem as NeewerLightGroupViewModel
            : FindGroup(request.GroupName);

        if (group is not null)
        {
            group.State = CaptureCurrentState();
        }
    }

    private void UpdateStreamDeckRequestText()
    {
        if (!_isWindowLoaded)
        {
            return;
        }

        string target_query = GetStreamDeckTargetQuery();
        if (string.IsNullOrWhiteSpace(target_query))
        {
            StreamDeckRequestTextBox.Text = "Select one light or select/create a group to generate a StreamDeck URL.";
            return;
        }

        string base_url = $"http://{InterfaceIpTextBox.Text.Trim()}:{HttpPortTextBox.Text.Trim()}";
        StreamDeckRequestTextBox.Text = $"{base_url}{CreateCurrentStreamDeckPath()}?{target_query}&{CreateCurrentStreamDeckQuery()}";
    }

    private string GetStreamDeckTargetQuery()
    {
        if (GroupsListBox.SelectedItem is NeewerLightGroupViewModel group)
        {
            return $"group={Uri.EscapeDataString(group.Name)}";
        }

        IReadOnlyList<NeewerLightDeviceViewModel> selected_lights = GetSelectedLights();
        return selected_lights.Count == 1
            ? $"light={Uri.EscapeDataString(selected_lights[0].DisplayId)}"
            : string.Empty;
    }

    private string CreateCurrentStreamDeckPath()
    {
        if (SceneModeRadioButton.IsChecked == true)
        {
            return "/neewerbt_SceneSet";
        }

        if (CctModeRadioButton.IsChecked == true)
        {
            return "/neewerbt_CCTToneSet";
        }

        return "/neewerbt_RGBSet";
    }

    private string CreateCurrentStreamDeckQuery()
    {
        string brightness = $"brightness={GetBrightness():0.###}";
        if (SceneModeRadioButton.IsChecked == true)
        {
            string scene_name = string.IsNullOrWhiteSpace(_selectedSceneName) ? SceneNames[0] : _selectedSceneName;
            return $"scenename={Uri.EscapeDataString(scene_name)}&{brightness}";
        }

        if (CctModeRadioButton.IsChecked == true)
        {
            return $"tone={GetTone()}&{brightness}";
        }

        return $"r={GetRed()}&g={GetGreen()}&b={GetBlue()}&{brightness}";
    }

    private async Task SaveGroupsAsync()
    {
        StoreCurrentStateForSelectedGroup();
        using Forms.SaveFileDialog dialog = new()
        {
            Title = "Save Neewer group configuration",
            Filter = "Neewer group configuration (*.json)|*.json|JSON files (*.json)|*.json|All files (*.*)|*.*",
            FileName = "neewer-light-groups.json",
            AddExtension = true,
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
        {
            return;
        }

        try
        {
            NeewerLightPanelConfig config = CreateCurrentConfig();
            string json = JsonSerializer.Serialize(config, _jsonOptions);
            await File.WriteAllTextAsync(dialog.FileName, json).ConfigureAwait(true);
            await SaveLastStateAsync().ConfigureAwait(true);
            StatusTextBlock.Text = $"Saved {_lightGroups.Count} group(s) to {dialog.FileName}.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Save failed: {ex.Message}";
        }
    }

    private async Task LoadGroupsAsync()
    {
        using Forms.OpenFileDialog dialog = new()
        {
            Title = "Load Neewer group configuration",
            Filter = "Neewer group configuration (*.json)|*.json|JSON files (*.json)|*.json|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != Forms.DialogResult.OK)
        {
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(dialog.FileName).ConfigureAwait(true);
            NeewerLightPanelConfig? config = JsonSerializer.Deserialize<NeewerLightPanelConfig>(json, _jsonOptions);
            if (config is null)
            {
                StatusTextBlock.Text = "Load failed: configuration file was empty.";
                return;
            }

            ApplyConfig(config);
            await SaveLastStateAsync().ConfigureAwait(true);
            StatusTextBlock.Text = $"Loaded {_lightGroups.Count} group(s) from {dialog.FileName}.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Load failed: {ex.Message}";
        }
    }

    private async Task SaveLastStateAsync()
    {
        if (_isLoadingLastState)
        {
            return;
        }

        try
        {
            StoreCurrentStateForSelectedGroupWithoutSaving();
            string? directory = Path.GetDirectoryName(_lastStateFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(CreateCurrentConfig(), _jsonOptions);
            await File.WriteAllTextAsync(_lastStateFilePath, json).ConfigureAwait(true);
        }
        catch
        {
            // Last-state persistence is a convenience path; explicit save/load reports errors to the UI.
        }
    }

    private async Task LoadLastStateAsync()
    {
        if (!File.Exists(_lastStateFilePath))
        {
            return;
        }

        _isLoadingLastState = true;
        try
        {
            string json = await File.ReadAllTextAsync(_lastStateFilePath).ConfigureAwait(true);
            NeewerLightPanelConfig? config = JsonSerializer.Deserialize<NeewerLightPanelConfig>(json, _jsonOptions);
            if (config is null)
            {
                return;
            }

            bool restart_http = config.HttpServerWasRunning;
            ApplyConfig(config);
            StatusTextBlock.Text = $"Loaded last state from {_lastStateFilePath}.";

            if (restart_http)
            {
                await ToggleStreamDeckServerAsync().ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Last state load failed: {ex.Message}";
        }
        finally
        {
            _isLoadingLastState = false;
        }
    }

    private NeewerLightPanelConfig CreateCurrentConfig()
    {
        return new NeewerLightPanelConfig
        {
            InterfaceIp = InterfaceIpTextBox.Text.Trim(),
            HttpPort = int.TryParse(HttpPortTextBox.Text.Trim(), out int port) ? port : 5088,
            HttpServerWasRunning = _streamDeckServer.IsRunning,
            Groups = _lightGroups.Select(ToGroupConfig).ToList()
        };
    }

    private void ApplyConfig(NeewerLightPanelConfig config)
    {
        InterfaceIpTextBox.Text = string.IsNullOrWhiteSpace(config.InterfaceIp) ? "127.0.0.1" : config.InterfaceIp;
        HttpPortTextBox.Text = config.HttpPort is > 0 and <= 65535 ? config.HttpPort.ToString() : "5088";

        _lightGroups.Clear();
        foreach (NeewerLightGroupConfig group_config in config.Groups.Where(group => !string.IsNullOrWhiteSpace(group.Name)))
        {
            NeewerLightGroupViewModel group = new(group_config.Name, group_config.LightIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList())
            {
                State = ToStateViewModel(group_config.State)
            };
            RefreshGroupStatus(group);
            _lightGroups.Add(group);
        }

        GroupsListBox.SelectedItem = _lightGroups.FirstOrDefault();
        UpdateStreamDeckRequestText();
    }

    private void StoreCurrentStateForSelectedGroup()
    {
        StoreCurrentStateForSelectedGroupWithoutSaving();
        _ = SaveLastStateAsync();
    }

    private void StoreCurrentStateForSelectedGroupWithoutSaving()
    {
        if (GroupsListBox.SelectedItem is NeewerLightGroupViewModel group)
        {
            group.State = CaptureCurrentState();
        }
    }

    private NeewerLightStateViewModel CaptureCurrentState()
    {
        return new NeewerLightStateViewModel
        {
            Mode = GetCurrentModeName(),
            Brightness = GetBrightness(),
            Red = GetRed(),
            Green = GetGreen(),
            Blue = GetBlue(),
            Tone = GetTone(),
            SceneName = _selectedSceneName
        };
    }

    private void ApplyGroupStateToControls(NeewerLightStateViewModel state)
    {
        _isUpdatingControls = true;
        try
        {
            RedSlider.Value = state.Red;
            GreenSlider.Value = state.Green;
            BlueSlider.Value = state.Blue;
            ToneSlider.Value = Math.Clamp(state.Tone, 3200, 5600);
            BrightnessSlider.Value = Math.Clamp(state.Brightness, 0, 100);
            _selectedSceneName = state.SceneName;

            RgbModeRadioButton.IsChecked = string.Equals(state.Mode, "RGB", StringComparison.OrdinalIgnoreCase);
            CctModeRadioButton.IsChecked = string.Equals(state.Mode, "CCT", StringComparison.OrdinalIgnoreCase);
            SceneModeRadioButton.IsChecked = string.Equals(state.Mode, "Scene", StringComparison.OrdinalIgnoreCase);
            if (RgbModeRadioButton.IsChecked != true && CctModeRadioButton.IsChecked != true && SceneModeRadioButton.IsChecked != true)
            {
                RgbModeRadioButton.IsChecked = true;
            }

            RgbGroupBox.Visibility = RgbModeRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            CctGroupBox.Visibility = CctModeRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            SceneGroupBox.Visibility = SceneModeRadioButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            UpdateColorPreview();
            ToneValueTextBlock.Text = $"{GetTone()} K";
            BrightnessValueTextBlock.Text = $"{GetBrightness():0}%";
        }
        finally
        {
            _isUpdatingControls = false;
        }
    }

    private string GetCurrentModeName()
    {
        if (SceneModeRadioButton.IsChecked == true)
        {
            return "Scene";
        }

        if (CctModeRadioButton.IsChecked == true)
        {
            return "CCT";
        }

        return "RGB";
    }

    private static NeewerLightGroupConfig ToGroupConfig(NeewerLightGroupViewModel group)
    {
        return new NeewerLightGroupConfig
        {
            Name = group.Name,
            LightIds = group.LightIds.ToList(),
            State = ToStateConfig(group.State)
        };
    }

    private static NeewerLightStateConfig ToStateConfig(NeewerLightStateViewModel state)
    {
        return new NeewerLightStateConfig
        {
            Mode = state.Mode,
            Brightness = state.Brightness,
            Red = state.Red,
            Green = state.Green,
            Blue = state.Blue,
            Tone = state.Tone,
            SceneName = state.SceneName
        };
    }

    private static NeewerLightStateViewModel ToStateViewModel(NeewerLightStateConfig state)
    {
        return new NeewerLightStateViewModel
        {
            Mode = state.Mode,
            Brightness = state.Brightness,
            Red = state.Red,
            Green = state.Green,
            Blue = state.Blue,
            Tone = state.Tone,
            SceneName = state.SceneName
        };
    }

    private byte GetRed()
    {
        return (byte)Math.Clamp((int)Math.Round(RedSlider.Value), 0, 255);
    }

    private byte GetGreen()
    {
        return (byte)Math.Clamp((int)Math.Round(GreenSlider.Value), 0, 255);
    }

    private byte GetBlue()
    {
        return (byte)Math.Clamp((int)Math.Round(BlueSlider.Value), 0, 255);
    }

    private int GetTone()
    {
        return Math.Clamp((int)Math.Round(ToneSlider.Value), 3200, 5600);
    }

    private float GetBrightness()
    {
        return (float)Math.Clamp(BrightnessSlider.Value, 0, 100);
    }

    private void SetBusy(bool isBusy, string? message = null)
    {
        ScanButton.IsEnabled = !isBusy;
        ConnectButton.IsEnabled = !isBusy;
        DisconnectButton.IsEnabled = !isBusy;
        HeaderStatusTextBlock.Text = message ?? "Ready.";
    }

    protected override async void OnClosed(EventArgs e)
    {
        await SaveLastStateAsync().ConfigureAwait(true);
        await _streamDeckServer.DisposeAsync().ConfigureAwait(true);
        base.OnClosed(e);
    }
}
