using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// A <see cref="GunUpgradeComponent"/> for increasing the speed of a gun's projectile.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeSpeedComponent : Component
{
    [DataField]
    public float Coefficient = 1;
}
