using Content.Shared.Damage;
namespace Content.Shared._Goobstation.Bingle;

[RegisterComponent]
public sealed partial class BingleComponent : Component
{
    [DataField]
    public bool Upgraded = false;
    [DataField]
    public DamageSpecifier UpgradeDamage = default!;
    [DataField]
    public bool Prime = false;

    public EntityUid? MyPit;
}
