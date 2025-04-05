using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Traps;

[RegisterComponent]
public sealed partial class DamageTrapComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    [DataField]
    public EntProtoId? SpawnedEntity;
}
