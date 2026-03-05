using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// Improves attack rate of melee weapon.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WeaponUpgradeSpeedComponent : Component
{
    [DataField]
    public float? BonusAttackRate;

    [DataField]
    public float? AttackRateMultiplier;
}
