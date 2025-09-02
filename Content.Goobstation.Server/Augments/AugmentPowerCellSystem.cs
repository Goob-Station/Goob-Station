using Content.Goobstation.Shared.Augments;
using Content.Server.Power.Components; // ough BatteryComponent why are you in server
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared.Alert;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Augments;

public sealed class AugmentPowerCellSystem : SharedAugmentPowerCellSystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly new PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private TimeSpan _nextUpdate = TimeSpan.Zero;
    private static readonly TimeSpan _updateDelay = TimeSpan.FromSeconds(2);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HasAugmentPowerCellSlotComponent, FindBatteryEvent>(OnFindBattery);
        SubscribeLocalEvent<HasAugmentPowerCellSlotComponent, AugmentBatteryAlertEvent>(OnBatteryAlert);
    }

    private void OnFindBattery(Entity<HasAugmentPowerCellSlotComponent> ent, ref FindBatteryEvent args)
    {
        if (GetBodyCell(ent) is {} battery)
            args.FoundBattery = battery;
    }

    private void OnBatteryAlert(Entity<HasAugmentPowerCellSlotComponent> ent, ref AugmentBatteryAlertEvent args)
    {
        var user = args.User;
        if (GetBodyAugment(ent) is not {} augment || GetAugmentCell(augment) is not {} battery)
        {
            _popup.PopupEntity(Loc.GetString("power-cell-no-battery"), user, user, PopupType.MediumCaution);
            return;
        }

        var percent = 100f * battery.Comp.CurrentCharge / battery.Comp.MaxCharge;
        var draw = CompOrNull<PowerCellDrawComponent>(augment)?.DrawRate ?? 0f;
        _popup.PopupEntity(Loc.GetString("augments-power-cell-info", ("percent", $"{percent:F0}"), ("draw", draw)), user, user);
    }

    /// <summary>
    /// Get a power cell slot augment's installed power cell.
    /// Returns null if the slot is empty.
    /// </summary>
    public Entity<BatteryComponent>? GetAugmentCell(EntityUid augment)
    {
        if (_powerCell.TryGetBatteryFromSlot(augment, out var battery, out var comp))
            return (battery.Value, comp);

        return null;
    }

    /// <summary>
    /// Gets a power cell for a body if it both:
    /// 1. has a power cell slot augment
    /// 2. that augment has a power cell installed
    /// Returns null otherwise.
    /// </summary>
    public Entity<BatteryComponent>? GetBodyCell(EntityUid body)
    {
        if (GetBodyAugment(body) is {} augment && GetAugmentCell(augment) is {} battery)
            return battery;

        return null;
    }

    /// <summary>
    /// Tries to use charge from a body's power cell slot augment.
    /// Does a popup for the user if it fails.
    /// </summary>
    public bool TryUseChargeBody(EntityUid body, float amount)
    {
        if (GetBodyAugment(body) is not {} slot)
        {
            _popup.PopupEntity(Loc.GetString("augments-no-power-cell-slot"), body, body, PopupType.MediumCaution);
            return false;
        }

        if (GetAugmentCell(slot) is not {} battery)
        {
            _popup.PopupEntity(Loc.GetString("power-cell-no-battery"), body, body, PopupType.MediumCaution);
            return false;
        }

        if (!_battery.TryUseCharge(battery.Owner, amount, battery.Comp))
        {
            _popup.PopupEntity(Loc.GetString("power-cell-insufficient"), body, body, PopupType.MediumCaution);
            return false;
        }

        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // don't need to burn server tps on alerts
        var now = _timing.CurTime;
        if (now < _nextUpdate)
            return;

        _nextUpdate = now + _updateDelay;

        var query = EntityQueryEnumerator<HasAugmentPowerCellSlotComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (_mob.IsDead(uid) || GetBodyAugment(uid) is not {} augment)
                continue;

            if (GetAugmentCell(augment) is not {} battery)
            {
                if (_alerts.IsShowingAlert(uid, augment.Comp.BatteryAlert))
                {
                    _alerts.ClearAlert(uid, augment.Comp.BatteryAlert);
                    _alerts.ShowAlert(uid, augment.Comp.NoBatteryAlert);
                }
                continue;
            }

            _alerts.ClearAlert(uid, augment.Comp.NoBatteryAlert);

            var chargePercent = (short) MathF.Round(battery.Comp.CurrentCharge / battery.Comp.MaxCharge * 10f);
            _alerts.ShowAlert(uid, augment.Comp.BatteryAlert, chargePercent);
        }
    }
}
