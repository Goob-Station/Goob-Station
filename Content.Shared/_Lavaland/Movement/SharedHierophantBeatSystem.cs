using Content.Shared._vg.TileMovement;
using Content.Shared.Alert;
using Content.Shared.Movement.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Movement;

public abstract class SharedHierophantBeatSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantBeatComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HierophantBeatComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<HierophantBeatComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    private void OnStartup(EntityUid uid, HierophantBeatComponent component, ref ComponentStartup args)
    {
        EnsureComp<TileMovementComponent>(uid);
        ShowAlert(uid, component.HierophantBeatAlertKey);
    }

    private void OnRemove(EntityUid uid, HierophantBeatComponent component, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(uid))
            return;

        RemComp<TileMovementComponent>(uid);
        ClearAlert(uid, component.HierophantBeatAlertKey);
    }

    private void OnRefreshSpeed(EntityUid uid, HierophantBeatComponent component, ref RefreshMovementSpeedModifiersEvent args)
        => args.ModifySpeed(component.MovementSpeedBuff, component.MovementSpeedBuff);

    protected virtual void ShowAlert(EntityUid uid, ProtoId<AlertPrototype> alertId) { }

    protected virtual void ClearAlert(EntityUid uid, ProtoId<AlertPrototype> alertId) { }
}
