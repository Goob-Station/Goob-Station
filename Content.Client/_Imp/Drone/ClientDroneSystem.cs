using Content.Client.Popups;
using Content.Shared._Imp.Drone;
using Content.Shared.Alert;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client._Imp.Drone;

/// <summary>
/// Clientside part of the SharedDroneSystem.
/// Mainly handles updating the battery alert.
/// Heavily based on <see cref="Content.Client.Silicons.Borgs.BorgSystem">BorgSystem.Battery.cs</see>.
/// </summary>
public sealed class ClientDroneSystem : SharedDroneSystem
{
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    
    // How often to update the battery alert.
    // Also gets updated instantly when switching bodies or a battery is inserted or removed.
    private static readonly TimeSpan AlertUpdateDelay = TimeSpan.FromSeconds(0.5f);

    // Don't put this on the component because we only need to track the time for a single entity
    // and we don't want to TryComp it every single tick.
    private TimeSpan _nextAlertUpdate = TimeSpan.Zero;
    private EntityQuery<DroneComponent> _chassisQuery;
    private EntityQuery<PowerCellSlotComponent> _slotQuery;

    public override void Initialize()
    {
        base.Initialize();

        InitializeBattery();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateBattery();
    }

    private void InitializeBattery()
    {
        SubscribeLocalEvent<DroneComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<DroneComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _chassisQuery = GetEntityQuery<DroneComponent>();
        _slotQuery = GetEntityQuery<PowerCellSlotComponent>();
    }

    private void OnPlayerAttached(Entity<DroneComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        UpdateBatteryAlert((ent.Owner, ent.Comp, null));
    }

    private void OnPlayerDetached(Entity<DroneComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        // Remove all batteru related alerts.
        _alerts.ClearAlert(ent.Owner, ent.Comp.BatteryAlert);
        _alerts.ClearAlert(ent.Owner, ent.Comp.NoBatteryAlert);
    }

    private void UpdateBatteryAlert(Entity<DroneComponent, PowerCellSlotComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp2))
            return;

        if (!_powerCell.TryGetBatteryFromSlot((ent.Owner, ent.Comp2), out var battery))
        {
            _alerts.ShowAlert(ent.Owner, ent.Comp1.NoBatteryAlert);
            return;
        }

        // Alert levels from 0 to 10.
        var chargePercent = (short)MathF.Round(_battery.GetChargeLevel(battery.Value.AsNullable()) * 10f);

        if (chargePercent == 5 && chargePercent < ent.Comp1.LastChargePercent)
        {
            if (_gameTiming.CurTime >= ent.Comp1.NextProximityAlert)
            {
                _popupSystem.PopupEntity(Loc.GetString("drone-med-battery"), ent.Owner, ent.Owner, PopupType.MediumCaution);
                ent.Comp1.NextProximityAlert = _gameTiming.CurTime + ent.Comp1.ProximityDelay;
            }
        }

        if (chargePercent == 2 && chargePercent < ent.Comp1.LastChargePercent)
        {
            if (_gameTiming.CurTime >= ent.Comp1.NextProximityAlert)
            {
                _popupSystem.PopupEntity(Loc.GetString("drone-low-battery"), ent.Owner, ent.Owner, PopupType.LargeCaution);
                ent.Comp1.NextProximityAlert = _gameTiming.CurTime + ent.Comp1.ProximityDelay;
            }
        }

        // we make sure 0 only shows if they have absolutely no battery.
        // also account for floating point imprecision
        if (chargePercent == 0 && _powerCell.HasDrawCharge(ent.Owner))
            chargePercent = 1;

        ent.Comp1.LastChargePercent = chargePercent;

        _alerts.ClearAlert(ent.Owner, ent.Comp1.NoBatteryAlert);
        _alerts.ShowAlert(ent.Owner, ent.Comp1.BatteryAlert, chargePercent);
    }

    // Periodically update the charge indicator.
    // We do this with a client-side alert so that we don't have to network the charge level.
    private void UpdateBattery()
    {
        if (_player.LocalEntity is not { } localPlayer)
            return;

        var curTime = _gameTiming.CurTime;

        if (curTime < _nextAlertUpdate)
            return;

        _nextAlertUpdate = curTime + AlertUpdateDelay;

        if (!_chassisQuery.TryComp(localPlayer, out var chassis) || !_slotQuery.TryComp(localPlayer, out var slot))
            return;

        UpdateBatteryAlert((localPlayer, chassis, slot));
    }
}