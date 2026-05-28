namespace NeewerLightPanelTool.Services;

public sealed class NeewerLightPanelConfig
{
    public string InterfaceIp { get; set; } = "127.0.0.1";

    public int HttpPort { get; set; } = 5088;

    public bool HttpServerWasRunning { get; set; }

    public List<NeewerLightGroupConfig> Groups { get; set; } = [];
}

public sealed class NeewerLightGroupConfig
{
    public string Name { get; set; } = string.Empty;

    public List<string> LightIds { get; set; } = [];

    public NeewerLightStateConfig State { get; set; } = new();
}

public sealed class NeewerLightStateConfig
{
    public string Mode { get; set; } = "RGB";

    public float Brightness { get; set; } = 39;

    public byte Red { get; set; } = 255;

    public byte Green { get; set; }

    public byte Blue { get; set; }

    public int Tone { get; set; } = 4500;

    public string SceneName { get; set; } = string.Empty;
}
