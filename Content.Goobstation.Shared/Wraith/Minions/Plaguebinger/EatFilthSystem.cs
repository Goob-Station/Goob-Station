using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Fluids.Components;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Wraith.Minions.Plaguebinger;
public sealed class EatFilthSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EatFilthComponent, EatFilthEvent>(OnEat);
        SubscribeLocalEvent<EatFilthComponent, EatFilthDoAfterEvent>(OnEatDoAfter);
    }

    private void OnEat(Entity<EatFilthComponent> ent, ref EatFilthEvent args)
    {
        if (!_entityWhitelist.IsWhitelistPass(ent.Comp.AllowedEntities, args.Target))
            return; // TODO: Popup fail here

        // TODO: Popup
        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.EatDuration,
            new EatFilthDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnEatDoAfter(Entity<EatFilthComponent> ent, ref EatFilthDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Handled
            || args.Target is not { } target)
            return;

        // First, check if its compatible reagent
        if (!CanEatTarget(ent, target))
            return;

        ent.Comp.FilthConsumed++;
        Dirty(ent);

        var ev = new AteFilthEvent(ent.Comp.FilthConsumed);
        RaiseLocalEvent(ent.Owner, ref ev);

        PredictedQueueDel(args.Target);
    }

    #region Helper
    private bool CanEatTarget(Entity<EatFilthComponent> ent, EntityUid target)
    {
        // first, check for puddles
        if (TryComp<PuddleComponent>(target, out var puddle) && ent.Comp.AllowedReagents is { } allowedReagents)
        {
            if (!_solutionContainer.ResolveSolution(target, puddle.SolutionName, ref puddle.Solution, out var solution))
                return false;

            foreach (var reagent in solution.Contents)
            {
                if (!allowedReagents.Contains(reagent.Reagent.Prototype))
                    continue;

                return true;
            }
        }

        // then if its not a puddle, just delete check if it passes the whitelist and delete it
        if (_entityWhitelist.IsWhitelistPass(ent.Comp.AllowedEntities, target))
            return true;

        return false;
    }
    #endregion
}
