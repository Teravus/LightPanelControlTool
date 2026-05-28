using InTheHand.Bluetooth;
using NeewerLightControlBT;
using NeewerLightPanelTool.ViewModels;

namespace NeewerLightPanelTool.Services;

public sealed class NeewerLightDiscoveryService
{
    private static readonly Guid NEEWER_SERVICE_ID = new("69400001-B5A3-F393-E0A9-E50E24DCCA99");
    private readonly Dictionary<string, NeewerLight> _lights = new(StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<NeewerLightDeviceViewModel>> ScanAsync()
    {
        RequestDeviceOptions options = new()
        {
            AcceptAllDevices = true
        };
        options.OptionalServices.Add(NEEWER_SERVICE_ID);

        IReadOnlyCollection<BluetoothDevice> devices = await Bluetooth.ScanForDevicesAsync(options).ConfigureAwait(false);
        List<NeewerLightDeviceViewModel> results = [];

        foreach (BluetoothDevice device in devices.OrderBy(device => device.Name).ThenBy(device => device.Id))
        {
            if (!LooksLikeNeewerPanel(device))
            {
                continue;
            }

            string display_id = CreateDisplayId(device);
            if (!_lights.ContainsKey(display_id))
            {
                NeewerLight light = new()
                {
                    Device = device,
                    ID = display_id
                };
                light.LookupLightDataByNameID(device.Name, device.Id);
                _lights.Add(display_id, light);
            }

            results.Add(new NeewerLightDeviceViewModel(display_id, device.Name, device.Id));
        }

        return results;
    }

    public async Task<bool> ConnectAsync(string displayId)
    {
        return _lights.TryGetValue(displayId, out NeewerLight? light) && await light.ConnectAsync().ConfigureAwait(false);
    }

    public async Task DisconnectAsync(string displayId)
    {
        if (_lights.TryGetValue(displayId, out NeewerLight? light))
        {
            await light.DisconnectAsync().ConfigureAwait(false);
        }
    }

    public async Task ExecuteAsync(IEnumerable<string> displayIds, NeewerLightInstruction instruction)
    {
        foreach (string display_id in displayIds)
        {
            if (_lights.TryGetValue(display_id, out NeewerLight? light))
            {
                await light.ExecuteLightInstruction(instruction).ConfigureAwait(false);
            }
        }
    }

    private static bool LooksLikeNeewerPanel(BluetoothDevice device)
    {
        return ContainsNeewerMarker(device.Name) || ContainsNeewerMarker(device.Id);
    }

    private static bool ContainsNeewerMarker(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains("NEEWER", StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateDisplayId(BluetoothDevice device)
    {
        string suffix = device.Id.Length <= 6 ? device.Id : device.Id[^6..];
        return $"{device.Name}-{suffix}";
    }
}
