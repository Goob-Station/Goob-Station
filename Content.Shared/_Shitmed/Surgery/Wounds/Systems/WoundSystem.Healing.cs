// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;
using Content.Shared.Body.Part;


namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;

/// <summary>
/// This class is responsible for managing wound healing in the shared game code.
/// It contains methods for updating the pain state after wounds are healed,
/// and for halting all bleeding on a given entity.
/// </summary>
public partial class WoundSystem
{
    [Dependency] private readonly PainSystem _pain = default!;

    // Updates pain state after wounds are healed and starts pain decay
    /// <param name="woundable">The entity on which to update the pain state</param>
    private void UpdatePainAfterHealing(EntityUid woundable)
    {
        // Check if the entity has a BodyPartComponent and if it is part of a body.
        if (!TryComp<BodyPartComponent>(woundable, out var bodyPart) || !bodyPart.Body.HasValue)
            return;

        // Get the body entity.
        var body = bodyPart.Body.Value;

        // Check if the body has a NerveSystemComponent.
        if (!TryComp<NerveSystemComponent>(body, out var nerveSystem))
            return;

        // Start pain decay if there's still pain after healing
        if (nerveSystem.Pain > FixedPoint2.Zero)
        {
            // Calculate decay duration based on current pain level - 12 seconds per pain point
            // 50 pain * 12 seconds per pain point = 600 seconds = 10 minutes
            var decayDuration = TimeSpan.FromSeconds(nerveSystem.Pain.Float() * 12);

            // Start the pain decay process
            _pain.StartPainDecay(body, nerveSystem.Pain, decayDuration, nerveSystem);
        }
    }

    #region Public API

    public bool TryHaltAllBleeding(EntityUid woundable, WoundableComponent? component = null, bool force = false)
    {
        if (!Resolve(woundable, ref component)
            || component.Wounds == null
            || component.Wounds.Count == 0)
            return true;

        foreach (var wound in GetWoundableWounds(woundable, component))
        {
            if (force)
            {
                // For wounds like scars. Temporary for now
                wound.Comp.CanBeHealed = true;
            }

            if (!TryComp<BleedInflicterComponent>(wound, out var bleeds))
                continue;

            bleeds.IsBleeding = false;
        }

        return true;
    }

    /// <summary>
    /// Heals bleeding wounds on a body entity, starting with the most severely bleeding woundable
    /// and cascading any leftover healing to the next most severe bleeding woundable.
    /// </summary>
    /// <param name="body">The body entity to check for bleeding wounds</param>
    /// <param name="healAmount">The amount of healing to apply</param>
    /// <param name="healed">The total amount of bleeding that was healed</param>
    /// <param name="component">Optional body component if already resolved</param>
    /// <returns>True if any bleeding was healed, false otherwise</returns>
    public bool TryHealMostSevereBleedingWoundables(EntityUid body, float healAmount, out FixedPoint2 healed, BodyComponent? component = null)
    {
        healed = FixedPoint2.Zero;
        if (!Resolve(body, ref component) || healAmount <= 0)
            return false;

        // Get the root part of the body
        var rootPart = component.RootContainer.ContainedEntity;
        if (!rootPart.HasValue)
            return false;

        // Collect all woundables and their total bleeding amounts
        var bleedingWoundables = new List<(EntityUid Woundable, FixedPoint2 BleedAmount)>();
        foreach (var (bodyPart, _) in _body.GetBodyChildren(body))
        {
            FixedPoint2 totalBleedAmount = FixedPoint2.Zero;
            bool hasBleedingWounds = false;
            foreach (var wound in GetWoundableWounds(bodyPart))
            {
                if (!TryComp<BleedInflicterComponent>(wound, out var bleeds) || !bleeds.IsBleeding)
                    continue;

                hasBleedingWounds = true;
                totalBleedAmount += bleeds.BleedingAmount;
            }

            if (hasBleedingWounds)
                bleedingWoundables.Add((bodyPart, totalBleedAmount));
        }

        // Sort woundables by bleeding amount (descending)
        var sortedWoundables = bleedingWoundables
            .OrderByDescending(x => x.BleedAmount)
            .Select(x => x.Woundable)
            .ToList();

        float remainingHealAmount = healAmount * sortedWoundables.Count();
        bool anyHealed = false;

        // Apply healing to each woundable in order
        foreach (var woundable in sortedWoundables)
        {
            if (remainingHealAmount <= 0)
                break;

            FixedPoint2 modifiedBleed;
            bool didHeal = TryHealBleedingWounds(woundable, -remainingHealAmount, out modifiedBleed);
            if (didHeal)
            {
                anyHealed = true;
                healed += -modifiedBleed - remainingHealAmount;
                remainingHealAmount -= (float) modifiedBleed; // Goobstation fix

                if (remainingHealAmount <= 0)
                    break;
            }
        }

        return anyHealed;
    }

