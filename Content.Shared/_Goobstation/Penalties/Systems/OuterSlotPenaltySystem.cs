using Content.Shared._Goobstation.Penalties.Components;
using Content.Shared.Clothing;
using Content.Shared.Movement.Systems;
using Content.Shared.Clothing.Components;
using Robust.Shared.Containers;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Inventory;

namespace Content.Shared._Goobstation.Penalties.Systems;

public sealed partial class OuterSlotPenaltySystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly ClothingSpeedModifierSystem _clothingSpeedModifierSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OuterSlotPenaltyComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<OuterSlotPenaltyComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<OuterSlotPenaltyComponent, RefreshMovementSpeedModifiersEvent>(OnMove);
    }

    private void OnEntInserted(EntityUid uid, OuterSlotPenaltyComponent comp, ref EntInsertedIntoContainerMessage args)
    {
        if (TryComp<ClothingComponent>(args.Entity, out var cloth) && cloth.Slots == SlotFlags.OUTERCLOTHING)
        {
            cloth.EquipDelay = TimeSpan.FromSeconds(comp.EquipDelay);
            cloth.UnequipDelay = TimeSpan.FromSeconds(comp.UnequipDelay);
        }
    }

    private void OnEntRemoved(EntityUid uid, OuterSlotPenaltyComponent comp, ref EntRemovedFromContainerMessage args)
    {
        if (TryComp<ClothingComponent>(args.Entity, out var cloth) && cloth.Slots == SlotFlags.OUTERCLOTHING)
        {
            cloth.EquipDelay = default!;
            cloth.UnequipDelay = default!;
        }
    }

    private void OnMove(EntityUid uid, OuterSlotPenaltyComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (comp.OuterLayerEquipped)
            args.ModifySpeed(comp.EquippedSpeedMultiplier);
    }
}
