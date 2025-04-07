using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.PowerCell.Components;

namespace Content.Server._White.Blocking;

public sealed class RechargeableBlockingSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RechargeableBlockingComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<RechargeableBlockingComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<RechargeableBlockingComponent, ItemToggleActivateAttemptEvent>(AttemptToggle);
        SubscribeLocalEvent<RechargeableBlockingComponent, ChargeChangedEvent>(OnChargeChanged);
        SubscribeLocalEvent<RechargeableBlockingComponent, PowerCellChangedEvent>(OnPowerCellChanged);
    }

    private void OnExamined(EntityUid uid, RechargeableBlockingComponent component, ExaminedEvent args)
    {
        if (!component.Discharged)
        {
            _powerCell.OnBatteryExamined(uid, null, args);
            return;
        }

        args.PushMarkup(Loc.GetString("rechargeable-blocking-discharged"));
        args.PushMarkup(Loc.GetString("rechargeable-blocking-remaining-time", ("remainingTime", GetRemainingTime(uid))));
    }

    private int GetRemainingTime(EntityUid uid)
    {
        if (!_battery.TryGetBatteryComponent(uid, out var batteryComponent, out var batteryUid)
            || !TryComp<BatterySelfRechargerComponent>(batteryUid, out var recharger)
            || recharger is not { AutoRechargeRate: > 0, AutoRecharge: true })
            return 0;

        return (int) MathF.Round((batteryComponent.MaxCharge - batteryComponent.CurrentCharge) /
                                 recharger.AutoRechargeRate);
    }

    private void OnDamageChanged(EntityUid uid, RechargeableBlockingComponent component, DamageChangedEvent args)
    {
        if (!_battery.TryGetBatteryComponent(uid, out var batteryComponent, out var batteryUid)
            || !_itemToggle.IsActivated(uid)
            || args.DamageDelta == null)
            return;

        var batteryUse = Math.Min(args.DamageDelta.GetTotal().Float(), batteryComponent.CurrentCharge);
        _battery.TryUseCharge(batteryUid.Value, batteryUse, batteryComponent);
    }

    private void AttemptToggle(EntityUid uid, RechargeableBlockingComponent component, ref ItemToggleActivateAttemptEvent args)
    {
        if (!component.Discharged)
            return;

        args.Popup = Loc.GetString("rechargeable-blocking-remaining-time-popup",
            ("remainingTime", GetRemainingTime(uid)));
        args.Cancelled = true;
    }
    private void OnChargeChanged(EntityUid uid, RechargeableBlockingComponent component, ChargeChangedEvent args)
    {
        CheckCharge(uid, component);
    }

    private void OnPowerCellChanged(EntityUid uid, RechargeableBlockingComponent component, PowerCellChangedEvent args)
    {
        CheckCharge(uid, component);
    }

    private void CheckCharge(EntityUid uid, RechargeableBlockingComponent component)
    {
        if (!_battery.TryGetBatteryComponent(uid, out var battery, out _))
            return;

        BatterySelfRechargerComponent? recharger;
        if (battery.CurrentCharge < 1)
        {
            if (TryComp(uid, out recharger))
                recharger.AutoRechargeRate = component.DischargedRechargeRate;

            component.Discharged = true;
            _itemToggle.TryDeactivate(uid, predicted: false);
            return;
        }

        if (battery.CurrentCharge < battery.MaxCharge)
            return;

        component.Discharged = false;
        if (TryComp(uid, out recharger))
                recharger.AutoRechargeRate = component.ChargedRechargeRate;
    }
}
