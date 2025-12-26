using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Doodons;

/// <summary>
/// Marks an entity as a Doodon unit.
/// </summary>
[RegisterComponent]
public sealed partial class DoodonComponent : Component
{
    /// <summary>
    /// Town Hall this doodon belongs to.
    /// </summary>
    [DataField]
    public EntityUid? TownHall;
}
