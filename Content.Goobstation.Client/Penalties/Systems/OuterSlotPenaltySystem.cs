using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;

namespace Content.Goobstation.Client.Penalties.Systems;

public sealed partial class OuterSlotPenaltySystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Shared.Penalties.Components.OuterSlotPenaltyComponent, ClothingDidEquippedEvent>(OnEquip);
        SubscribeLocalEvent<Shared.Penalties.Components.OuterSlotPenaltyComponent, ClothingDidUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid uid, Shared.Penalties.Components.OuterSlotPenaltyComponent comp, ref ClothingDidEquippedEvent args)
    {
        if (args.Clothing.Comp.Slots == SlotFlags.OUTERCLOTHING)
            comp.OuterLayerEquipped = true;
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }

    private void OnUnequip(EntityUid uid, Shared.Penalties.Components.OuterSlotPenaltyComponent comp, ref ClothingDidUnequippedEvent args)
    {
        if (args.Clothing.Comp.Slots == SlotFlags.OUTERCLOTHING)
            comp.OuterLayerEquipped = false;
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }
}
