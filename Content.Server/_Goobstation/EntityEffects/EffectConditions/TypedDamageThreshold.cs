using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Localizations;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.EntityEffects.EffectConditions;

/// <summary>
/// Checking for at least this amount of damage, but only for specified types/groups
/// If we have less, this condition is false. Inverse flips the output boolean
/// </summary>
/// <remarks>
/// DamageSpecifier splits damage groups across types, we greedily revert that split to create
/// behaviour closer to what user expects; any damage in specified group contributes to that
/// group total. Use multiple conditions if you want to explicitly avoid that behaviour,
/// or don't use damage types within a group when specifying prototypes.
/// </remarks>
public sealed partial class TypedDamageThreshold : EntityEffectCondition
{
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    [DataField]
    public bool Inverse = false;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (args.EntityManager.TryGetComponent<DamageableComponent>(args.TargetEntity, out var damage))
        {
            var protoManager = IoCManager.Resolve<IPrototypeManager>();
            var comparison = new DamageSpecifier(Damage);
            foreach (var group in protoManager.EnumeratePrototypes<DamageGroupPrototype>())
            {
                // Greedily revert the split and check; Quickly skip when not relevant
                var lowestDamage = FixedPoint2.MaxValue;
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
                if (MathF.Abs(groupDamage.Float() - MathF.Round(groupDamage.Float())) < 0.02)
                    groupDamage = MathF.Round(groupDamage.Float()); // otherwise brutes split unevenly
                if (damage.Damage.TryGetDamageInGroup(group, out var total) && total > groupDamage)
                    return !Inverse;
                // we finished comparing this group, remove future interferences
                foreach (var damageType in group.DamageTypes)
                {
                    comparison.DamageDict[damageType] -= lowestDamage;
                    // not a fan, but it's needed
                    if (MathF.Abs(comparison.DamageDict[damageType].Float()
                        - MathF.Round(comparison.DamageDict[damageType].Float()))
                        < 0.02)
                        comparison.DamageDict[damageType] = MathF.Round(comparison.DamageDict[damageType].Float());
                }
                comparison.ClampMin(0);
                comparison.TrimZeros();
            }
            comparison.ExclusiveAdd(-damage.Damage);
            comparison = -comparison;
            return comparison.AnyPositive() ^ Inverse;
        }
        return false;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        var damages = new List<string>();
        var comparison = new DamageSpecifier(Damage);
        foreach (var group in prototype.EnumeratePrototypes<DamageGroupPrototype>())
        {
            var lowestDamage = FixedPoint2.MaxValue;
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
            if (MathF.Abs(groupDamage.Float() - MathF.Round(groupDamage.Float())) < 0.02)
                groupDamage = MathF.Round(groupDamage.Float());
            if (groupDamage > 0)
                damages.Add(
                Loc.GetString("health-change-display",
                    ("kind", group.LocalizedName),
                    ("amount", MathF.Abs(groupDamage.Float())),
                    ("deltasign", 1))
                );
            foreach (var damageType in group.DamageTypes)
            {
                comparison.DamageDict[damageType] -= lowestDamage;
                if (MathF.Abs(comparison.DamageDict[damageType].Float()
                        - MathF.Round(comparison.DamageDict[damageType].Float()))
                        < 0.02)
                    comparison.DamageDict[damageType] = MathF.Round(comparison.DamageDict[damageType].Float());
            }
            comparison.ClampMin(0);
            comparison.TrimZeros();
        }

        foreach (var (kind, amount) in comparison.DamageDict)
        {
            damages.Add(
                Loc.GetString("health-change-display",
                    ("kind", prototype.Index<DamageTypePrototype>(kind).LocalizedName),
                    ("amount", MathF.Abs(amount.Float())),
                    ("deltasign", 1))
                );
        }

        return Loc.GetString("reagent-effect-condition-guidebook-typed-damage-threshold",
                ("inverse", Inverse),
                ("changes", ContentLocalizationManager.FormatList(damages))
                );
    }
}
