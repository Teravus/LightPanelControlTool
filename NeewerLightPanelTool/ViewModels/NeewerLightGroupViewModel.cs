namespace NeewerLightPanelTool.ViewModels;

public sealed class NeewerLightGroupViewModel : ObservableObject
{
    private string _status;

    public NeewerLightGroupViewModel(string name, IReadOnlyList<string> lightIds)
    {
        Name = name;
        LightIds = lightIds;
        _status = $"{lightIds.Count} light(s)";
    }

    public string Name { get; }

    public IReadOnlyList<string> LightIds { get; }

    public NeewerLightStateViewModel State { get; set; } = new();

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
}
