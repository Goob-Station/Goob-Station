using System.Linq;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Hypospray.Events;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Chemistry.Hypospray;

public sealed partial class HyposprayWhitelistSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HyposprayWhitelistComponent, BeforeHyposprayDrawEvent>(BeforeHyposprayDraw);
        SubscribeLocalEvent<HyposprayWhitelistComponent, BeforeHyposprayInjectsEvent>(BeforeHyposprayInjects);
    }

    public bool IsValid(Solution solution, HyposprayWhitelistComponent comp)
    {
        return !solution.Contents.Select(reagent => new ProtoId<ReagentPrototype>(reagent.Reagent.Prototype))
            .Except(comp.Whitelist).Any();
    }

    private void BeforeHyposprayDraw(Entity<HyposprayWhitelistComponent> ent, ref BeforeHyposprayDrawEvent args)
    {
        if (args.Cancelled)
            return;

        if (!IsValid(args.Soln.Comp.Solution, ent.Comp))
        {
            args.Cancel();
            _popup.PopupClient(Loc.GetString(ent.Comp.DrawFailureMessage), args.Target, args.User);
        }
    }

    private void BeforeHyposprayInjects(Entity<HyposprayWhitelistComponent> ent, ref BeforeHyposprayInjectsEvent args)
    {
        if (args.Cancelled)
            return;

        // non-mob injections are allowed so that you can empty the hypospray
        if (HasComp<MobStateComponent>(args.TargetGettingInjected)
            && TryComp<HyposprayComponent>(ent, out var hypo)
            && _solution.TryGetSolution(ent.Owner, hypo.SolutionName, out _, out var solution)
            && !IsValid(solution, ent.Comp))
        {
            args.InjectMessageOverride = ent.Comp.InjectFailureMessage;
            args.Cancel();
        }
    }
}
