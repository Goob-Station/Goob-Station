using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Melee.Components;

/// <summary>
/// An upgrade for increasing gun's projectile damage.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WeaponUpgradeDamageComponent : Component
{
    [DataField]
    public DamageSpecifier? BonusDamage;

    /// <summary>
    /// How much should we multiply the total projectile's damage.
    /// </summary>
    [DataField]
    public float Modifier = 1f;
}
