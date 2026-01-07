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
        _actions.AddAction(uid, ref comp.CommandActionEntity, comp.CommandAction);

        // Set correct icon/text immediately
        UpdateCommandAction(uid, comp);
    }

    private void OnShutdown(EntityUid uid, PapaDoodonComponent comp, ref ComponentShutdown args)
    {
        if (comp.CommandActionEntity is { } actionEnt)
            _actions.RemoveAction(uid, actionEnt);

        comp.CommandActionEntity = null;
    }

    protected void UpdateCommandAction(EntityUid uid, PapaDoodonComponent comp)
    {
        // Use the ACTION ENTITY, not the prototype
        if (comp.CommandActionEntity is not { } actionEnt)
            return;

        var (iconState, name, desc) = comp.CurrentOrder switch
        {
            DoodonOrderType.Stay =>
                ("stay", "Order: Stay", "Servants hold position."),
            DoodonOrderType.Follow =>
                ("follow", "Order: Follow", "Servants follow you."),
            DoodonOrderType.AttackTarget =>
                ("attack", "Order: Attack", "Point at a target to attack."),
            _ =>
                ("loose", "Order: Loose", "Servants attack enemies freely."),
        };

        _actions.SetIcon(actionEnt,
            new SpriteSpecifier.Rsi(
                new ResPath("_Goobstation/Interface/doodon_orders.rsi"),
                iconState));
    }
}
