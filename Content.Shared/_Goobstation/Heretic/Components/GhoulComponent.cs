using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Heretic;

[RegisterComponent, NetworkedComponent]
public sealed partial class GhoulComponent : Component
{
    /// <summary>
    ///     Indicates who ghouled the entity.
    /// </summary>
    [DataField] public EntityUid? BoundHeretic = null;

    /// <summary>
    ///     Total health for ghouls.
    /// </summary>
    [DataField] public FixedPoint2 TotalHealth = 50;
}
