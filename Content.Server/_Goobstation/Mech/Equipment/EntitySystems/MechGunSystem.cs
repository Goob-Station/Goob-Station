using Content.Server.Mech.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Mech.Equipment.Components;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Server.Mech.Equipment.EntitySystems;
public sealed class MechGunSystem : EntitySystem
{
    [Dependency] private readonly MechSystem _mech = default!;
    [Dependency] private readonly BatterySystem _battery = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MechEquipmentComponent, HandleMechEquipmentBatteryEvent>(OnHandleMechEquipmentBattery);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, CheckMechWeaponBatteryEvent>(OnCheckBattery);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, CheckMechWeaponBatteryEvent>(OnCheckBattery);
    }

    private void OnHandleMechEquipmentBattery(EntityUid uid, MechEquipmentComponent component, HandleMechEquipmentBatteryEvent args)
    {
        if (!component.EquipmentOwner.HasValue)
            return;

        if (!TryComp<MechComponent>(component.EquipmentOwner.Value, out var mech))
            return;

        if (TryComp<BatteryComponent>(uid, out var battery))
        {
            var ev = new CheckMechWeaponBatteryEvent(battery);
            RaiseLocalEvent(uid, ref ev);

            if (ev.Cancelled)
                return;

            ChargeGunBattery(uid, battery);
        }
    }

    private void OnCheckBattery(EntityUid uid, BatteryAmmoProviderComponent component, CheckMechWeaponBatteryEvent args)
    {
        if (args.Battery.CurrentCharge > component.FireCost)
            args.Cancelled = true;
    }

    private void ChargeGunBattery(EntityUid uid, BatteryComponent component)
    {
        if (!TryComp<MechEquipmentComponent>(uid, out var mechEquipment) || !mechEquipment.EquipmentOwner.HasValue)
            return;

        if (!TryComp<MechComponent>(mechEquipment.EquipmentOwner.Value, out var mech))
            return;

        var maxCharge = component.MaxCharge;
        var currentCharge = component.CurrentCharge;

        var chargeDelta = maxCharge - currentCharge;

        // TODO: The battery charge of the mech would be spent directly when fired.
        if (chargeDelta <= 0 || mech.Energy - chargeDelta < 0)
            return;

        if (!_mech.TryChangeEnergy(mechEquipment.EquipmentOwner.Value, -chargeDelta, mech))
            return;

        _battery.SetCharge(uid, component.MaxCharge, component);
    }
}

[ByRefEvent]
public record struct CheckMechWeaponBatteryEvent(BatteryComponent Battery, bool Cancelled = false);
