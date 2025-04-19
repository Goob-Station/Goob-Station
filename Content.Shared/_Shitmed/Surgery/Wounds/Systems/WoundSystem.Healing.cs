using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared.Body.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Timing;

namespace Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;

public partial class WoundSystem
{
    private const double WoundableJobTime = 0.005;
    private readonly JobQueue _woundableJobQueue = new(WoundableJobTime);
    public sealed class IntegrityJob : Job<object>
    {
        private readonly WoundSystem _self;
        private readonly Entity<WoundableComponent> _ent;
        public IntegrityJob(WoundSystem self, Entity<WoundableComponent> ent, double maxTime, CancellationToken cancellation = default) : base(maxTime, cancellation)
        {
            _self = self;
            _ent = ent;
        }

        public IntegrityJob(WoundSystem self, Entity<WoundableComponent> ent, double maxTime, IStopwatch stopwatch, CancellationToken cancellation = default) : base(maxTime, stopwatch, cancellation)
        {
            _self = self;
            _ent = ent;
        }

        protected override Task<object?> Process()
        {
            _self.ProcessHealing(_ent);
            return Task.FromResult<object?>(null);
        }
    }

    private void ProcessHealing(Entity<WoundableComponent> ent)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var healableWounds = ent.Comp.Wounds.ContainedEntities.Count(wound => CanHealWound(wound));
        var healAmount = -ent.Comp.HealAbility / healableWounds;

        foreach (var wound in ent.Comp.Wounds!.ContainedEntities.ToList().Where(wound => CanHealWound(wound)))
        {
            ApplyWoundSeverity(wound, ApplyHealingRateMultipliers(wound, ent.Owner, healAmount, ent.Comp));
        }

