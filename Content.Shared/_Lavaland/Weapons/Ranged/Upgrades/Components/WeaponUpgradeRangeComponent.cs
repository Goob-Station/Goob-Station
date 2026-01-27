using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// Upgrades the range of a melee weapon.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WeaponUpgradeRangeComponent : Component
{
    [DataField]
    public float? BonusRange;

    [DataField]
    public float? RangeMultiplier;
}
