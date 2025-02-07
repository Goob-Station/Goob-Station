using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.EntityEffects.EffectConditions;

public sealed partial class TypedDamageThreshold : EntityEffectCondition
{
    /// <summary>
    /// We're checking for at least *this* amount of damage, but only for assigned types/groups
    /// If we have less, this condition is false, unless Inverse is true
    /// </summary>
    /// <remarks>
    /// Engine splits damage groups across types, we greedily revert that split to create behaviour
    /// closer to what user expects; any damage in specified group contributes to that group total.
    /// Use multiple conditions if you want to explicitly avoid that behaviour, or don't use all
    /// damage types within a group when specifying prototypes
    /// </remarks>
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    [DataField]
    public bool Inverse = false;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (args.EntityManager.TryGetComponent<DamageableComponent>(args.TargetEntity, out var damage))
        {
            var protoManager = IoCManager.Resolve<IPrototypeManager>();
            var comparison = new DamageSpecifier();
            comparison += Damage;
            // Greedily revert the split and check; Quickly skip when not relevant
            FixedPoint2 lowestDamage = default!;
            foreach (var group in protoManager.EnumeratePrototypes<DamageGroupPrototype>())
            {
                lowestDamage = FixedPoint2.MaxValue;
                foreach (var damageType in group.DamageTypes)
                {
                    if (comparison.DamageDict.TryGetValue(damageType, out var value))
                        lowestDamage = value < lowestDamage ? value : lowestDamage;
                    else
                    {
                        lowestDamage = FixedPoint2.Zero;
                        break;
                    }
                }
                if (lowestDamage == FixedPoint2.MaxValue || lowestDamage == FixedPoint2.Zero)
                    continue;
                var groupDamage = lowestDamage * group.DamageTypes.Count;
                if (damage.Damage.TryGetDamageInGroup(group, out var total) && total > groupDamage)
                    return !Inverse;
                // we finished comparing this group, remove future interferences
                foreach (var damageType in group.DamageTypes)
                {
                    comparison.DamageDict[damageType] -= lowestDamage;
                    comparison.DamageDict[damageType] -= 0.01; // limit floating point errors
                    comparison.ClampMin(0);
                    comparison.TrimZeros();
                }
            }
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
