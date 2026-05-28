namespace NeewerLightPanelTool.ViewModels;

public sealed class NeewerLightDeviceViewModel : ObservableObject
{
    private string _status = "Found";
    private bool _isConnected;

    public NeewerLightDeviceViewModel(string displayId, string name, string bluetoothId)
    {
        DisplayId = displayId;
        Name = name;
        BluetoothId = bluetoothId;
    }

    public string DisplayId { get; }

    public string Name { get; }

    public string BluetoothId { get; }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }
}
