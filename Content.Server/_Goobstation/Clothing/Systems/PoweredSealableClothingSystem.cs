using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared._Goobstation.Clothing.Components;
using Content.Shared._Goobstation.Clothing.Systems;
using Content.Shared.Alert;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.Rounding;

namespace Content.Server._Goobstation.Clothing.Systems;

public sealed partial class PoweredSealableClothingSystem : SharedPoweredSealableClothingSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly PowerCellSystem _powerCellSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SealableClothingRequiresPowerComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnMovementSpeedChange);
        SubscribeLocalEvent<SealableClothingRequiresPowerComponent, PowerCellChangedEvent>(OnPowerCellChanged);
        SubscribeLocalEvent<SealableClothingRequiresPowerComponent, PowerCellSlotEmptyEvent>(OnPowerCellEmpty);
        SubscribeLocalEvent<SealableClothingRequiresPowerComponent, ClothingControlSealCompleteEvent>(OnRequiresPowerSealCompleteEvent);
        SubscribeLocalEvent<SealableClothingRequiresPowerComponent, InventoryRelayedEvent<FindInventoryBatteryEvent>>(OnFindInventoryBatteryEvent);

    }

    private void OnPowerCellChanged(Entity<SealableClothingRequiresPowerComponent> entity, ref PowerCellChangedEvent args)
    {
        if (!entity.Comp.IsPowered && _powerCellSystem.HasDrawCharge(entity))
        {
            entity.Comp.IsPowered = true;
            Dirty(entity);

            ModifySpeed(entity);
        }

        UpdateClothingPowerAlert(entity);
    }

    private void OnPowerCellEmpty(Entity<SealableClothingRequiresPowerComponent> entity, ref PowerCellSlotEmptyEvent args)
    {
        entity.Comp.IsPowered = false;
        Dirty(entity);

        ModifySpeed(entity);
    }

    /// <summary>
    /// Enables or disables power cell draw on seal/unseal complete
    /// </summary>
    private void OnRequiresPowerSealCompleteEvent(Entity<SealableClothingRequiresPowerComponent> entity, ref ClothingControlSealCompleteEvent args)
    {
        if (!TryComp(entity, out PowerCellDrawComponent? drawComp))
            return;

        _powerCellSystem.SetDrawEnabled((entity.Owner, drawComp), args.IsSealed);

        UpdateClothingPowerAlert(entity);
        ModifySpeed(entity);
    }

    private void OnMovementSpeedChange(Entity<SealableClothingRequiresPowerComponent> entity, ref InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (!TryComp(entity, out SealableClothingControlComponent? controlComp))
            return;

        // If suit is unsealed - don't care about penalty
        if (!controlComp.IsCurrentlySealed)
            return;

        if (!entity.Comp.IsPowered)
            args.Args.ModifySpeed(entity.Comp.MovementSpeedPenalty);
    }

    private void ModifySpeed(EntityUid uid)
    {
        if (!TryComp(uid, out SealableClothingControlComponent? controlComp) || controlComp.WearerEntity == null)
            return;

        _movementSpeed.RefreshMovementSpeedModifiers(controlComp.WearerEntity.Value);
    }

    /// <summary>
    /// Sets power alert to wearer when clothing is sealed
    /// </summary>
    private void UpdateClothingPowerAlert(Entity<SealableClothingRequiresPowerComponent> entity)
    {
        var (uid, comp) = entity;

        if (!TryComp<SealableClothingControlComponent>(uid, out var controlComp) || controlComp.WearerEntity == null)
            return;

        if (!_powerCellSystem.TryGetBatteryFromSlot(entity, out var battery) || !controlComp.IsCurrentlySealed)
        {
            _alertsSystem.ClearAlert(controlComp.WearerEntity.Value, comp.SuitPowerAlert);
            return;
        }

        var severity = ContentHelpers.RoundToLevels(MathF.Max(0f, battery.CurrentCharge), battery.MaxCharge, 6);
        _alertsSystem.ShowAlert(controlComp.WearerEntity.Value, comp.SuitPowerAlert, (short) severity);
    }

    /// <summary>
    /// Tries to find battery for charger
    /// </summary>
    private void OnFindInventoryBatteryEvent(Entity<SealableClothingRequiresPowerComponent> entity, ref InventoryRelayedEvent<FindInventoryBatteryEvent> args)
    {
        if (args.Args.FoundBattery != null)
            return;

        if (_powerCellSystem.TryGetBatteryFromSlot(entity, out var batteryEnt, out var battery))
            args.Args.FoundBattery = (batteryEnt.Value, battery);
    }
}
