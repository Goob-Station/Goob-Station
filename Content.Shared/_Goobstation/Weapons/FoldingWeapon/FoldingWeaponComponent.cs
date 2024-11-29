using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.FoldingWeapon;

/// <summary>
/// Prevents shooting and wielding the weapon if it is toggled off
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FoldingWeaponComponent : Component
{
}
