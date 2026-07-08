// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
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
public sealed partial class WoundSystem
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
        bleedingWoundables.Sort((a, b) => b.BleedAmount.CompareTo(a.BleedAmount));

        float remainingHealAmount = healAmount * bleedingWoundables.Count;
        bool anyHealed = false;

        // Apply healing to each woundable in order
        foreach (var (woundable, _) in bleedingWoundables)
        {
            if (remainingHealAmount <= 0)
                break;

            FixedPoint2 modifiedBleed;
            bool didHeal = TryHealBleedingWounds(woundable, -remainingHealAmount, out modifiedBleed);
            if (didHeal)
            {
                anyHealed = true;
                healed += -modifiedBleed - remainingHealAmount;
                remainingHealAmount -= (float) modifiedBleed;

                if (remainingHealAmount <= 0)
                    break;
            }
        }

        return anyHealed;
    }

    public bool TryHealBleedingWounds(EntityUid woundable, float bleedStopAbility, out FixedPoint2 modifiedBleed, WoundableComponent? component = null)
    {
        modifiedBleed = FixedPoint2.Zero;
        if (!Resolve(woundable, ref component))
            return false;

        foreach (var wound in GetWoundableWounds(woundable, component))
        {
            if (!TryComp<BleedInflicterComponent>(wound, out var bleeds)
                || !bleeds.IsBleeding)
                continue;

            if (-bleedStopAbility > bleeds.BleedingAmount)
            {
                modifiedBleed = bleeds.BleedingAmount;
                bleeds.BleedingAmountRaw = 0;
                bleeds.IsBleeding = false;
                bleeds.Scaling = 0;
            }
            else
            {
                bleeds.BleedingAmountRaw += bleedStopAbility;
                modifiedBleed = -bleedStopAbility;
            }

            Dirty(wound, bleeds);
        }
        return modifiedBleed <= -bleedStopAbility;
    }

    public void ForceHealWoundsOnWoundable(EntityUid woundable,
        out FixedPoint2 healed,
        DamageGroupPrototype? damageGroup = null,
        WoundableComponent? component = null)
    {
        healed = 0;
        if (!Resolve(woundable, ref component))
            return;

        var woundsToHeal = new List<Entity<WoundComponent>>();
        foreach (var wound in GetWoundableWounds(woundable, component))
        {
            if (damageGroup == null || wound.Comp.DamageGroup == damageGroup)
                woundsToHeal.Add(wound);
        }

        foreach (var wound in woundsToHeal)
        {
            healed += wound.Comp.WoundSeverityPoint;
            RemoveWound(wound, wound);
        }

        UpdateWoundableIntegrity(woundable, component);
        CheckWoundableSeverityThresholds(woundable, component);

        // Update pain state after healing wounds if any wounds were healed
        if (woundsToHeal.Count > 0)
            UpdatePainAfterHealing(woundable);
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

        var woundsToHeal = new List<Entity<WoundComponent>>();
        foreach (var wound in component.Wounds.ContainedEntities)
        {
            var woundComp = Comp<WoundComponent>(wound);
            if (CanHealWound(wound, woundComp, ignoreBlockers)
                && (damageGroup == null || damageGroup == woundComp.DamageGroup))
                woundsToHeal.Add((wound, woundComp));
        }

        if (woundsToHeal.Count == 0)
            return false;

        var healNumba = healAmount / woundsToHeal.Count;
        var actualHeal = FixedPoint2.Zero;
        foreach (var wound in woundsToHeal)
        {
            var heal = ignoreMultipliers
                ? -healNumba
                : ApplyHealingRateMultipliers(wound, woundable, -healNumba, component);

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

        if (!HealWoundsCore(woundable, healAmount, damageType, out healed, component, ignoreMultipliers, ignoreBlockers))
            return false;

        UpdateWoundableIntegrity(woundable, component);
        CheckWoundableSeverityThresholds(woundable, component);
        return true;
    }

    public bool TryHealWoundsOnWoundable(EntityUid woundable,
        DamageSpecifier damage,
        out Dictionary<string, FixedPoint2> healed,
        WoundableComponent? component = null,
        bool ignoreMultipliers = false,
        bool ignoreBlockers = false)
    {
        healed = [];
        if (!Resolve(woundable, ref component, false)
            || component.Wounds == null)
            return false;

        foreach (var (key, value) in damage.DamageDict)
        {
            if (HealWoundsCore(woundable, -value, key, out var tempHealed, component, ignoreMultipliers, ignoreBlockers))
                healed.Add(key, tempHealed);
        }

        if (healed.Count == 0)
            return false;

        UpdateWoundableIntegrity(woundable, component);
        CheckWoundableSeverityThresholds(woundable, component);
        return true;
    }

    /// <summary>
    /// Core healing logic shared by TryHealWoundsOnWoundable overloads.
    /// Does NOT call UpdateWoundableIntegrity or CheckWoundableSeverityThresholds.
    /// </summary>
    private bool HealWoundsCore(EntityUid woundable,
        FixedPoint2 healAmount,
        string damageType,
        out FixedPoint2 healed,
        WoundableComponent component,
        bool ignoreMultipliers = false,
        bool ignoreBlockers = false)
    {
        healed = 0;

        var woundsToHeal = new List<Entity<WoundComponent>>();
        foreach (var wound in component.Wounds.ContainedEntities)
        {
            var woundComp = Comp<WoundComponent>(wound);
            if (CanHealWound(wound, woundComp, ignoreBlockers)
                && damageType == woundComp.DamageType)
                woundsToHeal.Add((wound, woundComp));
        }

        if (woundsToHeal.Count == 0)
            return false;

        var healNumba = healAmount / woundsToHeal.Count;
        var actualHeal = FixedPoint2.Zero;
        foreach (var wound in woundsToHeal)
        {
            var heal = ignoreMultipliers
                ? -healNumba
                : ApplyHealingRateMultipliers(wound, woundable, -healNumba, component);

            actualHeal += -heal;
            ApplyWoundSeverity(wound, heal, wound);
        }

        healed = actualHeal;
        return actualHeal > 0;
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
        string damageType)
    {
        foreach (var wound in GetWoundableWounds(woundable))
        {
            if (wound.Comp.DamageType == damageType)
                return true;
        }
        return false;
    }

    public bool HasDamageOfGroup(
        EntityUid woundable,
        string damageGroup)
    {
        foreach (var wound in GetWoundableWounds(woundable))
        {
            if (wound.Comp.DamageGroup == damageGroup)
                return true;
        }
        return false;
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
            _prototype.Index(woundComp.DamageType).WoundHealingMultiplier;

        if (component.HealingMultipliers.Count == 0)
            return severity * woundHealingMultiplier;

        var sum = 0f;
        foreach (var multiplier in component.HealingMultipliers)
            sum += (float) multiplier.Value.Change;

        return severity * (sum / component.HealingMultipliers.Count) * woundHealingMultiplier;
    }

    public bool TryAddHealingRateMultiplier(EntityUid owner, EntityUid woundable, string identifier, FixedPoint2 change, WoundableComponent? component = null)
    {
        if (!Resolve(woundable, ref component))
            return false;

        return component.HealingMultipliers.TryAdd(owner, new WoundableHealingMultiplier(change, identifier));
    }

    public bool TryRemoveHealingRateMultiplier(EntityUid owner, EntityUid woundable, WoundableComponent? component = null)
    {
        if (!Resolve(woundable, ref component))
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

        foreach (var wound in GetAllWounds(body.Value.Owner))
            wounds.Add(wound);

        return wounds.Count > 0;
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
            if (!TryComp<WoundableComponent>(bodyPart.Id, out var woundableComp) || woundableComp.Wounds.ContainedEntities.Count == 0)
                continue;

            woundables.Add((bodyPart.Id, woundableComp));
        }

        return woundables.Count > 0;
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
        var woundables = new List<Entity<WoundableComponent>>();
        var woundCountByType = new Dictionary<string, int>();

        foreach (var (id, _) in _body.GetBodyChildren(target))
        {
            if (!TryComp<WoundableComponent>(id, out var woundableComp)
                || woundableComp.Wounds.ContainedEntities.Count == 0)
                continue;

            woundables.Add((id, woundableComp));

            foreach (var woundEntity in woundableComp.Wounds.ContainedEntities)
            {
                var type = Comp<WoundComponent>(woundEntity).DamageType;
                woundCountByType[type] = woundCountByType.GetValueOrDefault(type) + 1;
            }
        }

        if (woundables.Count == 0)
            return false;

        var healingPerPart = new DamageSpecifier(healing);
        healingPerPart.DamageDict.Clear();

        foreach (var healingType in healing.DamageDict)
        {
            var splitAmount = woundCountByType.GetValueOrDefault(healingType.Key, 0);
            var splittedDamage = splitAmount != 0 ? healingType.Value / splitAmount : healingType.Value;
            healingPerPart.DamageDict.Add(healingType.Key, splittedDamage);
        }

        var healedWounds = 0;
        foreach (var woundable in woundables)
        {
            if (!TryHealWoundsOnWoundable(woundable.Owner, healingPerPart, out var healed, woundable.Comp, ignoreBlockers: ignoreBlockers))
                continue;

            healedWounds++;
        }

        return healedWounds > 0;
    }

    #endregion
}
