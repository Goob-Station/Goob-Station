// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Pirate.Blood.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Timing;

namespace Content.Server._Pirate.Blood;

public sealed class BloodRegenerationPirateSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<BloodstreamComponent>();
        while (query.MoveNext(out var uid, out var blood))
        {
            // Align with bloodstream tick: run only when its tick is due (before or after base system advances it).
            if (curTime < blood.NextUpdate)
                continue;

            if (_mobState.IsDead(uid))
                continue;

            if (!_solutions.ResolveSolution(uid, blood.BloodSolutionName, ref blood.BloodSolution, out var bloodSolution))
                continue;

            TryDoNaturalRegeneration((uid, blood), bloodSolution).ToString();
        }
    }

    private bool TryDoNaturalRegeneration(Entity<BloodstreamComponent> ent, Solution bloodSolution)
    {
        // Modify via event
        var ev = new NaturalBloodRegenerationAttemptEvent { Amount = ent.Comp.BloodRefreshAmount };
        RaiseLocalEvent(ent, ref ev);
        if (ev.Cancelled)
            return false;
        // Amount scaled by component; allows negative for loss if desired.
        var amount = ev.Amount;

        if (amount > FixedPoint2.Zero && _bloodstream.GetBloodLevel(ent.AsNullable()) >= 1f)
            return false;

        // Costs
        var usedHunger = amount * ent.Comp.BloodRegenerationHunger;
        var usedThirst = amount * ent.Comp.BloodRegenerationThirst;
        // Check resources
        var hungerComp = CompOrNull<HungerComponent>(ent);
        var thirstComp = CompOrNull<ThirstComponent>(ent);
        if ((usedHunger > FixedPoint2.Zero && hungerComp is not null && (_hunger.GetHunger(hungerComp) < usedHunger || hungerComp.CurrentThreshold <= HungerThreshold.Starving))
            || (usedThirst > FixedPoint2.Zero && thirstComp is not null && (thirstComp.CurrentThirst < usedThirst || thirstComp.CurrentThirstThreshold <= ThirstThreshold.Parched)))
            return false;

        // Spend and apply
        if (usedHunger > FixedPoint2.Zero && hungerComp is not null)
            _hunger.ModifyHunger(ent, (float)-usedHunger, hungerComp);

        if (usedThirst > FixedPoint2.Zero && thirstComp is not null)
            _thirst.ModifyThirst(ent, thirstComp, (float)-usedThirst);
        if (amount > FixedPoint2.Zero)
        {
            return _bloodstream.TryModifyBloodLevel(ent.AsNullable(), amount);
        }

        // If we do it by _bloodstream.TryModifyBloodLevel, it will create blood puddles, soo we do it manually
        if (amount < FixedPoint2.Zero)
        {
            if (ent.Comp.BloodSolution == null || ent.Comp.BloodReferenceSolution.Contents.Count == 0)
                return false;

            var bloodReagent = ent.Comp.BloodReferenceSolution.Contents[0].Reagent.Prototype;
            return _solutions.RemoveReagent(
                       ent.Comp.BloodSolution.Value,
                       new ReagentId(bloodReagent, _bloodstream.GetEntityBloodData(ent.Owner)),
                       -amount) > FixedPoint2.Zero;
        }

        return false;
    }
}
