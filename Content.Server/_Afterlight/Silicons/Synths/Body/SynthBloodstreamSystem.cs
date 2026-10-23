using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Afterlight.Silicons.Synths.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Afterlight.Silicons.Synths.Body;

public sealed partial class SynthBloodstreamSystem : EntitySystem
{
    [Dependency] private SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private HungerSystem _hunger = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SynthBloodstreamComponent, HungerComponent, BloodstreamComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var synth, out var hunger, out var bloodstream, out var damageable))
        {
            if (_timing.CurTime < synth.NextUpdate)
                continue;

            synth.NextUpdate = _timing.CurTime + synth.UpdateRate;

            if (TryComp(uid, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead)
                continue;

            UpdateSynthBloodstream((uid, synth, hunger, bloodstream, damageable));
        }
    }

    private void UpdateSynthBloodstream(Entity<SynthBloodstreamComponent, HungerComponent, BloodstreamComponent, DamageableComponent> ent)
    {
        var hunger = _hunger.GetHunger(ent.Comp2);
        if (hunger <= ent.Comp1.MinHunger)
            return;

        var availableHunger = hunger - ent.Comp1.MinHunger;
        var hungerCost = 0f;

        UpdatePassiveRepair(ent, ref hungerCost, availableHunger);
        UpdateBloodRegeneration(ent, ref hungerCost, availableHunger);

        if (hungerCost > 0f)
            _hunger.ModifyHunger(ent.Owner, -MathF.Min(hungerCost, availableHunger), ent.Comp2);
    }

    private void UpdatePassiveRepair(
        Entity<SynthBloodstreamComponent, HungerComponent, BloodstreamComponent, DamageableComponent> ent,
        ref float hungerCost,
        float availableHunger)
    {
        var efficiency = GetBloodEfficiency(ent.Comp1, _bloodstream.GetBloodLevel((ent.Owner, ent.Comp3)));
        if (efficiency <= 0f)
            return;

        var repair = BuildPassiveRepair(ent, efficiency);
        LimitRepairByHunger(repair, ent.Comp1.HungerCostPerRepair, availableHunger);

        if (!repair.Empty
            && _damageable.TryChangeDamage((ent.Owner, ent.Comp4), repair, out var repaired, true, false))
        {
            hungerCost += MathF.Max(0f, -repaired.GetTotal().Float()) * ent.Comp1.HungerCostPerRepair;
        }

        var remainingHunger = MathF.Max(0f, availableHunger - hungerCost);
        if (ent.Comp1.BleedReductionAmount <= 0f || ent.Comp3.BleedAmount <= 0f || remainingHunger <= 0f)
            return;

        var sealedAmount = MathF.Min(ent.Comp3.BleedAmount, ent.Comp1.BleedReductionAmount * efficiency);
        if (ent.Comp1.HungerCostPerBleed > 0f)
            sealedAmount = MathF.Min(sealedAmount, remainingHunger / ent.Comp1.HungerCostPerBleed);

        if (_bloodstream.TryModifyBleedAmount((ent.Owner, ent.Comp3), -sealedAmount))
            hungerCost += sealedAmount * ent.Comp1.HungerCostPerBleed;
    }

    private void UpdateBloodRegeneration(
        Entity<SynthBloodstreamComponent, HungerComponent, BloodstreamComponent, DamageableComponent> ent,
        ref float hungerCost,
        float availableHunger)
    {
        var regeneration = ent.Comp1.BloodRegeneration;
        if (_bloodstream.GetBloodLevel((ent.Owner, ent.Comp3)) >= regeneration.TargetBloodLevel)
            return;

        var amount = regeneration.BloodRefreshAmount;
        if (regeneration.HungerCostPerUnit > 0f)
        {
            var remainingHunger = MathF.Max(0f, availableHunger - hungerCost);
            amount = FixedPoint2.Min(amount, FixedPoint2.New(remainingHunger / regeneration.HungerCostPerUnit));
        }

        if (amount <= FixedPoint2.Zero || !_bloodstream.TryModifyBloodLevel((ent.Owner, ent.Comp3), amount))
            return;

        hungerCost += amount.Float() * regeneration.HungerCostPerUnit;
    }

    private DamageSpecifier BuildPassiveRepair(Entity<SynthBloodstreamComponent, HungerComponent, BloodstreamComponent, DamageableComponent> ent, float efficiency)
    {
        var repair = ent.Comp1.PassiveRepair;
        repair.DamageDict.Clear();

        foreach (var (type, amount) in ent.Comp1.Damage.Types)
        {
            if (!ent.Comp4.Damage.DamageDict.TryGetValue(type, out var current) || current <= FixedPoint2.Zero)
                continue;

            var scaled = amount * efficiency;
            if (scaled != FixedPoint2.Zero)
                repair.DamageDict[type] = scaled;
        }

        foreach (var (groupId, amount) in ent.Comp1.Damage.Groups)
        {
            if (!_prototype.TryIndex(groupId, out var group))
                continue;

            var scaled = amount * efficiency;
            if (scaled != FixedPoint2.Zero)
                AddGroupDamage(repair, group, scaled, ent.Comp4);
        }

        return repair;
    }

    private static void AddGroupDamage(DamageSpecifier repair, DamageGroupPrototype group, FixedPoint2 amount, DamageableComponent damageable)
    {
        var damagedTypes = 0;
        foreach (var type in group.DamageTypes)
        {
            if (damageable.Damage.DamageDict.TryGetValue(type, out var current) && current > FixedPoint2.Zero)
                damagedTypes++;
        }

        if (damagedTypes == 0)
            return;

        var remaining = amount;
        foreach (var type in group.DamageTypes)
        {
            if (!damageable.Damage.DamageDict.TryGetValue(type, out var current) || current <= FixedPoint2.Zero)
                continue;

            var share = remaining / FixedPoint2.New(damagedTypes);
            repair.DamageDict[type] = repair.DamageDict.GetValueOrDefault(type) + share;
            remaining -= share;
            damagedTypes--;
        }
    }

    private static void LimitRepairByHunger(DamageSpecifier repair, float hungerCostPerRepair, float availableHunger)
    {
        if (hungerCostPerRepair <= 0f)
            return;

        var availableRepair = FixedPoint2.New(availableHunger / hungerCostPerRepair);
        if (availableRepair <= FixedPoint2.Zero)
        {
            repair.DamageDict.Clear();
            return;
        }

        var totalRepair = FixedPoint2.Zero;
        foreach (var amount in repair.DamageDict.Values)
        {
            if (amount < FixedPoint2.Zero)
                totalRepair -= amount;
        }

        if (totalRepair <= availableRepair)
            return;

        var scale = availableRepair / totalRepair;
        foreach (var type in repair.DamageDict.Keys)
        {
            if (repair.DamageDict[type] < FixedPoint2.Zero)
                repair.DamageDict[type] *= scale;
        }
    }

    private static float GetBloodEfficiency(SynthBloodstreamComponent component, float bloodLevel)
    {
        if (bloodLevel < component.MinBloodLevel)
            return 0f;

        if (bloodLevel >= component.FullEfficiencyBloodLevel || component.FullEfficiencyBloodLevel <= component.MinBloodLevel)
            return 1f;

        var progress = (bloodLevel - component.MinBloodLevel) / (component.FullEfficiencyBloodLevel - component.MinBloodLevel);
        return MathHelper.Lerp(component.MinBloodEfficiency, 1f, Math.Clamp(progress, 0f, 1f));
    }
}
