namespace NeewerLightPanelTool.ViewModels;

public sealed class NeewerLightStateViewModel
{
    public string Mode { get; set; } = "RGB";

    public float Brightness { get; set; } = 39;

    public byte Red { get; set; } = 255;

    public byte Green { get; set; }

    public byte Blue { get; set; }

    public int Tone { get; set; } = 4500;

    public string SceneName { get; set; } = string.Empty;

    public NeewerLightStateViewModel Clone()
    {
        return new NeewerLightStateViewModel
        {
            Mode = Mode,
            Brightness = Brightness,
            Red = Red,
            Green = Green,
            Blue = Blue,
            Tone = Tone,
            SceneName = SceneName
        };
    }
}
