using Content.Server._Lavaland.Mobs.Hierophant.Components;
using Content.Shared.Alert;
using Content.Shared.Movement.Systems;

namespace Content.Server._Lavaland.Mobs.Hierophant;

public sealed class HierophantBeatSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantBeatComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HierophantBeatComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<HierophantBeatComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    private void OnStartup(EntityUid uid, HierophantBeatComponent component, ref ComponentStartup args)
    {
        _alertsSystem.ShowAlert(uid, component.HierophantBeatAlertKey);
    }

    private void OnRemove(EntityUid uid, HierophantBeatComponent component, ref ComponentRemove args)
    {
        _alertsSystem.ClearAlert(uid, component.HierophantBeatAlertKey);
    }

    private void OnRefreshSpeed(EntityUid uid, HierophantBeatComponent component, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.MovementSpeedBuff, component.MovementSpeedBuff);
    }
}
