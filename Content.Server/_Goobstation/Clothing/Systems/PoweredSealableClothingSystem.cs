using Content.Server.Power.Components;
using Content.Server.PowerCell;
using Content.Shared._Goobstation.Clothing.Components;
using Content.Shared._Goobstation.Clothing.Systems;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;

namespace Content.Server._Goobstation.Clothing.Systems;

public sealed partial class PoweredSealableClothingSystem : SharedPoweredSealableClothingSystem
{
    [Dependency] private readonly PowerCellSystem _powerCellSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SealableClothingRequiresPowerComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnMovementSpeedChange);
        SubscribeLocalEvent<SealableClothingRequiresPowerComponent, PowerCellChangedEvent>(OnPowerCellChanged);
        SubscribeLocalEvent<SealableClothingRequiresPowerComponent, PowerCellSlotEmptyEvent>(OnPowerCellEmpty);
        SubscribeLocalEvent<SealableClothingRequiresPowerComponent, ClothingControlSealCompleteEvent>(OnRequiresPowerSealCompleteEvent);

    }

    private void OnPowerCellChanged(Entity<SealableClothingRequiresPowerComponent> entity, ref PowerCellChangedEvent args)
    {
        if (entity.Comp.IsPowered || !_powerCellSystem.HasDrawCharge(entity))
            return;

        entity.Comp.IsPowered = true;
        Dirty(entity);

        ModifySpeed(entity, entity.Comp.MovementSpeedPenalty);

    }

    private void OnPowerCellEmpty(Entity<SealableClothingRequiresPowerComponent> entity, ref PowerCellSlotEmptyEvent args)
    {
        entity.Comp.IsPowered = false;
        Dirty(entity);

        ModifySpeed(entity, entity.Comp.MovementSpeedPenalty);
    }

    /// <summary>
    /// Enables or disables power cell draw on seal/unseal complete
    /// </summary>
    private void OnRequiresPowerSealCompleteEvent(Entity<SealableClothingRequiresPowerComponent> entity, ref ClothingControlSealCompleteEvent args)
    {
        if (!TryComp(entity, out PowerCellDrawComponent? drawComp))
            return;

        _powerCellSystem.SetDrawEnabled((entity.Owner, drawComp), args.IsSealed);
        ModifySpeed(entity, entity.Comp.MovementSpeedPenalty);
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

    private void ModifySpeed(EntityUid uid, float modifier)
    {
        if (!TryComp(uid, out SealableClothingControlComponent? controlComp) || controlComp.WearerEntity == null)
            return;

        _movementSpeed.RefreshMovementSpeedModifiers(controlComp.WearerEntity.Value);
    }
}
