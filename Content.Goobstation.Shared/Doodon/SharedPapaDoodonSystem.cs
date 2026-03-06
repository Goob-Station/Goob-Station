using Content.Shared.Actions;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Doodons;

public abstract partial class SharedPapaDoodonSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PapaDoodonComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PapaDoodonComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMapInit(EntityUid uid, PapaDoodonComponent comp, ref MapInitEvent args)
    {
        _actions.AddAction(uid, ref comp.OrderStayActionEntity, comp.OrderStayAction);
        _actions.AddAction(uid, ref comp.OrderFollowActionEntity, comp.OrderFollowAction);
        _actions.AddAction(uid, ref comp.OrderAttackActionEntity, comp.OrderAttackAction);
        _actions.AddAction(uid, ref comp.OrderLooseActionEntity, comp.OrderLooseAction);

        UpdateOrderToggles(uid, comp);
    }

    private void OnShutdown(EntityUid uid, PapaDoodonComponent comp, ref ComponentShutdown args)
    {
        Remove(uid, comp.OrderStayActionEntity);
        Remove(uid, comp.OrderFollowActionEntity);
        Remove(uid, comp.OrderAttackActionEntity);
        Remove(uid, comp.OrderLooseActionEntity);

        comp.OrderStayActionEntity = null;
        comp.OrderFollowActionEntity = null;
        comp.OrderAttackActionEntity = null;
        comp.OrderLooseActionEntity = null;
    }

    private void Remove(EntityUid uid, EntityUid? act)
    {
        if (act is { } a)
            _actions.RemoveAction(uid, a);
    }

    protected void UpdateOrderToggles(EntityUid uid, PapaDoodonComponent comp)
    {
        // This is optional. If your action system automatically uses iconOn when “toggled”,
        // you’ll want to set the toggled state. If not, you can ignore this and just rely on the click.
        // (Exact API differs by fork.)
    }
}
