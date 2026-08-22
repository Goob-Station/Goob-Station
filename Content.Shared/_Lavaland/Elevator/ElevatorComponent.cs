using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.Elevator;

/// <summary>
/// Just a portal to loads grids on the fly.
/// </summary>
[RegisterComponent]
public sealed partial class ElevatorComponent : Component
{
    /// <summary>
    /// Which beacon to teleport the player to.
    /// </summary>
    [DataField(required: true)]
    public string DestinationId = string.Empty;

    /// <summary>
    /// Map to load.
    /// </summary>
    [DataField]
    public ResPath? MapPath;
}
