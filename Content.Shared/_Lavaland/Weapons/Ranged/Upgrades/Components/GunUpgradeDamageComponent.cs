using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// A <see cref="GunUpgradeComponent"/> for increasing the damage of a gun's projectile.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeDamageComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// How much of damage applies if the weapon shoots pellets (shotgun)
    /// </summary>
    [DataField]
    public float PelletModifier = 1f;
}
