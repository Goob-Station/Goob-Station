using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.DoAfter;

public sealed partial class CombatDoAfterSystem
{
    private void InitializeTriggers()
    {
        SubscribeLocalEvent<CombatDoAfterComponent, MeleeHitEvent>(OnHit);

        SubscribeLocalEvent<InjectorComponent, CombatSyringeTriggerEvent>(OnCombatSyringeHit);
    }

    private void OnCombatSyringeHit(Entity<InjectorComponent> ent, ref CombatSyringeTriggerEvent args)
    {
        if (args.Targets.Count == 0)
            return;

        var target = args.Targets[0];

        if (!HasComp<MobStateComponent>(target))
            return;

        if (!_solution.TryGetInjectableSolution(target, out var injectableSolution, out _))
            return;

        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out var soln, out var solution))
            return;

        if (solution.Volume > FixedPoint2.Zero && args.SolutionSplitFraction > 0f)
        {
            var fraction = MathF.Min(args.SolutionSplitFraction, 1f);
            var removedSolution = _solution.SplitSolution(soln.Value, solution.Volume * fraction);
            _reactiveSystem.DoEntityReaction(target, removedSolution, ReactionMethod.Injection);
            _solution.Inject(target, injectableSolution.Value, removedSolution);
        }

        args.BonusDamage = args.SyringeExtraDamage;

        if (_net.IsClient)
            return;

        _audio.PlayPvs(args.InjectSound, target);
        QueueDel(ent);
    }

    private void OnHit(Entity<CombatDoAfterComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        if (ent.Comp.Trigger is not CombatDoAfterMeleeHitEvent hitEvent)
            return;

        if (CheckDoAfter(ent, args.User, args.HitEntities))
        {
            hitEvent.Targets = args.HitEntities;
            hitEvent.BonusDamage = new();
            RaiseLocalEvent(ent, (object) hitEvent);
            args.BonusDamage = hitEvent.BonusDamage;
        }

        if (ent.Comp is { DoAfterId: not null, DoAfterUser: not null })
        {
            _doAfter.Cancel(ent.Comp.DoAfterUser.Value, ent.Comp.DoAfterId.Value);
            ent.Comp.DoAfterId = null;
            ent.Comp.DoAfterUser = null;
            Dirty(ent);
        }
    }
}
