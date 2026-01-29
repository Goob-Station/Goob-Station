using Content.Shared.Actions;
using Content.Shared.PowerCell.Components;
using Content.Server.PowerCell;
using Content.Shared.Alert;
using Content.Shared.Rounding;
using Content.Shared.Clothing;

namespace Content.Goobstation.Server.ModSuits;

public sealed class ShowEnergyAlarmSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly PowerCellSystem _powerCellSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<EnergyAlertComponent, PowerCellChangedEvent>(OnPowerCellUpdate);

        SubscribeLocalEvent<EnergyAlertComponent, ClothingGotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<EnergyAlertComponent, ClothingGotUnequippedEvent>(OnUnequipped);
    }
    private void OnPowerCellUpdate(EntityUid uid, EnergyAlertComponent component, PowerCellChangedEvent args)
    {
        UpdateClothingPowerAlert((uid, component), component.User.HasValue);
    }

    private void OnEquipped(EntityUid uid, EnergyAlertComponent component, ref ClothingGotEquippedEvent args)
    {
        component.User = args.Wearer;
        UpdateClothingPowerAlert((uid, component), true);
    }

    private void OnUnequipped(EntityUid uid, EnergyAlertComponent component, ref ClothingGotUnequippedEvent args)
    {
        UpdateClothingPowerAlert((uid, component), false);
        component.User = null;
    }

    private void UpdateClothingPowerAlert(Entity<EnergyAlertComponent> entity, bool equipped)
    {
        if (entity.Comp.User == null)
            return;

        if (!_powerCellSystem.TryGetBatteryFromSlot(entity, out var battery) || !equipped)
        {
            _alertsSystem.ClearAlert(entity.Comp.User.Value, entity.Comp.PowerAlert);
            return;
        }

        var severity = ContentHelpers.RoundToLevels(MathF.Max(0f, battery.CurrentCharge), battery.MaxCharge, 6);
        _alertsSystem.ShowAlert(entity.Comp.User.Value, entity.Comp.PowerAlert, (short) severity);
    }
}
