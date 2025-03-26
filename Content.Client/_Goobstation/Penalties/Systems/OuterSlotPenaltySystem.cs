using Content.Shared._Goobstation.Penalties.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;

namespace Content.Client._Goobstation.Penalties.Systems;

public sealed partial class OuterSlotPenaltySystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly ClothingSpeedModifierSystem _clothingSpeedModifierSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OuterSlotPenaltyComponent, ClothingDidEquippedEvent>(OnEquip);
        SubscribeLocalEvent<OuterSlotPenaltyComponent, ClothingDidUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid uid, OuterSlotPenaltyComponent comp, ref ClothingDidEquippedEvent args)
    {
        if (args.Clothing.Comp.Slots == SlotFlags.OUTERCLOTHING)
            comp.OuterLayerEquipped = true;
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }

    private void OnUnequip(EntityUid uid, OuterSlotPenaltyComponent comp, ref ClothingDidUnequippedEvent args)
    {
        if (args.Clothing.Comp.Slots == SlotFlags.OUTERCLOTHING)
            comp.OuterLayerEquipped = false;
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }
}
