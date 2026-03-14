using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Werewolf.Abilities;
using Content.Goobstation.Shared.Werewolf.Abilities.Basic;
using Content.Shared.Body.Components;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Werewolf.Systems.Abilities;

/// <summary>
/// Handles Dire werewolf abilities
/// </summary>
public partial class WerewolfBasicAbilitiesSystem
{
    public void InitializeWerewolfDire()
    {
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, EventWerewolfBleedingBite>(TryBite);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, WerewolfBleedingBiteDoAfterEvent>(DoBite);
    }

    private void TryBite(EntityUid uid, WerewolfBasicAbilitiesComponent component, EventWerewolfBleedingBite args)
    {
        if (TryComp<MobStateComponent>(args.Target, out var mobState) && mobState.CurrentState == MobState.Dead) // to prevent wolves from biting corpses for heals and whatnot
        {
            _popup.PopupClient(Loc.GetString("werewolf-bite-fail-state"), uid, PopupType.Large);
            return;
        }

        _popup.PopupEntity(Loc.GetString("werewolf-bite-start", ("user", Identity.Entity(uid, EntityManager)), ("target", Identity.Entity(args.Target, EntityManager))), uid, PopupType.LargeCaution); // todo locale

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(1), new WerewolfBleedingBiteDoAfterEvent(), uid, args.Target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd
        });

        args.Handled = true;
    }

    private void DoBite(EntityUid uid, WerewolfBasicAbilitiesComponent comp, WerewolfBleedingBiteDoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null)
            return;

        SpillBloodPercentage(args.Target.Value, 30);
        TryRegen(uid, comp, new EventWerewolfRegen()); // goida

        args.Handled = true;
    }

    private void SpillBloodPercentage(EntityUid uid, int percentage)
    {
        if (percentage <= 0 || percentage > 100 || !TryComp<BloodstreamComponent>(uid, out var stream))
            return;

        if (!_solution.ResolveSolution(uid, stream.BloodSolutionName, ref stream.BloodSolution, out var solution))
            return;

        var blood = _solution.SplitSolution(stream.BloodSolution.Value, solution.Volume * (percentage / 100f));

        if (blood.Volume > FixedPoint2.Zero)
            _puddle.TrySpillAt(uid, blood, out _);
    }
}
