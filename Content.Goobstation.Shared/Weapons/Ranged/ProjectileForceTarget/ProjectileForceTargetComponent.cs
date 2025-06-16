using Content.Shared._Shitmed.Targeting;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileForceTarget;

/// <summary>
/// Forces the projectile with this component always hit the said part (by setting the shooter's targeting to it :trollface:)
/// Can also be used to make projectiles never miss.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProjectileForceTargetComponent : Component
{
    /// <summary>
    /// Null means target does not get changed.
    /// </summary>
    [DataField]
    public TargetBodyPart? Part;

    [DataField]
    public bool MakeUnableToMiss;
}
