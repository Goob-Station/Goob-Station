using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Server.Cuffs;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Cuffs.Components;
using Content.Shared.Popups;
using Content.Server.Actions;

namespace Content.Goobstation.Server.Slasher.Systems;

public sealed class SlasherRegenerateSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private readonly CuffableSystem _cuffs = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlasherRegenerateComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherRegenerateComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherRegenerateComponent, SlasherRegenerateEvent>(OnRegenerate);
    }

    private void OnMapInit(Entity<SlasherRegenerateComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherRegenerateComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEnt);
    }

    private void OnRegenerate(EntityUid uid, SlasherRegenerateComponent comp, SlasherRegenerateEvent args)
    {
        if (args.Handled)
            return;

        TryInjectReagent(uid, comp);

        if (TryComp<CuffableComponent>(uid, out var cuffs) && cuffs.Container.ContainedEntities.Count > 0)
        {
            var cuff = cuffs.LastAddedCuffs;
            _cuffs.Uncuff(uid, cuffs.LastAddedCuffs, cuff);
            QueueDel(cuff);
        }

        args.Handled = true;
    }

    private void TryInjectReagent(EntityUid target, SlasherRegenerateComponent comp)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return;

        if (!_solutions.ResolveSolution(target, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution))
            return;

        _solutions.TryAddReagent(bloodstream.ChemicalSolution.Value, new ReagentId(comp.Reagent, null), FixedPoint2.New(comp.ReagentAmount), out _);
    }
}
