using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared.Damage.Systems;

public sealed partial class DamageableSystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private EntityQuery<BodyComponent> _bodyQuery;
    private EntityQuery<BodyPartComponent> _bodyPartQuery;
    private EntityQuery<ConsciousnessComponent> _consciousnessQuery;
    private EntityQuery<WoundableComponent> _woundableQuery;

    /// <summary>
    /// These damages are always only dealt to vital body parts.
    /// Todo: This should not be here!
    /// </summary>
    private static readonly ProtoId<DamageGroupPrototype>[] VitalOnlyDamageTypes = [
        "Airloss",
        "Toxin",
        "Genetic",
        "Metaphysical"
    ];

    /// <summary>
    ///     [Woundmed]
    ///     Updates the parent entity's damage values by summing damage from all body parts.
    ///     Should be called after damage is applied to any body part.
    /// </summary>
    /// <param name="bodyPartUid">
    ///     The <see cref="Entity{BodyPartComponent}"/>  that receives damage
    /// </param>
    /// <param name="appliedDamage">
    ///     The <see cref="DamageSpecifier?"/> to apply to the body part
    /// </param>
    /// <param name="interruptsDoAfters">
    ///     Whether this damage change interrupts do-afters
    /// </param>
    /// <param name="origin">
    ///     The <see cref="EntityUid?"=> that caused the damage
    /// </param>
    /// <param name="ignoreBlockers">
    /// Whether to ignore damage blockers
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if parent damage was updated, <see langword="false"/> otherwise.
    /// </returns>
    private bool UpdateWoundableBodyDamage(
        Entity<BodyComponent, DamageableComponent> body,
        bool interruptsDoAfters,
        EntityUid? origin,
        bool ignoreBlockers = false)
    {
        // Reset the parent's damage values
        foreach (var type in body.Comp2.Damage.DamageDict.Keys.ToList())
            body.Comp2.Damage.DamageDict[type] = FixedPoint2.Zero;

        // Sum up damage from all body parts
        foreach (var (partId, _) in _body.GetBodyChildren(body))
        {
            if (!_damageableQuery.TryComp(partId, out var partDamageable))
            {
                DebugTools.Assert(
                    $"When updating body damage, BodyPart {ToPrettyString(partId)} had no DamageableComponent!"
                );
                continue;
            }

            foreach (var (type, value) in partDamageable.Damage.DamageDict)
            {
                if (value == 0)
                    continue;

                if (body.Comp2.Damage.DamageDict.TryGetValue(type, out var existing))
                    body.Comp2.Damage.DamageDict[type] = existing + value;
            }
        }

        return true;
    }

    /// <summary>
    ///     [Woundmed]
    ///     Gets a List[BodyPartComponent, DamageableComponent]
    ///     from an Entity[BodyComponent] using a <see cref="TargetBodyPart"/> as a mask. 
    /// </summary>
    /// <param name="body"></param>
    /// <param name="mask"></param>
    /// <returns></returns>
    private List<Entity<BodyPartComponent, DamageableComponent>> ExtractDamageableBodyPartsFromMask(Entity<BodyComponent> body, TargetBodyPart mask)
    {
        // Extract only the body parts that are targeted in the bitmask
        var extracted = new List<Entity<BodyPartComponent, DamageableComponent>>();

        // Get only the primitive flags (powers of 2) - these are the actual individual body parts
        var primitiveFlags = Enum.GetValues<TargetBodyPart>()
            .Where(flag => flag != 0 && (flag & (flag - 1)) == 0) // Power of 2 check
            .ToList();

        foreach (var flag in primitiveFlags)
        {
            // Check if this specific flag is set in our targetPart bitmask
            if (mask.HasFlag(flag))
            {
                var query = _body.ConvertTargetBodyPart(flag);
                var parts = _body.GetBodyChildrenOfTypeWithComponent<DamageableComponent>(body, query.Type,
                    symmetry: query.Symmetry).ToList();

                // Todo update above API to support Entity<T> instead of that stuff
                // just gonna not bother rn cuz nubody needs to be ported anyway when refactoring
                foreach (var (a, b, c) in parts)
                    extracted.Add((a, b, c));
            }
        }

        // If we couldn't find any of the targeted parts, fall back to all body parts
        if (extracted.Count == 0)
        {
            var query = _body.GetBodyChildrenWithComponent<DamageableComponent>(body).ToList();

            foreach (var (a, b, c) in query)
                extracted.Add((a, b, c));
        }

        return extracted;
    }

    /// <summary>
    ///     [Woundmed]
    ///     This cursed shitcode lets us know if the target part is a power of 2
    ///     therefore having multiple parts targeted.
    /// </summary>
    /// <returns></returns>
    private bool IsMultipleSelected(TargetBodyPart? a)
    {
        return a != null && a != 0 && (a & (a - 1)) != 0;
    }

    /// <summary>
    ///     [Woundmed]
    ///     Applies damage to an entity with body parts, targeting specific parts as needed.
    /// </summary>
    private DamageSpecifier? ApplyDamageToBodyParts(
        Entity<BodyComponent> body,
        DamageSpecifier damage,
        EntityUid? origin,
        bool ignoreResistances,
        bool interruptsDoAfters,
        TargetBodyPart? targetPart,
        float partMultiplier,
        bool ignoreBlockers = false,
        SplitDamageBehavior splitDamageBehavior = SplitDamageBehavior.Split,
        bool canMiss = true)
    {
        DamageSpecifier? totalAppliedDamage = null;
        var adjustedDamage = damage * partMultiplier;

        if (IsMultipleSelected(targetPart))
        {
            var targettedParts = ExtractDamageableBodyPartsFromMask(body, targetPart ?? TargetBodyPart.All);

            if (targettedParts.Count <= 0)
            {
                Log.Error(
                    $"Couldn't find any body parts for Body {ToPrettyString(body)} when applying damage!"
                );
                return null;
            }

            var damagePerPart = ApplySplitDamageBehaviors(splitDamageBehavior, adjustedDamage, targettedParts);
            var appliedDamage = new DamageSpecifier();
            var surplusHealing = new DamageSpecifier();
            for (var i = 0; i < targettedParts.Count; i++)
            {
                var part = targettedParts[i];
                var modifiedDamage = damagePerPart;
                modifiedDamage += surplusHealing;

                // Apply damage to this part
                var partDamageResult = ChangeDamage(
                    (part, part.Comp2),
                    modifiedDamage,
                    ignoreResistances,
                    interruptsDoAfters,
                    origin,
                    ignoreGlobalModifiers: false
                );

                if (partDamageResult != null && !partDamageResult.Empty)
                {
                    appliedDamage += partDamageResult;

                    /*
                        Why this ugly shitcode? Its so that we can track chems and other sorts of healing surpluses.
                        Assume you're fighting in a spaced area. Your chest has 30 damage, and every other part
                        is getting 0.5 per tick. Your chems will only be 1/11th as effective, so we take the surplus
                        healing and pass it along parts. That way a chem that would heal you for 75 brute would truly
                        heal the 75 brute per tick, and not some weird shit like 6.8 per tick.
                    */
                    foreach (var (type, damageFromDict) in modifiedDamage.DamageDict)
                    {
                        if (damageFromDict >= 0
                            || !partDamageResult.DamageDict.TryGetValue(type, out var damageFromResult)
                            || damageFromResult > 0)
                            continue;

                        // If the damage from the dict plus the surplus healing is equal to the damage from the result,
                        // we can safely set the surplus healing to 0, as that means we consumed all of it.
                        if (damageFromDict >= damageFromResult)
                        {
                            surplusHealing.DamageDict[type] = FixedPoint2.Zero;
                        }
                        else
                        {
                            if (surplusHealing.DamageDict.TryGetValue(type, out var _))
                                surplusHealing.DamageDict[type] = damageFromDict - damageFromResult;
                            else
                                surplusHealing.DamageDict.TryAdd(type, damageFromDict - damageFromResult);
                        }
                    }
                }
            }

            totalAppliedDamage = appliedDamage;
        }
        else
        {
            // Target a specific body part
            TargetBodyPart? target;
            var totalDamage = damage.GetTotal();

            if (totalDamage <= 0 || !canMiss) // Whoops i think i fucked up damage here.
                target = _body.GetTargetBodyPart(body, origin, targetPart);
            else
                target = _body.GetRandomBodyPart(body, origin, targetPart);

            var (partType, symmetry) = _body.ConvertTargetBodyPart(target);
            var possibleTargets = _body.GetBodyChildrenOfType(body, partType, symmetry: symmetry).ToList();

            if (possibleTargets.Count == 0)
            {
                if (totalDamage <= 0)
                    return null;

                possibleTargets = _body.GetBodyChildren(body).ToList();
            }

            // No body parts at all?
            if (possibleTargets.Count == 0)
            {
                Log.Error(
                    $"Couldn't find any possible target body parts for Body {ToPrettyString(body)} when applying damage!"
                );
                return null;
            }

            var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(body));
            var chosenTarget = random.PickAndTake(possibleTargets);

            if (!_damageableQuery.TryComp(chosenTarget.Id, out var partDamageable))
                return null;

            totalAppliedDamage = ChangeDamage((chosenTarget.Id, partDamageable), adjustedDamage, ignoreResistances,
                interruptsDoAfters, origin, ignoreBlockers: ignoreBlockers);
        }

        if (_damageableQuery.TryComp(body, out var damageableComp))
            OnEntityDamageChanged((body, damageableComp), totalAppliedDamage, interruptsDoAfters, origin);

        return totalAppliedDamage;
    }

    /// <summary>
    ///     [Woundmed]
    ///     Applies damage to a complex body.
    /// </summary>
    /// <returns>
    ///     <see cref="DamageSpecifier?"/> of the vital damage dealt.
    /// </returns>
    private DamageSpecifier ApplyDamageComplex(
        Entity<BodyComponent> body,
        DamageSpecifier damage,
        bool ignoreResistances = false,
        bool interruptsDoAfters = true,
        EntityUid? origin = null,
        bool ignoreGlobalModifiers = false, // TODO SHITMED : this is not handled for complex bodies
        float partMultiplier = 1f,
        TargetBodyPart? targetPart = null,
        bool ignoreBlockers = false,
        SplitDamageBehavior splitDamage = SplitDamageBehavior.Split,
        bool canMiss = true
    )
    {
        var vitalDamage = new DamageSpecifier();
        var regularDamage = new DamageSpecifier();

        // separate the vital damage so we have
        // vitalDamage for damage that must be applied to vital body parts (TargetBodyPart.Vital) &
        // regularDamage for damage that can be regularly applied to the target part
        foreach (var (type, value) in damage.DamageDict)
        {
            if (VitalOnlyDamageTypes.Contains(type))
                vitalDamage.DamageDict[type] = value;
            else
                regularDamage.DamageDict[type] = value;
        }

        var appliedRegularDamage = regularDamage.Empty
            ? null
            : ApplyDamageToBodyParts(
                body,
                regularDamage,
                origin,
                ignoreResistances,
                interruptsDoAfters,
                targetPart,
                partMultiplier,
                ignoreBlockers,
                splitDamage,
                canMiss
            );

        var appliedVitalDamage = vitalDamage.Empty
            ? null
            : ApplyDamageToBodyParts(
                body,
                vitalDamage,
                origin,
                ignoreResistances,
                interruptsDoAfters,
                TargetBodyPart.Vital,
                partMultiplier,
                ignoreBlockers,
                splitDamage,
                canMiss
            );

        var totalDamage = new DamageSpecifier();

        if (appliedRegularDamage is not null)
            totalDamage += appliedRegularDamage;
        if (appliedVitalDamage is not null)
            totalDamage += appliedVitalDamage;

        if (!_damageableQuery.TryComp(body, out var damageableComp))
        {
            DebugTools.Assert("Applied damage to complex body without DamageableComponent!");
        }
        else
        {
            UpdateWoundableBodyDamage(
                (body, body.Comp, damageableComp),
                interruptsDoAfters,
                origin,
                ignoreBlockers
            );
        }

        return totalDamage;
    }

    /// <summary>
    ///     [Woundmed]
    ///     Applies the provided <see cref="SplitDamageBehavior"/> onto the given body parts with the provided <see cref="DamageSpecifier"/>  
    /// </summary>
    /// <param name="splitDamageBehavior"></param>
    /// <param name="damage"></param>
    /// <param name="parts"></param>
    /// <returns>
    ///     The <see cref="DamageSpecifier"=> that should be equally dealt to each provided body part.
    /// </returns>
    public DamageSpecifier ApplySplitDamageBehaviors(SplitDamageBehavior splitDamageBehavior,
        DamageSpecifier damage,
        List<Entity<BodyPartComponent, DamageableComponent>> parts)
    {
        var newDamage = new DamageSpecifier(damage);
        switch (splitDamageBehavior)
        {
            case SplitDamageBehavior.None:
                return newDamage;
            case SplitDamageBehavior.Split:
                return newDamage / parts.Count;
            case SplitDamageBehavior.SplitExplosion:
                var vitalParts = parts.Where(part =>
                TryComp<ConsciousnessRequiredComponent>(part, out var consciousness)
                && consciousness.CausesDeath == true).ToList();

                return newDamage / vitalParts.Count;
            case SplitDamageBehavior.SplitEnsureAllDamaged:
                var damagedParts = parts.Where(part =>
                    part.Comp2.TotalDamage > FixedPoint2.Zero).ToList();

                parts.Clear();
                parts.AddRange(damagedParts);

                goto case SplitDamageBehavior.SplitEnsureAll;
            case SplitDamageBehavior.SplitEnsureAllOrganic:
                var organicParts = parts.Where(part =>
                    part.Comp1.PartComposition == BodyPartComposition.Organic).ToList();

                parts.Clear();
                parts.AddRange(organicParts);

                goto case SplitDamageBehavior.SplitEnsureAll;
            case SplitDamageBehavior.SplitEnsureAllDamagedAndOrganic:
                var compatableParts = parts.Where(part =>
                    part.Comp2.TotalDamage > FixedPoint2.Zero &&
                    part.Comp1.PartComposition == BodyPartComposition.Organic).ToList();

                parts.Clear();
                parts.AddRange(compatableParts);
                goto case SplitDamageBehavior.SplitEnsureAll;
            case SplitDamageBehavior.SplitEnsureAll:
                foreach (var (type, val) in newDamage.DamageDict)
                {
                    if (val > 0)
                    {
                        if (parts.Count > 0)
                            newDamage.DamageDict[type] = val / parts.Count;
                        else
                            newDamage.DamageDict[type] = FixedPoint2.Zero;
                    }
                    else if (val < 0)
                    {
                        var count = 0;

                        foreach (var (id, _, damageable) in parts)
                            if (damageable.Damage.DamageDict.TryGetValue(type, out var currentDamage)
                                && currentDamage > 0)
                                count++;

                        if (count > 0)
                            newDamage.DamageDict[type] = val / count;
                        else
                            newDamage.DamageDict[type] = FixedPoint2.Zero;
                    }
                }
                // We sort the parts to ensure that surplus damage gets passed from least to most damaged.
                parts.Sort((a, b) => a.Comp2.TotalDamage.CompareTo(b.Comp2.TotalDamage));
                return newDamage;
            default:
                return damage;
        }
    }

    /// <summary>
    /// [Woundmed]
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="newValue"></param>
    private void SetAllWoundDamage(Entity<DamageableComponent> ent, FixedPoint2 newValue)
    {
        // If entity has a body, set damage on all body parts
        if (_bodyQuery.HasComp(ent))
        {
            foreach (var (part, _) in _body.GetBodyChildren(ent))
            {
                if (!_damageableQuery.TryComp(part, out var partDamageable))
                    continue;

                // I LOVE RECURSION!!!
                SetAllDamage((part, partDamageable), newValue);
            }
        }

        // Update cached values
        ent.Comp.Damage.GetDamagePerGroup(_prototypeManager, ent.Comp.DamagePerGroup);
        ent.Comp.TotalDamage = ent.Comp.Damage.GetTotal();

        if (_woundableQuery.TryComp(ent, out var woundable))
        {
            _wounds.UpdateWoundableIntegrity(ent, woundable);

            // Create wounds if damage was applied
            if (newValue > 0 && woundable.AllowWounds)
            {
                foreach (var (type, value) in ent.Comp.Damage.DamageDict)
                {
                    _wounds.TryInduceWound(ent,
                        type,
                        value * ent.Comp.Damage.WoundSeverityMultipliers.GetValueOrDefault(type, 1),
                        out _,
                        woundable);
                }
            }
        }
    }
}