using Content.Shared._Shitmed.Targeting;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileForceTarget;

/// <summary>
/// Forces the projectile with this component always hit the said part (by setting the shooter's targeting to it :trollface:).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ProjectileForceTargetComponent : Component
{
    [DataField]
    public TargetBodyPart Part = TargetBodyPart.Chest;
}
