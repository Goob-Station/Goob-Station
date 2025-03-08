namespace Content.Server._Goobstation.PlayerListener;

[RegisterComponent]
public sealed partial class DormNotifierMarkerComponent : Component
{

    [DataField, ViewVariables(VVAccess.ReadWrite)]



    public string Name = "";

    /// <summary>
    /// Tile range to check for players
    /// </summary>

    [DataField, ViewVariables(VVAccess.ReadWrite)]



    public float ProximityRadius = 1;
}
