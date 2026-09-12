using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Body;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using System.Linq;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Damage.Systems;

public sealed partial class DamageableSystem
{
    /// <summary>
    ///     Directly sets the damage in a damageable component.
    ///     This method keeps the damage types supported by the DamageContainerPrototype in the component.
    ///     If a type is given in <paramref name="damage"/>, but not supported then it will not be set.
    ///     If a type is supported but not given in <paramref name="damage"/> then it will be set to 0.
    ///     [Woundmed: This does not account for woundmed stuff!]
    /// </summary>
    /// <remarks>
    ///     Useful for some unfriendly folk. Also ensures that cached values are updated and that a damage changed
    ///     event is raised.
    /// </remarks>
    public void SetDamage(Entity<DamageableComponent?> ent, DamageSpecifier damage)
    {
        if (!_damageableQuery.Resolve(ent, ref ent.Comp, false))
            return;

        foreach (var type in ent.Comp.Damage.DamageDict.Keys)
        {
            if (damage.DamageDict.TryGetValue(type, out var value))
                ent.Comp.Damage.DamageDict[type] = value;
            else
                ent.Comp.Damage.DamageDict[type] = 0;
        }

        OnEntityDamageChanged((ent, ent.Comp));
    }

    /// <summary>
    ///     Directly sets the damage specifier of a damageable component.
    ///     This will overwrite the complete damage dict, meaning it will bulldoze the supported damage types.
    ///     [Woundmed: This does not account for woundmed stuff!]
    /// </summary>
    /// <remarks>
    ///     This may break persistance as the supported types are reset in case the component is initialized again.
    ///     So this only makes sense if you also change the DamageContainerPrototype in the component at the same time.
    ///     Only use this method if you know what you are doing.
    /// </remarks>
    public void SetDamageSpecifier(Entity<DamageableComponent?> ent, DamageSpecifier damage)
    {
        if (!_damageableQuery.Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.Damage = damage;

        OnEntityDamageChanged((ent, ent.Comp));
    }

    /// <summary>
    ///     [Woundmed edited]
    ///     Applies damage specified via a <see cref="DamageSpecifier"/>.
    /// </summary>
    /// <remarks>
    ///     <see cref="DamageSpecifier"/> is effectively just a dictionary of damage types and damage values. This
    ///     function just applies the container's resistances (unless otherwise specified) and then changes the
    ///     stored damage data. Division of group damage into types is managed by <see cref="DamageSpecifier"/>.
    /// </remarks>
    /// <returns>
    ///     If the attempt was successful or not.
    /// </returns>
    public bool TryChangeDamage(
        Entity<DamageableComponent?> ent,
        DamageSpecifier damage,
        bool ignoreResistances = false,
        bool interruptsDoAfters = true,
        EntityUid? origin = null,
        bool ignoreGlobalModifiers = false,
        bool canBeCancelled = false, // Woundmed
        float partMultiplier = 1f, // Woundmed
        TargetBodyPart? targetPart = null, // Woundmed
        bool ignoreBlockers = false, // Woundmed
        SplitDamageBehavior splitDamage = SplitDamageBehavior.Split, // Woundmed
        bool canMiss = true // Woundmed
    )
    {
        //! Empty just checks if the DamageSpecifier is _literally_ empty, as in, is internal dictionary of damage types is empty.
        // If you deal 0.0 of some damage type, Empty will be false!
        return TryChangeDamage(ent, damage, out _, ignoreResistances, interruptsDoAfters, origin, ignoreGlobalModifiers,
            canBeCancelled, partMultiplier, targetPart, ignoreBlockers, splitDamage, canMiss); // Woundmed
    }

    /// <summary>
    ///     [Woundmed Edited]
    ///     Applies damage specified via a <see cref="DamageSpecifier"/>.
    /// </summary>
    /// <remarks>
    ///     <see cref="DamageSpecifier"/> is effectively just a dictionary of damage types and damage values. This
    ///     function just applies the container's resistances (unless otherwise specified) and then changes the
    ///     stored damage data. Division of group damage into types is managed by <see cref="DamageSpecifier"/>.
    /// </remarks>
    /// <returns>
    ///     If the attempt was successful or not.
    /// </returns>
    public bool TryChangeDamage(
        Entity<DamageableComponent?> ent,
        DamageSpecifier damage,
        out DamageSpecifier newDamage,
        bool ignoreResistances = false,
        bool interruptsDoAfters = true,
        EntityUid? origin = null,
        bool ignoreGlobalModifiers = false,
        bool canBeCancelled = false, // Woundmed
        float partMultiplier = 1f, // Woundmed
        TargetBodyPart? targetPart = null, // Woundmed
        bool ignoreBlockers = false, // Woundmed
        SplitDamageBehavior splitDamage = SplitDamageBehavior.Split, // Woundmed
        bool canMiss = true // Woundmed
    )
    {
        //! Empty just checks if the DamageSpecifier is _literally_ empty, as in, is internal dictionary of damage types is empty.
        // If you deal 0.0 of some damage type, Empty will be false!
        newDamage = ChangeDamage(ent, damage, ignoreResistances, interruptsDoAfters, origin, ignoreGlobalModifiers,
            canBeCancelled, partMultiplier, targetPart, ignoreBlockers, splitDamage, canMiss); // Woundmed
        return !newDamage.Empty;
    }

    /// <summary>
    ///     [Woundmed Edited]
    ///     Applies damage specified via a <see cref="DamageSpecifier"/>.
    ///     [Woundmed: This accounts for complex bodies if they are one, using <see cref="ApplyDamageComplex"/>]
    /// </summary>
    /// <remarks>
    ///     <see cref="DamageSpecifier"/> is effectively just a dictionary of damage types and damage values. This
    ///     function just applies the container's resistances (unless otherwise specified) and then changes the
    ///     stored damage data. Division of group damage into types is managed by <see cref="DamageSpecifier"/>.
    /// </remarks>
    /// <returns>
    ///     The actual amount of damage taken, as a DamageSpecifier.
    ///     [Woundmed: If it was a complex body, we only return the vital damage done.]
    /// </returns>
    public DamageSpecifier ChangeDamage(
        Entity<DamageableComponent?> ent,
        DamageSpecifier damage,
        bool ignoreResistances = false,
        bool interruptsDoAfters = true,
        EntityUid? origin = null,
        bool ignoreGlobalModifiers = false,
        bool canBeCancelled = false, // Woundmed
        float partMultiplier = 1f, // Woundmed
        TargetBodyPart? targetPart = null, // Woundmed
        bool ignoreBlockers = false, // Woundmed
        SplitDamageBehavior splitDamage = SplitDamageBehavior.Split, // Woundmed
        bool canMiss = true // Woundmed
    )
    {
        var damageDone = new DamageSpecifier(damage.ArmorPenetration,
            damage.PartDamageVariation,
            damage.WoundSeverityMultipliers);

        if (!_damageableQuery.Resolve(ent, ref ent.Comp, false))
            return damageDone;

        if (damage.Empty)
            return damageDone;

        var before = new BeforeDamageChangedEvent(damage, origin, canBeCancelled, targetPart);
        RaiseLocalEvent(ent, ref before);

        if (before.Cancelled)
            return damageDone;

        // <Woundmed>
        if (_bodyQuery.TryComp(ent, out var bodyComp)
            && bodyComp.BodyType == BodyType.Complex)
        {
            return ApplyDamageComplex(
                (ent, bodyComp),
                damage,
                ignoreResistances,
                interruptsDoAfters,
                origin,
                ignoreGlobalModifiers, // TODO unhandled
                partMultiplier,
                targetPart,
                ignoreBlockers,
                splitDamage,
                canMiss
            );
        }
        // </Woundmed>

        if (!ignoreResistances)
        {
            if (
                ent.Comp.DamageModifierSetId != null &&
                _prototypeManager.Resolve(ent.Comp.DamageModifierSetId, out var modifierSet)
            )
                damage = DamageSpecifier.ApplyModifierSet(damage,
                    DamageSpecifier.PenetrateArmor(modifierSet, damage.ArmorPenetration)); // Goob edit


            // <Woundmed>
            if (_bodyPartQuery.TryComp(ent, out var bodyPart)
                && bodyPart.Body != null)
            {
                TargetBodyPart? target = _body.GetTargetBodyPart(bodyPart);

                // First raise the event on the parent to apply any parent modifiers
                var parentEv = new DamageModifyEvent(bodyPart.Body.Value, damage, origin, target);
                RaiseLocalEvent(bodyPart.Body.Value, parentEv);
                damage = parentEv.Damage;
                // Then raise on the part itself for any part-specific modifiers
                var ev = new DamageModifyEvent(ent, damage, origin, target);
                RaiseLocalEvent(ent, ev);
                damage = ev.Damage;
            }
            else
            // </Woundmed> Not a body part, do upstream logic
            {
                // TODO DAMAGE
                // byref struct event.
                var ev = new DamageModifyEvent(ent, damage, origin); // Woundmed - add Target ent
                RaiseLocalEvent(ent, ev);
                damage = ev.Damage;
            }
        }

        if (!ignoreGlobalModifiers)
            damage = ApplyUniversalAllModifiers(damage);

        // <Woundmed>
        damageDone = new DamageSpecifier(damage.ArmorPenetration,
            damage.PartDamageVariation,
            damage.WoundSeverityMultipliers);

        // Check for integrity cap on body parts
        var isWoundable = _woundableQuery.TryComp(ent, out var woundable);
        var damageCap = isWoundable ? woundable!.IntegrityCap : FixedPoint2.MaxValue;
        var remainingCap = damageCap - ent.Comp.TotalDamage;
        var damageDoneHypotheticalUncapped = new DamageSpecifier(damage.ArmorPenetration,
            damage.PartDamageVariation,
            damage.WoundSeverityMultipliers); // slop
        damageDoneHypotheticalUncapped.DamageDict.EnsureCapacity(damage.DamageDict.Count);
        // </Woundmed>

        damageDone.DamageDict.EnsureCapacity(damage.DamageDict.Count);

        var dict = ent.Comp.Damage.DamageDict;
        foreach (var (type, value) in damage.DamageDict)
        {
            // CollectionsMarshal my beloved.
            if (!dict.TryGetValue(type, out var oldValue))
                continue;

            // <Woundmed>
            // kill upstream code! because we enforce a body part integrity cap here
            // no block comment to force a merge conflict

            //var newValue = FixedPoint2.Max(FixedPoint2.Zero, oldValue + value);
            //if (newValue == oldValue)
            //    continue;
            //
            //dict[type] = newValue;
            //damageDone.DamageDict[type] = newValue - oldValue;

            // For positive damage, we need to check if we've hit the cap
            if (value > 0)
            {
                // If we're not a woundable or we don't have a cap, apply the damage normally
                if (!isWoundable || remainingCap == FixedPoint2.MaxValue)
                {
                    dict[type] = oldValue + value;
                    damageDone.DamageDict[type] = value;
                    continue;
                }

                // If we've already hit the cap, skip this damage type
                if (remainingCap <= 0)
                    continue;

                // Calculate how much of this damage type we can apply
                var damageToApply = FixedPoint2.Min(value, remainingCap);
                var newValue = FixedPoint2.Max(FixedPoint2.Zero, oldValue + damageToApply);

                // we need the damage that Would have occured were it uncapped
                damageDoneHypotheticalUncapped.DamageDict[type] = damageToApply;

                //  Dont care if the value didnt actually changed
                if (newValue == oldValue)
                    continue;

                dict[type] = newValue;

                var applied = newValue - oldValue;
                dict[type] = newValue;
                damageDone.DamageDict[type] = applied;

                // Update remaining cap
                remainingCap -= applied;
            }
            else
            {
                // For negative damage (healing), apply normally
                var newValue = FixedPoint2.Max(FixedPoint2.Zero, oldValue + value);
                if (newValue != oldValue)
                {
                    dict[type] = newValue;
                    damageDone.DamageDict[type] = newValue - oldValue;
                }
            }
            // </Woundmed>
        }

        // <Woundmed> This means that the damaged part was a woundable
        // which also means we send that shit to refresh the body.
        // We don't have to do this if it isn't a complex body.
        if (!damageDone.Empty && isWoundable && bodyComp != null)
        {
            UpdateWoundableBodyDamage(
                (ent, bodyComp, ent.Comp),
                interruptsDoAfters,
                origin,
                ignoreBlockers
            );
        }
        // </Woundmed>

        if (!damageDone.Empty) // Woundmed - made delta ig. Idk this is weird might be wrong but apparently this mostly effects
            OnEntityDamageChanged((ent, ent.Comp), damageDone, interruptsDoAfters, origin,
                uncappedDamage: damageDoneHypotheticalUncapped); // Woundmed

        return damageDone;
    }

    /*
        old broken EvenHealthChnage code. which accounts for scaleTemp.
        which is currently removed for EvenHealthChange but not HealthChange

        // <Goob>
        var healing = amount * args.Scale;
        if (args.Effect.ScaleByTemperature is {} scaleTemp)
        {
            if (!TryComp<TemperatureComponent>(entity, out var temp))
                return; // condition stays the same so this is actually a good return in loop

            healing *= scaleTemp.GetEfficiencyMultiplier(temp.CurrentTemperature, args.Scale, false);
        }
        var spec = new DamageSpecifier(); // todo marty unfuck this after damagesystem
        var groupProto = _proto.Index(group);
        foreach (var type in groupProto.DamageTypes)
        {
            spec.DamageDict[type] = healing / groupProto.DamageTypes.Count;
        }

        _damageable.ChangeDamage(
                entity.AsNullable(),
                spec,
                ignoreResistances: args.Effect.IgnoreResistances,
                interruptsDoAfters: false,
                targetPart: args.Effect.UseTargeting ? args.Effect.TargetPart : null, // Omu, needed for full body healing for cryo chems
                splitDamage: args.Effect.SplitDamage); // Goob
        // </Goob>
    */

    /// <summary>
    /// [Woundmed Edited]
    /// Will reduce the damage on the entity exactly by <see cref="amount"/> as close as equally distributed among all damage types the entity has.
    /// If one of the damage types of the entity is too low. it will heal that completly and distribute the excess healing among the other damage types.
    /// If the <see cref="amount"/> is larger than the total damage of the entity then it just clears all damage.
    /// </summary>
    /// <param name="ent">entity to be healed</param>
    /// <param name="amount">how much to heal. value has to be negative to heal</param>
    /// <param name="group">from which group to heal. if null, heal from all groups</param>
    /// <param name="origin">who did the healing</param>
    public DamageSpecifier HealEvenly(
        Entity<DamageableComponent?> ent,
        FixedPoint2 amount,
        ProtoId<DamageGroupPrototype>? group = null,
        EntityUid? origin = null,
        TargetBodyPart targetPart = TargetBodyPart.All, // Woundmed
        SplitDamageBehavior damageSplit = SplitDamageBehavior.SplitEnsureAllOrganic) // Woundmed
    {
        var damageChange = new DamageSpecifier();

        if (!_damageableQuery.Resolve(ent, ref ent.Comp, false) || amount >= 0)
            return damageChange;

        // Get our total damage, or heal if we're below a certain amount.
        if (!TryGetDamageGreaterThan((ent, ent.Comp), -amount, out var damage, group))
            return ChangeDamage(ent, -damage, true, false, origin);

        // make sure damageChange has the same damage types as damage
        damageChange.DamageDict.EnsureCapacity(damage.DamageDict.Count);
        foreach (var type in damage.DamageDict.Keys)
        {
            damageChange.DamageDict.Add(type, FixedPoint2.Zero);
        }

        var remaining = -amount;
        var keys = damage.DamageDict.Keys.ToList();

        while (remaining > 0)
        {
            var count = keys.Count;
            // We do this to ensure that we always round up when dividing to avoid excess loops.
            // We already have logic to prevent healing more than we have.
            var maxHeal = count == 1 ? remaining : (remaining + FixedPoint2.Epsilon * (count - 1)) / count;

            // Iterate backwards since we're removing items.
            for (var i = count - 1; i >= 0; i--)
            {
                var type = keys[i];
                // This is the amount we're trying to heal, capped by maxHeal
                var heal = damage.DamageDict[type] + damageChange.DamageDict[type];

                // Don't go above max, if we don't go above max
                if (heal > maxHeal)
                    heal = maxHeal;
                // If we're not above max, we will heal it fully and don't need to enumerate anymore!
                else
                    keys.RemoveAt(i);

                if (heal >= remaining)
                {
                    // Don't remove more than we can remove. Prevents us from healing more than we'd expect...
                    damageChange.DamageDict[type] -= remaining;
                    remaining = FixedPoint2.Zero;
                    break;
                }

                remaining -= heal;
                damageChange.DamageDict[type] -= heal;
            }
        }

        return ChangeDamage(ent, damageChange, true, false, origin,
            targetPart: targetPart, // Woundmed
            splitDamage: damageSplit); // Woundmed
    }

    /// <summary>
    /// Will reduce the damage on the entity exactly by <see cref="amount"/> distributed by weight among all damage types the entity has.
    /// (the weight is how much damage of the type there is)
    /// If the <see cref="amount"/> is larger than the total damage of the entity then it just clears all damage.
    /// </summary>
    /// <param name="ent">entity to be healed</param>
    /// <param name="amount">how much to heal. value has to be negative to heal</param>
    /// <param name="group">from which group to heal. if null, heal from all groups</param>
    /// <param name="origin">who did the healing</param>
    public DamageSpecifier HealDistributed(
        Entity<DamageableComponent?> ent,
        FixedPoint2 amount,
        ProtoId<DamageGroupPrototype>? group = null,
        EntityUid? origin = null)
    {
        var damageChange = new DamageSpecifier();

        if (!_damageableQuery.Resolve(ent, ref ent.Comp, false) || amount >= 0)
            return damageChange;

        // Get our total damage, or heal if we're below a certain amount.
        if (!TryGetDamageGreaterThan((ent, ent.Comp), -amount, out var damage, group))
            return ChangeDamage(ent, -damage, true, false, origin);

        // make sure damageChange has the same damage types as damageEntity
        damageChange.DamageDict.EnsureCapacity(damage.DamageDict.Count);
        var total = damage.GetTotal();

        // heal weighted by the damage of that type
        foreach (var (type, value) in damage.DamageDict)
        {
            damageChange.DamageDict.Add(type, value / total * amount);
        }

        return ChangeDamage(ent, damageChange, true, false, origin);
    }

    /// <summary>
    /// Tries to get damage from an entity with an optional group specifier.
    /// </summary>
    /// <param name="ent">Entity we're checking the damage on</param>
    /// <param name="amount">Amount we want the damage to be greater than ideally</param>
    /// <param name="damage">Damage specifier we're returning with</param>
    /// <param name="group">An optional group, note that if it fails to index it will just use all damage.</param>
    /// <returns>True if the total damage is greater than the specified amount</returns>
    public bool TryGetDamageGreaterThan(Entity<DamageableComponent> ent,
        FixedPoint2 amount,
        out DamageSpecifier damage,
        ProtoId<DamageGroupPrototype>? group = null)
    {
        // get the damage should be healed (either all or only from one group)
        damage = group == null ? GetDamage(ent) : GetDamage(ent, group.Value);

        // If trying to heal more than the total damage of damageEntity just heal everything
        return damage.GetTotal() > amount;
    }

    /// <summary>
    /// Returns a <see cref="DamageSpecifier"/> with all positive damage of the entity from the group specified
    /// </summary>
    /// <param name="ent">entity with damage</param>
    /// <param name="group">group of damage to get values from</param>
    /// <returns></returns>
    public DamageSpecifier GetDamage(Entity<DamageableComponent> ent, ProtoId<DamageGroupPrototype> group)
    {
        // No damage if no group exists...
        if (!_prototypeManager.Resolve(group, out var groupProto))
            return new DamageSpecifier();

        var damage = new DamageSpecifier();
        damage.DamageDict.EnsureCapacity(groupProto.DamageTypes.Count);

        foreach (var damageId in groupProto.DamageTypes)
        {
            if (!ent.Comp.Damage.DamageDict.TryGetValue(damageId, out var value))
                continue;
            if (value > FixedPoint2.Zero)
                damage.DamageDict.Add(damageId, value);
        }

        return damage;
    }

    /// <summary>
    /// Returns a <see cref="DamageSpecifier"/> with all positive damage of the entity
    /// </summary>
    /// <param name="ent">entity with damage</param>
    /// <returns></returns>
    public DamageSpecifier GetDamage(Entity<DamageableComponent> ent)
    {
        var damage = new DamageSpecifier();
        damage.DamageDict.EnsureCapacity(ent.Comp.Damage.DamageDict.Count);

        foreach (var (damageId, value) in ent.Comp.Damage.DamageDict)
        {
            if (value > FixedPoint2.Zero)
                damage.DamageDict.Add(damageId, value);
        }

        return damage;
    }

    /// <summary>
    /// Applies the two universal "All" modifiers, if set.
    /// Individual damage source modifiers are set in their respective code.
    /// </summary>
    /// <param name="damage">The damage to be changed.</param>
    public DamageSpecifier ApplyUniversalAllModifiers(DamageSpecifier damage)
    {
        // Checks for changes first since they're unlikely in normal play.
        if (
            MathHelper.CloseToPercent(UniversalAllDamageModifier, 1f) &&
            MathHelper.CloseToPercent(UniversalAllHealModifier, 1f)
        )
            return damage;

        foreach (var (key, value) in damage.DamageDict)
        {
            if (value == 0)
                continue;

            if (value > 0)
            {
                damage.DamageDict[key] *= UniversalAllDamageModifier;

                continue;
            }

            if (value < 0)
                damage.DamageDict[key] *= UniversalAllHealModifier;
        }

        return damage;
    }

    public void ClearAllDamage(Entity<DamageableComponent?> ent)
    {
        SetAllDamage(ent, FixedPoint2.Zero);
    }

    /// <summary>
    ///     [Woundmed Edited]
    ///     Sets all damage types supported by a <see cref="Components.DamageableComponent"/> to the specified value.
    ///     [Woundmed: If entity has a body, this will also set the damages for all of its body parts.]
    /// </summary>
    /// <remarks>
    ///     Does nothing If the given damage value is negative.
    /// </remarks>
    public void SetAllDamage(Entity<DamageableComponent?> ent, FixedPoint2 newValue)
    {
        if (!_damageableQuery.Resolve(ent, ref ent.Comp, false))
            return;

        if (newValue < 0)
            return;

        foreach (var type in ent.Comp.Damage.DamageDict.Keys)
        {
            ent.Comp.Damage.DamageDict[type] = newValue;
        }

        SetAllWoundDamage((ent, ent.Comp), newValue); // Woundmed

        // Setting damage does not count as 'dealing' damage, even if it is set to a larger value, so we pass an
        // empty damage delta.
        OnEntityDamageChanged((ent, ent.Comp), new DamageSpecifier());
    }

    /// <summary>
    /// [Woundmed Edited]
    /// Set's the damage modifier set prototype for this entity.
    /// [Woundmed: This will also set the damage modifier onto children body parts.]
    /// </summary>
    /// <param name="ent">The entity we're setting the modifier set of.</param>
    /// <param name="damageModifierSetId">The prototype we're setting.</param>
    public void SetDamageModifierSetId(Entity<DamageableComponent?> ent, ProtoId<DamageModifierSetPrototype>? damageModifierSetId)
    {
        if (!_damageableQuery.Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.DamageModifierSetId = damageModifierSetId;

        // <Woundmed> Individual part damage modifiers
        foreach (var (id, part) in _body.GetBodyChildren(ent))
        {
            if (!_damageableQuery.TryComp(id, out var comp))
            {
                DebugTools.Assert("Body part didn't have DamageableComponent when setting modifier!");
            }
            else
            {
                comp.DamageModifierSetId = damageModifierSetId;
                Dirty(id, comp);
            }
        }
        // </Woundmed>

        Dirty(ent);
    }
}
