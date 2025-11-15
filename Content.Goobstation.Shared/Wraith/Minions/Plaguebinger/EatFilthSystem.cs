using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Fluids.Components;
using Content.Shared.Popups;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Wraith.Minions.Plaguebringer;
public sealed class EatFilthSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EatFilthComponent, EatFilthEvent>(OnEat);
        SubscribeLocalEvent<EatFilthComponent, EatFilthDoAfterEvent>(OnEatDoAfter);
    }

    private void OnEat(Entity<EatFilthComponent> ent, ref EatFilthEvent args)
    {
        if (!_entityWhitelist.IsWhitelistPass(ent.Comp.AllowedEntities, args.Target))
        {
            _popup.PopupClient(Loc.GetString("wraith-plaguerat-eat-not-satisfy"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

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

        _popup.PopupClient(Loc.GetString("wraith-plaguerat-eat-you-start", ("target", args.Target)), ent.Owner, ent.Owner);
        args.Handled = true;
    }

    private void OnEatDoAfter(Entity<EatFilthComponent> ent, ref EatFilthDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled|| args.Target is not { } target)
        {
            _popup.PopupClient(Loc.GetString("wraith-plaguerat-eat-interrupt"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        // First, check if its compatible reagent
        if (!CanEatTarget(ent, target))
        {
            _popup.PopupClient(Loc.GetString("wraith-plaguerat-eat-not-satisfy"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        ent.Comp.FilthConsumed++;
        Dirty(ent);

        var ev = new AteFilthEvent(ent.Comp.FilthConsumed);
        RaiseLocalEvent(ent.Owner, ref ev);

        PredictedQueueDel(args.Target);

        _popup.PopupClient(Loc.GetString("wraith-plaguerat-eat-you-finish", ("target", args.Target)), ent.Owner, ent.Owner);
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

        // then if its not a puddle, just check if it passes the whitelist
        if (_entityWhitelist.IsWhitelistPass(ent.Comp.AllowedEntities, target))
            return true;

        return false;
    }
    #endregion
}
