using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ImmortalSnail;

/// <summary>
/// Marker component for targets that were smited.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ImmortalSnailSmitedComponent : Component
{
    /// <summary>
    /// The spawner entity that created the snail, tracked so we can delete if the target dies before snail spawns.
    /// </summary>
    [DataField]
    public EntityUid? SnailSpawner;

    /// <summary>
    /// The snail entity tracking this target, tracked so we can delete it when the target dies.
    /// </summary>
    [DataField]
    public EntityUid? SnailEntity;
}
