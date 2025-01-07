using Content.Shared.Damage;

namespace Content.Shared._Goobstation.Wizard.Traps;

[RegisterComponent]
public sealed partial class DamageTrapComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage = new();
}