    public bool TryHealBleedingWounds(EntityUid woundable, float bleedStopAbility, out FixedPoint2 modifiedBleed, WoundableComponent? component = null)
    {
        modifiedBleed = FixedPoint2.Zero; // Goobstation
        if (!Resolve(woundable, ref component))
            return false;

        foreach (var wound in GetWoundableWounds(woundable, component))
        {
            if (!TryComp<BleedInflicterComponent>(wound, out var bleeds)
                || !bleeds.IsBleeding)
                continue;

            if (-bleedStopAbility > bleeds.BleedingAmount) // Goobstation
            {
                modifiedBleed = bleeds.BleedingAmount; // Goobstation
                bleeds.BleedingAmountRaw = 0;
                bleeds.IsBleeding = false;
                bleeds.Scaling = 0;
            }
            else
            {
                bleeds.BleedingAmountRaw += bleedStopAbility; // Goobstation
                modifiedBleed = -bleedStopAbility; // Goobstation
            }

            Dirty(wound, bleeds);
        }
        return modifiedBleed <= -bleedStopAbility; // Goobstation
    }

    public void ForceHealWoundsOnWoundable(EntityUid woundable,
        out FixedPoint2 healed,
        DamageGroupPrototype? damageGroup = null,
        WoundableComponent? component = null)
    {
        healed = 0;
        if (!Resolve(woundable, ref component))
            return;

        var woundsToHeal =
            GetWoundableWounds(woundable, component)
                .Where(wound => damageGroup == null || wound.Comp.DamageGroup == damageGroup)
                .ToList();

        foreach (var wound in woundsToHeal)
        {
            healed += wound.Comp.WoundSeverityPoint;
            RemoveWound(wound, wound);
        }

        UpdateWoundableIntegrity(woundable, component);
        CheckWoundableSeverityThresholds(woundable, component);

        // Update pain state after healing wounds if any wounds were healed
        if (woundsToHeal.Count > 0)
        {
            UpdatePainAfterHealing(woundable);
        }
    }

    public bool TryHealWoundsOnWoundable(EntityUid woundable,
        FixedPoint2 healAmount,
        out FixedPoint2 healed,
        WoundableComponent? component = null,
        DamageGroupPrototype? damageGroup = null,
        bool ignoreMultipliers = false,
        bool ignoreBlockers = false)
    {
        healed = 0;
        if (!Resolve(woundable, ref component)
            || component.Wounds == null)
            return false;

        var woundsToHeal =
            (from wound in component.Wounds.ContainedEntities
                let woundComp = Comp<WoundComponent>(wound)
                where CanHealWound(wound, woundComp, ignoreBlockers)
                where damageGroup == null || damageGroup == woundComp.DamageGroup
                select (wound, woundComp)).Select(dummy => (Entity<WoundComponent>) dummy)
            .ToList(); // that's what I call LINQ.

        if (woundsToHeal.Count == 0)
            return false;

        var healNumba = healAmount / woundsToHeal.Count;
        var actualHeal = FixedPoint2.Zero;
        foreach (var wound in woundsToHeal)
        {
            var heal = ignoreMultipliers
                ? ApplyHealingRateMultipliers(wound, woundable, -healNumba, component)
                : -healNumba;

            actualHeal += -heal;
            ApplyWoundSeverity(wound, heal, wound);
        }

        UpdateWoundableIntegrity(woundable, component);
        CheckWoundableSeverityThresholds(woundable, component);

        healed = actualHeal;
        return actualHeal > 0;
    }

