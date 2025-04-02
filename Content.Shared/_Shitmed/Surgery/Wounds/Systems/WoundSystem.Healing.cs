using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._Shitmed.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Surgery.Wounds.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Timing;

namespace Content.Shared._Shitmed.Surgery.Wounds.Systems;

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

        var healableWounds = ent.Comp.Wounds!.ContainedEntities.Select(Comp<WoundComponent>).Count(comp => comp.CanBeHealed);
        var healAmount = -ent.Comp.HealAbility / healableWounds;

        foreach (var wound in ent.Comp.Wounds!.ContainedEntities.ToList())
        {
            var comp = Comp<WoundComponent>(wound);
            if (!comp.CanBeHealed)
                continue;

            ApplyWoundSeverity(wound, ApplyHealingRateMultipliers(wound, ent.Owner, healAmount, ent.Comp), comp);
        }

        // That's it! o(( >ω< ))o
    }

    #region Public API

    public bool TryHaltAllBleeding(EntityUid woundable,
    WoundableComponent? component = null,
    bool force = false)
    {
        if (!Resolve(woundable, ref component) || component.Wounds!.Count == 0)
            return true;

        foreach (var wound in GetWoundableWounds(woundable, component))
        {
            if (force)
                wound.Comp.CanBeHealed = true;

            if (!TryComp<BleedInflicterComponent>(wound, out var bleeds))
                continue;

            bleeds.BleedingScales = false;
            bleeds.IsBleeding = false;

            wound.Comp.CanBeHealed = true;
        }

        return true;
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
            RemoveWound(wound);
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
                where woundComp.CanBeHealed
                where damageGroup == null || damageGroup == woundComp.DamageGroup
                select (wound, woundComp)).Select(dummy => (Entity<WoundComponent>) dummy)
            .ToList(); // that's what I call LINQ.

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
        FixedPoint2 healAmount,
        string damageType,
        out FixedPoint2 healed,
        WoundableComponent? component = null,
        bool ignoreMultipliers = false)
    {
        healed = 0;
        if (!Resolve(woundable, ref component))
            return false;

        var woundsToHeal =
            (from wound in component.Wounds!.ContainedEntities
                let woundComp = Comp<WoundComponent>(wound)
                where woundComp.CanBeHealed
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

    #endregion
}
