


namespace Content.Goobstation.Server.SpeakFont;
[RegisterComponent]
public sealed partial class SpeakFontComponent : Component
{
    [DataField("fontid")]
    public string? FontId;
    [DataField("fontsize")]
    public int? FontSize;
    [DataField("color")]
    public Color? Color;

}