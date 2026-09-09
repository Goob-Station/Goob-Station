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
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Damage.Systems;

public sealed partial class DamageableSystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly IRobustRandom _LETSGOGAMBLINGEXCLAMATIONMARKEXCLAMATIONMARK = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private EntityQuery<BodyComponent> _bodyQuery;
    private EntityQuery<ConsciousnessComponent> _consciousnessQuery;
    private EntityQuery<WoundableComponent> _woundableQuery;

    private ProtoId<DamageGroupPrototype>[] _vitalOnlyDamageTypes = {"Airloss", "Toxin", "Genetic", "Metaphysical"};

    public void GoobInitialize()
    {
        _bodyQuery = GetEntityQuery<BodyComponent>();
        _consciousnessQuery = GetEntityQuery<ConsciousnessComponent>();
        _woundableQuery = GetEntityQuery<WoundableComponent>();
    }

    /// <summary>
    /// Applies damage to an entity with body parts, targeting specific parts as needed.
    /// </summary>
    private DamageSpecifier? ApplyDamageToBodyParts(
        EntityUid uid,
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
        // This cursed shitcode lets us know if the target part is a power of 2
        // therefore having multiple parts targeted.
        if (targetPart != null
            && targetPart != 0 && (targetPart & (targetPart - 1)) != 0)
        {
            // Extract only the body parts that are targeted in the bitmask
            var targetedBodyParts = new List<(EntityUid Id,
                BodyPartComponent Component,
                DamageableComponent Damageable)>();

            // Get only the primitive flags (powers of 2) - these are the actual individual body parts
            var primitiveFlags = Enum.GetValues<TargetBodyPart>()
                .Where(flag => flag != 0 && (flag & (flag - 1)) == 0) // Power of 2 check
                .ToList();

            foreach (var flag in primitiveFlags)
            {
                // Check if this specific flag is set in our targetPart bitmask
                if (targetPart.Value.HasFlag(flag))
                {
                    var query = _body.ConvertTargetBodyPart(flag);
                    var parts = _body.GetBodyChildrenOfTypeWithComponent<DamageableComponent>(uid, query.Type,
                        symmetry: query.Symmetry).ToList();

                    if (parts.Count > 0)
                        targetedBodyParts.AddRange(parts);
                }
            }

            // If we couldn't find any of the targeted parts, fall back to all body parts
            if (targetedBodyParts.Count == 0)
            {
                var query = _body.GetBodyChildrenWithComponent<DamageableComponent>(uid).ToList();
                if (query.Count > 0)
                    targetedBodyParts = query;
                else
                    return null;
            }

            // Goob edit start
            var damagePerPart = adjustedDamage;
            damagePerPart = ApplySplitDamageBehaviors(splitDamageBehavior, adjustedDamage, targetedBodyParts);
            var appliedDamage = new DamageSpecifier();
            var surplusHealing = new DamageSpecifier();
            for (var i = 0; i < targetedBodyParts.Count; i++)
            {
                var (partId, _, partDamageable) = targetedBodyParts[i];
                var modifiedDamage = damagePerPart;
                modifiedDamage += surplusHealing;
                // Goob edit end

                // Apply damage to this part
                var partDamageResult = ChangeDamage((partId, partDamageable), modifiedDamage, ignoreResistances,
                    interruptsDoAfters, origin, ignoreBlockers: ignoreBlockers);

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
                target = _body.GetTargetBodyPart(uid, origin, targetPart);
            else
                target = _body.GetRandomBodyPart(uid, origin, targetPart);

            var (partType, symmetry) = _body.ConvertTargetBodyPart(target);
            var possibleTargets = _body.GetBodyChildrenOfType(uid, partType, symmetry: symmetry).ToList();

            if (possibleTargets.Count == 0)
            {
                if (totalDamage <= 0)
                    return null;

                possibleTargets = _body.GetBodyChildren(uid).ToList();
            }

            // No body parts at all?
            if (possibleTargets.Count == 0)
                return null;

            // madeline todo note: do NOT FUCKING TELL ME WE ARE JUST RUNNING ROBUST.RANDOM IN SHARED CODE
            var chosenTarget = _LETSGOGAMBLINGEXCLAMATIONMARKEXCLAMATIONMARK.PickAndTake(possibleTargets);

            if (!_damageableQuery.TryComp(chosenTarget.Id, out var partDamageable))
                return null;

            totalAppliedDamage = ChangeDamage((chosenTarget.Id, partDamageable), adjustedDamage, ignoreResistances,
                interruptsDoAfters, origin, ignoreBlockers: ignoreBlockers);
        }

        return totalAppliedDamage;
    }

    /// <summary>
    /// Applies damage directly to an entity without routing through body parts.
    /// </summary>
    private DamageSpecifier? ApplyDamageToEntity(
        Entity<DamageableComponent> ent,
        DamageSpecifier? damage,
        bool ignoreResistances,
        bool interruptsDoAfters,
        EntityUid? origin,
        bool ignoreBlockers = false)
    {
        if (damage == null)
            return null;

        // Apply resistances
        if (!ignoreResistances)
        {
            if (ent.Comp.DamageModifierSetId != null &&
                _prototypeManager.Resolve(ent.Comp.DamageModifierSetId, out var modifierSet)) // Shitmed Change
            {
                damage = DamageSpecifier.ApplyModifierSet(damage,
                    DamageSpecifier.PenetrateArmor(modifierSet, damage.ArmorPenetration)); // Goob edit
            }

            if (TryComp(ent, out BodyPartComponent? bodyPart))
            {
                TargetBodyPart? target = _body.GetTargetBodyPart(bodyPart);
                if (bodyPart.Body != null)
                {
                    // First raise the event on the parent to apply any parent modifiers
                    var parentEv = new DamageModifyEvent(bodyPart.Body.Value, damage, origin, target);
                    RaiseLocalEvent(bodyPart.Body.Value, parentEv);
                    damage = parentEv.Damage;
                }

                // Then raise on the part itself for any part-specific modifiers
                var ev = new DamageModifyEvent(ent, damage, origin, target);
                RaiseLocalEvent(ent, ev);
                damage = ev.Damage;
            }
            else
            {
                // Not a body part, just apply modifiers normally
                var ev = new DamageModifyEvent(ent, damage, origin);
                RaiseLocalEvent(ent);
                damage = ev.Damage;
            }

            if (damage.Empty)
                return damage;
        }

        if (!ignoreResistances)
            damage = ApplyUniversalAllModifiers(damage);

        var delta = new DamageSpecifier(damage.ArmorPenetration,
            damage.PartDamageVariation,
            damage.WoundSeverityMultipliers); // Goob edit
        delta.DamageDict.EnsureCapacity(damage.DamageDict.Count);
        var dict = ent.Comp.Damage.DamageDict;

        // Check for integrity cap on body parts
        bool isWoundable = false;
        FixedPoint2? damageCap = null;
        if (_woundableQuery.TryComp(ent, out var woundable))
        {
            isWoundable = true;
            damageCap = woundable.IntegrityCap;
        }

        // Apply damage
        var currentTotalDamage = ent.Comp.TotalDamage.Float();
        FixedPoint2? remainingCap = damageCap.HasValue ? damageCap.Value - currentTotalDamage : null;

        foreach (var (type, value) in damage.DamageDict)
        {
            if (!dict.TryGetValue(type, out var oldValue))
                continue;

            // For positive damage, we need to check if we've hit the cap
            if (value > 0)
            {
                // Delta ignores this stuff since we need it for effects.
                delta.DamageDict[type] = value;

                // If we're not a woundable or we don't have a cap, apply the damage normally
                if (!isWoundable
                    || remainingCap is null)
                {
                    dict[type] = oldValue + value;
                    continue;
                }

                // If we've already hit the cap, skip this damage type
                if (remainingCap.Value <= 0)
                    continue;

                // Calculate how much of this damage type we can apply
                var damageToApply = FixedPoint2.Min(value, remainingCap.Value);
                var newValue = FixedPoint2.Max(FixedPoint2.Zero, oldValue + damageToApply);

                // Update remaining cap
                remainingCap -= damageToApply;

                // Only update the dict if the value actually changed
                if (newValue != oldValue)
                    dict[type] = newValue;
            }
            else
            {
                // For negative damage (healing), apply normally
                var newValue = FixedPoint2.Max(FixedPoint2.Zero, oldValue + value);
                if (newValue != oldValue)
                {
                    dict[type] = newValue;
                    delta.DamageDict[type] = newValue - oldValue;
                }
            }
        }

        OnEntityDamageChanged(ent, delta, interruptsDoAfters, origin, ignoreBlockers, damage);

        // This means that the damaged part was a woundable
        // which also means we send that shit to refresh the body.
        if (delta.DamageDict.Count > 0 && isWoundable)
        {
            UpdateParentDamageFromBodyParts(ent,
                delta,
                interruptsDoAfters,
                origin,
                ignoreBlockers: ignoreBlockers);
        }

        return delta;
    }

    /// <summary>
    /// Updates the parent entity's damage values by summing damage from all body parts.
    /// Should be called after damage is applied to any body part.
    /// </summary>
    /// <param name="bodyPartUid">The body part that received damage</param>
    /// <param name="appliedDamage">The damage that was applied to the body part</param>
    /// <param name="interruptsDoAfters">Whether this damage change interrupts do-afters</param>
    /// <param name="origin">The entity that caused the damage</param>
    /// <param name="ignoreBlockers">Whether to ignore damage blockers</param>
    /// <returns>True if parent damage was updated, false otherwise</returns>
    private bool UpdateParentDamageFromBodyParts(
        EntityUid bodyPartUid,
        DamageSpecifier? appliedDamage,
        bool interruptsDoAfters,
        EntityUid? origin,
        BodyPartComponent? bodyPart = null,
        bool ignoreBlockers = false)
    {
        // Check if this is a body part and get the parent body
        if (!Resolve(bodyPartUid, ref bodyPart, logMissing: false)
            || bodyPart.Body is not { } body
            || !TryComp(body, out DamageableComponent? parentDamageable))
            return false;

        // Reset the parent's damage values
        foreach (var type in parentDamageable.Damage.DamageDict.Keys.ToList())
            parentDamageable.Damage.DamageDict[type] = FixedPoint2.Zero;

        // Sum up damage from all body parts
        foreach (var (partId, _) in _body.GetBodyChildren(body))
        {
            if (!_damageableQuery.TryComp(partId, out var partDamageable))
                continue;

            foreach (var (type, value) in partDamageable.Damage.DamageDict)
            {
                if (value == 0)
                    continue;

                if (parentDamageable.Damage.DamageDict.TryGetValue(type, out var existing))
                    parentDamageable.Damage.DamageDict[type] = existing + value;
            }
        }

        // Raise the damage changed event on the parent
        OnEntityDamageChanged((body, parentDamageable),
            appliedDamage,
            interruptsDoAfters,
            origin,
            ignoreBlockers: ignoreBlockers);

        return true;
    }

    public DamageSpecifier ApplySplitDamageBehaviors(SplitDamageBehavior splitDamageBehavior,
        DamageSpecifier damage,
        List<(EntityUid Id, BodyPartComponent Component, DamageableComponent Damageable)> parts)
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
                TryComp<ConsciousnessRequiredComponent>(part.Id, out var consciousness)
                && consciousness.CausesDeath == true).ToList();

                return newDamage / vitalParts.Count;
            case SplitDamageBehavior.SplitEnsureAllDamaged:
                var damagedParts = parts.Where(part =>
                    part.Damageable.TotalDamage > FixedPoint2.Zero).ToList();

                parts.Clear();
                parts.AddRange(damagedParts);

                goto case SplitDamageBehavior.SplitEnsureAll;
            case SplitDamageBehavior.SplitEnsureAllOrganic:
                var organicParts = parts.Where(part =>
                    part.Component.PartComposition == BodyPartComposition.Organic).ToList();

                parts.Clear();
                parts.AddRange(organicParts);

                goto case SplitDamageBehavior.SplitEnsureAll;
            case SplitDamageBehavior.SplitEnsureAllDamagedAndOrganic:
                var compatableParts = parts.Where(part =>
                    part.Damageable.TotalDamage > FixedPoint2.Zero &&
                    part.Component.PartComposition == BodyPartComposition.Organic).ToList();

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
                parts.Sort((a, b) => a.Damageable.TotalDamage.CompareTo(b.Damageable.TotalDamage));
                return newDamage;
            default:
                return damage;
        }
    }

    private void SetAllDamageShitmed(Entity<DamageableComponent> ent, FixedPoint2 newValue)
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

    public Dictionary<string, FixedPoint2> DamageSpecifierToWoundList(
        EntityUid uid,
        EntityUid? origin,
        TargetBodyPart targetPart,
        DamageSpecifier damageSpecifier,
        DamageableComponent damageable,
        bool ignoreResistances = false,
        float partMultiplier = 1.00f)
    {
        var damageDict = new Dictionary<string, FixedPoint2>();

        damageSpecifier = ApplyUniversalAllModifiers(damageSpecifier);

        // some wounds like Asphyxiation and Bloodloss aren't supposed to be created.
        if (!ignoreResistances)
        {
            if (damageable.DamageModifierSetId != null &&
                _prototypeManager.TryIndex(damageable.DamageModifierSetId, out var modifierSet))
            {
                // lol bozo
                var spec = new DamageSpecifier
                {
                    DamageDict = damageSpecifier.DamageDict,
                };

                damageSpecifier = DamageSpecifier.ApplyModifierSet(spec, modifierSet);
            }

            var ev = new DamageModifyEvent(uid, damageSpecifier, origin, targetPart);
            RaiseLocalEvent(uid, ev);
            damageSpecifier = ev.Damage;

            if (damageSpecifier.Empty)
            {
                return damageDict;
            }
        }

        foreach (var (type, severity) in damageSpecifier.DamageDict)
        {
            if (!_prototypeManager.TryIndex<EntityPrototype>(type, out var woundPrototype)
                || !woundPrototype.TryGetComponent<WoundComponent>(out _, _factory)
                || severity <= 0)
                continue;

            damageDict.Add(type, severity * partMultiplier);
        }

        return damageDict;
    }

    /// <summary>
    ///     Change the DamageContainer of a DamageableComponent. - Goobstation, Rubin Code
    /// </summary>
    public void ChangeDamageContainer(EntityUid uid, string newDamageContainerId, DamageableComponent? component = null)
    {
        if (!Resolve(uid, ref component, logMissing: false)
            || newDamageContainerId == component.DamageContainerID)
        {
            return;
        }

        // Try to get the new DamageContainerPrototype
        if (!_prototypeManager.TryIndex<DamageContainerPrototype>(newDamageContainerId, out var damageContainerPrototype))
        {
            // Return early if no DamageContainerPrototype is found
            return;
        }

        // Update the DamageContainerID
        component.DamageContainerID = new ProtoId<DamageContainerPrototype>(newDamageContainerId);

        // Clear the existing damage dictionary
        component.Damage.DamageDict.Clear();

        // Initialize damage dictionary, using the types and groups from the damage container prototype
        foreach (var type in damageContainerPrototype.SupportedTypes)
        {
            component.Damage.DamageDict.TryAdd(type, FixedPoint2.Zero);
        }

        foreach (var groupId in damageContainerPrototype.SupportedGroups)
        {
            var group = _prototypeManager.Index<DamageGroupPrototype>(groupId);
            foreach (var type in group.DamageTypes)
            {
                component.Damage.DamageDict.TryAdd(type, FixedPoint2.Zero);
            }
        }

        component.Damage.GetDamagePerGroup(_prototypeManager, component.DamagePerGroup);
        component.TotalDamage = component.Damage.GetTotal();
    }

    // Begin DeltaV Additions - We need to be able to change DamageContainer to make cultists vulnerable to Holy Damage
    public void SetDamageContainerID(Entity<DamageableComponent?> ent, string damageContainerId)
    {
        if (!_damageableQuery.Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.DamageContainerID = damageContainerId;
        Dirty(ent);
    }
    // End DeltaV Additions
}

public sealed partial class DamageChangedEvent
{
    /// <summary>
    ///     Shitmed - Whether or not the damage change should be blocked due to traumas or wounds
    /// </summary>
    public readonly bool IgnoreBlockers;

    /// <summary>
    ///     Shitmed - Damage before clamp of excessive heal and damage cap was applied
    /// </summary>
    public readonly DamageSpecifier? UncappedDamage;
}

public sealed partial class DamageModifyEvent
{
    public readonly EntityUid Target = target; // Goob - need this for some bullshit i think
    public readonly TargetBodyPart? TargetPart = targetPart; // Goob - Shitmed
}