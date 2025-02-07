using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.EntityEffects.EffectConditions;
public sealed partial class TypedDamageThreshold : EntityEffectCondition
{
    /// <summary>
    /// We're checking for at least *this* amount of damage, but only for assigned types/groups
    /// If we have less, this condition is false, unless Inverse is true
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    [DataField]
    public bool Inverse = false;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (args.EntityManager.TryGetComponent<DamageableComponent>(args.TargetEntity, out var damage))
        {
            var comparison = Damage;
            comparison.ExclusiveAdd(-damage.Damage);
            comparison = -comparison;
            return comparison.AnyPositive() ^ Inverse;
        }
        return false;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return "";
    }
}
