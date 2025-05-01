using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Robust.Shared.Network;

namespace Content.Shared._White.Blocking;

public abstract class SharedRechargeableBlockingSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RechargeableBlockingComponent, ItemToggleActivateAttemptEvent>(AttemptToggle);
    }

    private void AttemptToggle(EntityUid uid,
        RechargeableBlockingComponent component,
        ref ItemToggleActivateAttemptEvent args)
    {
        if (!component.Discharged)
            return;

        args.Cancelled = true;

        if (_net.IsClient || args.User == null)
            return;

        _popup.PopupEntity(Loc.GetString("rechargeable-blocking-remaining-time-popup",
                ("remainingTime", GetRemainingTime(uid))),
            args.User.Value,
            args.User.Value);
    }

    protected virtual int GetRemainingTime(EntityUid uid) { }
}