        // That's it! o(( >ω< ))o
    }

    #region Public API

    public bool TryHaltAllBleeding(EntityUid woundable, WoundableComponent? component = null, bool force = false)
    {
        if (!Resolve(woundable, ref component) || component.Wounds!.Count == 0)
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
        Logger.Debug($"Attempting to heal most severe bleeding woundables with {healAmount} bleed");
        if (!Resolve(body, ref component) || healAmount <= 0)
            return false;

        Logger.Debug($"Checking for root part of body");
        // Get the root part of the body
        var rootPart = component.RootContainer.ContainedEntity;
        if (!rootPart.HasValue)
        {
            Logger.Debug("No root part found");
            return false;
        }

        // Collect all woundables and their total bleeding amounts
        var bleedingWoundables = new List<(EntityUid Woundable, FixedPoint2 BleedAmount)>();
        foreach (var (bodyPart, _) in _body.GetBodyChildren(body))
        {
            Logger.Debug($"Checking body part: {bodyPart}");
            FixedPoint2 totalBleedAmount = FixedPoint2.Zero;
            bool hasBleedingWounds = false;
            foreach (var wound in GetWoundableWounds(bodyPart))
            {
                Logger.Debug($"Checking wound: {wound}");
                if (!TryComp<BleedInflicterComponent>(wound, out var bleeds) || !bleeds.IsBleeding)
                    continue;

                Logger.Debug("Bleeding wound found");
                hasBleedingWounds = true;
                totalBleedAmount += bleeds.BleedingAmount;
            }

            Logger.Debug($"{bodyPart} Has bleeding wounds: {hasBleedingWounds}, total bleed amount: {totalBleedAmount}");
            if (hasBleedingWounds)
                bleedingWoundables.Add((bodyPart, totalBleedAmount));
        }

        // Sort woundables by bleeding amount (descending)
        var sortedWoundables = bleedingWoundables
            .OrderByDescending(x => x.BleedAmount)
            .Select(x => x.Woundable)
            .ToList();

        float remainingHealAmount = healAmount;
        bool anyHealed = false;

        // Apply healing to each woundable in order
        foreach (var woundable in sortedWoundables)
        {
            Logger.Debug($"Applying healing to {woundable} w/remaining heal amount: {remainingHealAmount}");
            if (remainingHealAmount <= 0)
                break;

            FixedPoint2 modifiedBleed;
            bool didHeal = TryHealBleedingWounds(woundable, -remainingHealAmount, out modifiedBleed);
            Logger.Debug($"Did heal: {didHeal}, modified bleed: {modifiedBleed}");
            if (didHeal)
            {
                Logger.Debug($"Healing {woundable} with {modifiedBleed} bleed");
                anyHealed = true;
                healed += -modifiedBleed - remainingHealAmount;
                remainingHealAmount = (float) -modifiedBleed;

                if (remainingHealAmount <= 0)
                    break;
            }
        }

        return anyHealed;
    }

    public bool TryHealBleedingWounds(EntityUid woundable, float bleedStopAbility, out FixedPoint2 modifiedBleed, WoundableComponent? component = null)
    {
        modifiedBleed = FixedPoint2.New(-bleedStopAbility);
        Logger.Debug($"Initial modified bleed: {modifiedBleed}");
        if (!Resolve(woundable, ref component))
            return false;

        foreach (var wound in GetWoundableWounds(woundable, component))
        {
            if (!TryComp<BleedInflicterComponent>(wound, out var bleeds) || !bleeds.IsBleeding)
                continue;

            Logger.Debug($"Starting bleed check");
            if (modifiedBleed > bleeds.BleedingAmount)
            {
                Logger.Debug($"Bleeding amount is greater than modified bleed, subtracting {bleeds.BleedingAmountRaw} from {modifiedBleed}, total sum: {modifiedBleed - bleeds.BleedingAmountRaw}");
                modifiedBleed -= bleeds.BleedingAmountRaw;
                bleeds.BleedingAmountRaw = 0;
                bleeds.IsBleeding = false;
                bleeds.Scaling = 0;
            }
            else
            {
                Logger.Debug($"Bleeding amount is less than modified bleed, subtracting {modifiedBleed} from {bleeds.BleedingAmountRaw}, total sum: {bleeds.BleedingAmountRaw - modifiedBleed}");
                bleeds.BleedingAmountRaw -= modifiedBleed;
                modifiedBleed = 0;
            }
        }

        Logger.Debug($"Modified bleed on healing: {modifiedBleed}, comparing to {bleedStopAbility}");
        return modifiedBleed != -bleedStopAbility;
    }

    public void ForceHealWoundsOnWoundable(EntityUid woundable,
        out FixedPoint2 healed,
        string? damageGroup = null,
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
    }

    public bool TryHealWoundsOnWoundable(EntityUid woundable,
        FixedPoint2 healAmount,
        out FixedPoint2 healed,
        WoundableComponent? component = null,
        string? damageGroup = null,
        bool ignoreMultipliers = false)
    {
        healed = 0;
        if (!Resolve(woundable, ref component))
            return false;

        var woundsToHeal =
            (from wound in component.Wounds!.ContainedEntities
                let woundComp = Comp<WoundComponent>(wound)
                where CanHealWound(wound)
                where damageGroup == null || damageGroup == woundComp.DamageGroup
                select (wound, woundComp)).Select(dummy => (Entity<WoundComponent>) dummy)
            .ToList(); // that's what I call LINQ.

        if (woundsToHeal.Count == 0)
            return false;

        var healNumba = healAmount / woundsToHeal.Count;
        var actualHeal = FixedPoint2.New(0);
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
        bool ignoreMultipliers = false)
    {
        healed = 0;
        if (!Resolve(woundable, ref component, false))
            return false;

        var woundsToHeal =
            (from wound in component.Wounds!.ContainedEntities
                let woundComp = Comp<WoundComponent>(wound)
                where CanHealWound(wound)
                where damageType == MetaData(wound).EntityPrototype!.ID
                select (wound, woundComp)).Select(dummy => (Entity<WoundComponent>) dummy)
            .ToList();

        if (woundsToHeal.Count == 0)
            return false;

        var healNumba = healAmount / woundsToHeal.Count;
        var actualHeal = (FixedPoint2) 0;
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
        out FixedPoint2 healed,
        WoundableComponent? component = null,
        bool ignoreMultipliers = false)
    {
        healed = 0;
        if (!Resolve(woundable, ref component, false))
            return false;

        foreach (var (key, value) in damage.DamageDict)
        {
            if (TryHealWoundsOnWoundable(woundable, -value, key, out var tempHealed, component, ignoreMultipliers))
            {
                healed += tempHealed;
                continue;
            }
        }

        return healed > 0;
    }

    public bool TryGetWoundableWithMostDamage(
        EntityUid body,
        [NotNullWhen(true)] out Entity<WoundableComponent>? woundable,
        string? damageGroup = null,
        bool healable = false)
    {
        var biggestDamage = (FixedPoint2) 0;

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
        return GetWoundableWounds(woundable).Any(wound => MetaData(wound).EntityPrototype!.ID == damageType);
    }

    public bool HasDamageOfGroup(
        EntityUid woundable,
        string damageGroup)
    {
        return GetWoundableWounds(woundable).Any(wound => wound.Comp.DamageGroup == damageGroup);
    }

    public FixedPoint2 ApplyHealingRateMultipliers(EntityUid wound, EntityUid woundable, FixedPoint2 severity, WoundableComponent? component = null)
    {
        if (!Resolve(woundable, ref component))
            return severity;

        var woundHealingMultiplier =
            _prototype.Index<DamageTypePrototype>(MetaData(wound).EntityPrototype!.ID).WoundHealingMultiplier;

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

    public bool CanHealWound(EntityUid wound, WoundComponent? comp = null)
    {
        if (!Resolve(wound, ref comp))
            return false;

        if (!comp.CanBeHealed)
            return false;

        var holdingWoundable = comp.HoldingWoundable;

        var ev = new WoundHealAttemptOnWoundableEvent((wound, comp));
        RaiseLocalEvent(holdingWoundable, ref ev);

        if (ev.Cancelled)
            return false;

        var ev1 = new WoundHealAttemptEvent((holdingWoundable, Comp<WoundableComponent>(holdingWoundable)));
        RaiseLocalEvent(wound, ref ev1);

        return !ev1.Cancelled;
    }

    #endregion
}