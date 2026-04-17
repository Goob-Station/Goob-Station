namespace Content.Goobstation.Server.SpeakFont;

[RegisterComponent]
public sealed partial class SpeakFontOverrideComponent : Component
{
    [DataField]
    public bool Enabled = false;

    [DataField]
    public string? FontId;

    [DataField]
    public int? FontSize;

    [DataField]
    public Color? Color;

}