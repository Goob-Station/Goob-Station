namespace Content.Goobstation.Shared.Weapons.CounterattackWeapon;

[RegisterComponent]
public sealed partial class CounterattackWeaponUserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> Weapons = [];
}
