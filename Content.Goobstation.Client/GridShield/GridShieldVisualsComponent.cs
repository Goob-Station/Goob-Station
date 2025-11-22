namespace Content.Goobstation.Client.GridShield;

[RegisterComponent]
public sealed partial class GridShieldVisualsComponent : Component
{
    [DataField]
    public Color StartColor = Color.Aquamarine;

    [DataField]
    public Color EndColor = Color.DarkRed;
}
