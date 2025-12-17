using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Doodons;

/// <summary>
/// Marks an entity as a Doodon building.
/// Stores Town Hall ownership and whether it is active.
/// </summary>
[RegisterComponent]
public sealed partial class DoodonBuildingComponent : Component
{
    /// <summary>
    /// Town Hall this building belongs to.
    /// </summary>
    [DataField]
    public EntityUid? TownHall;

    /// <summary>
    /// Whether this building is currently functional.
    /// </summary>
    [DataField]
    public bool Active;
}