    public bool TryHealWoundsOnWoundable(EntityUid woundable,
        FixedPoint2 healAmount,
        string damageType,
        out FixedPoint2 healed,
        WoundableComponent? component = null,
        bool ignoreMultipliers = false,
        bool ignoreBlockers = false)
    {
        healed = 0;
        if (!Resolve(woundable, ref component, false)
            || component.Wounds == null)
            return false;

        var woundsToHeal =
            (from wound in component.Wounds.ContainedEntities
                let woundComp = Comp<WoundComponent>(wound)
                where CanHealWound(wound, woundComp, ignoreBlockers)
                where damageType == woundComp.DamageType
                select (wound, woundComp)).Select(dummy => (Entity<WoundComponent>) dummy)
            .ToList();

        if (woundsToHeal.Count == 0)
            return false;

        var healNumba = healAmount / woundsToHeal.Count;
        var actualHeal = FixedPoint2.Zero;
        foreach (var wound in woundsToHeal)
        {
            var heal = ignoreMultipliers
                ? ApplyHealingRateMultipliers(wound, woundable, -healNumba, component)
                : -healNumba;

            actualHeal += -heal;
            ApplyWoundSeverity(wound, heal, wound);
        }

        UpdateWoundableIntegrity(woundable, component);
        CheckWoundableSeverityThresholds(woundable, component);

        healed = actualHeal;
        return actualHeal > 0;
    }

    public bool TryHealWoundsOnWoundable(EntityUid woundable,
        DamageSpecifier damage,
        out Dictionary<string, FixedPoint2> healed,
        WoundableComponent? component = null,
        bool ignoreMultipliers = false)
    {
        healed = [];
        if (!Resolve(woundable, ref component, false))
            return false;

        foreach (var (key, value) in damage.DamageDict)
        {
            if (TryHealWoundsOnWoundable(woundable, -value, key, out var tempHealed, component, ignoreMultipliers))
            {
                healed.Add(key, tempHealed);
                continue;
            }
        }

        return healed.Any();
    }

    public bool TryGetWoundableWithMostDamage(
        EntityUid body,
        [NotNullWhen(true)] out Entity<WoundableComponent>? woundable,
        string? damageGroup = null,
        bool healable = false)
    {
        var biggestDamage = FixedPoint2.Zero;

        woundable = null;
        foreach (var bodyPart in _body.GetBodyChildren(body))
        {
            if (!TryComp<WoundableComponent>(bodyPart.Id, out var woundableComp))
                continue;

            var woundableDamage = GetWoundableSeverityPoint(bodyPart.Id, woundableComp, damageGroup, healable);
            if (woundableDamage <= biggestDamage)
                continue;

            biggestDamage = woundableDamage;
            woundable = (bodyPart.Id, woundableComp);
        }

        return woundable != null;
    }

    public bool HasDamageOfType(
        EntityUid woundable,
        string damageType,
        bool healable = false)
    {
        if (healable)
            return GetWoundableWounds(woundable)
                .Any(wound => wound.Comp.DamageType == damageType);

        return GetWoundableWounds(woundable).Any(wound => wound.Comp.DamageType == damageType);
    }

    public bool HasDamageOfGroup(
        EntityUid woundable,
        string damageGroup,
        bool healable = false)
    {
        if (healable)
            return GetWoundableWounds(woundable)
                .Any(wound => wound.Comp.DamageGroup == damageGroup);

        return GetWoundableWounds(woundable).Any(wound => wound.Comp.DamageGroup == damageGroup);
    }

    public FixedPoint2 ApplyHealingRateMultipliers(EntityUid wound,
        EntityUid woundable,
        FixedPoint2 severity,
        WoundableComponent? component = null,
        WoundComponent? woundComp = null)
    {
        if (!Resolve(woundable, ref component))
            return severity;

        if (!Resolve(wound, ref woundComp, false)
            || !woundComp.CanBeHealed)
            return FixedPoint2.Zero;

        var woundHealingMultiplier =
            _prototype.Index<DamageTypePrototype>(Comp<WoundComponent>(wound).DamageType).WoundHealingMultiplier;

        if (component.HealingMultipliers.Count == 0)
            return severity * woundHealingMultiplier;

        var toMultiply =
            component.HealingMultipliers.Sum(multiplier => (float) multiplier.Value.Change) / component.HealingMultipliers.Count;
        return severity * toMultiply * woundHealingMultiplier;
    }

