namespace Content.Shared._Goobstation.Weapons.Ranged;

/// <summary>
///     Component that allows guns to instantly inject all the contents of any syringe on a target.
/// </summary>
[RegisterComponent]
public sealed partial class SyringeGunComponent : Component
{
    [DataField]
    public bool PierceArmor;
}
