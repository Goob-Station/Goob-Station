namespace Content.Goobstation.Server.PlayerListener;

[RegisterComponent]
public sealed partial class DormNotifierMarkerComponent : Component
{
    [DataField]
    public string Name = "";

    /// <summary>
    /// Tile range to check for players
    /// </summary>
    [DataField]
    public float ProximityRadius = 1;
}
