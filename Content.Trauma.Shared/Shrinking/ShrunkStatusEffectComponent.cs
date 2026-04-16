using Content.Shared.Damage.Prototypes;

namespace Content.Trauma.Shared.Shrinking;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShrunkStatusEffectComponent : Component
{
    [DataField]
    public ProtoId<DamageModifierSetPrototype> DamageModifierSet = new ("DevilDealNegative");

    [DataField]
    public ProtoId<DamageModifierSetPrototype>? OriginalDamageModifierSet;
}