    public bool TryAddHealingRateMultiplier(EntityUid owner, EntityUid woundable, string identifier, FixedPoint2 change, WoundableComponent? component = null)
    {
        if (!Resolve(woundable, ref component) || !_net.IsServer)
            return false;

        return component.HealingMultipliers.TryAdd(owner, new WoundableHealingMultiplier(change, identifier));
    }

    public bool TryRemoveHealingRateMultiplier(EntityUid owner, EntityUid woundable, WoundableComponent? component = null)
    {
        if (!Resolve(woundable, ref component)  || !_net.IsServer)
            return false;

        return component.HealingMultipliers.Remove(owner);
    }

    public bool CanHealWound(EntityUid wound, WoundComponent? comp = null, bool ignoreBlockers = false)
    {
        if (!Resolve(wound, ref comp))
            return false;

        if (!ignoreBlockers && !comp.CanBeHealed)
            return false;

        var holdingWoundable = comp.HoldingWoundable;

        var ev = new WoundHealAttemptOnWoundableEvent((wound, comp));
        RaiseLocalEvent(holdingWoundable, ref ev);

        if (ev.Cancelled)
            return false;

        var ev1 = new WoundHealAttemptEvent((holdingWoundable, Comp<WoundableComponent>(holdingWoundable)), ignoreBlockers);
        RaiseLocalEvent(wound, ref ev1);

        return !ev1.Cancelled;
    }

    /// <summary>
    /// Method to get all wounds of some entity
    /// </summary>
    /// <param name="target"></param>
    /// <param name="wounds"></param>
    /// <returns></returns>
    public bool TryGetAllOwnerWounds(EntityUid target, [NotNullWhen(true)] out List<Entity<WoundComponent>> wounds)
    {
        wounds = [];

        if (!_body.TryGetRootPart(target, out var body))
            return false;

        wounds = GetAllWounds(body.Value.Owner).ToList();

        return wounds.Any();
    }

    /// <summary>
    /// Method to get all wounded parts of entity
    /// </summary>
    /// <param name="target"></param>
    /// <param name="woundables"></param>
    /// <returns></returns>
    public bool TryGetAllOwnerWoundedParts(EntityUid target, [NotNullWhen(true)] out List<Entity<WoundableComponent>> woundables)
    {
        woundables = [];

        foreach (var bodyPart in _body.GetBodyChildren(target))
        {
            if (!TryComp<WoundableComponent>(bodyPart.Id, out var woundableComp) || !woundableComp.Wounds.ContainedEntities.Any())
                continue;

            woundables.Add((bodyPart.Id, woundableComp));
        }

        return woundables.Any();
    }

    /// <summary>
    /// Method to heal all wounds on entity by specific healing amount.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="healing"></param>
    /// <param name="ignoreBlockers"></param>
    /// <returns></returns>
    public bool TryHealWoundsOnOwner(EntityUid target, DamageSpecifier healing, bool ignoreBlockers = false)
    {
        var healedWounds = 0;

        if (!TryGetAllOwnerWoundedParts(target, out var woundables) || !TryGetAllOwnerWounds(target, out var wounds))
            return false;

        DamageSpecifier healingPerPart = new DamageSpecifier(healing);
        healingPerPart.DamageDict.Clear();

        var woundCountByType = wounds
            .GroupBy(w => w.Comp.DamageType)
            .ToDictionary(g => g.Key, g => g.Count());


        foreach (var healingType in healing.DamageDict)
        {
            var splitAmount = woundCountByType.GetValueOrDefault(healingType.Key, 0);

            // If we don't have wounds with our damage type just set it to heal value
            var splittedDamage = splitAmount != 0 ? healingType.Value / splitAmount : healingType.Value;

            healingPerPart.DamageDict.Add(healingType.Key, splittedDamage);
        }

        foreach (var woundable in woundables)
        {
            if (!TryHealWoundsOnWoundable(woundable.Owner, healingPerPart, out var healed, woundable.Comp, ignoreBlockers))
                continue;

            healedWounds++;
        }

        return healedWounds > 0;
    }

    #endregion
}
