using Content.Shared.Species.Components;
using Content.Shared.Clothing;
using Content.Shared.Movement.Systems;
using Content.Shared.Clothing.Components;

namespace Content.Server.Species.Systems;

public sealed partial class YowieSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<YowieComponent, ClothingDidEquippedEvent>(OnEquip);
        SubscribeLocalEvent<YowieComponent, ClothingDidUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<YowieComponent, RefreshMovementSpeedModifiersEvent>(OnMove);
        SubscribeLocalEvent<YowieComponent, ClothingEquipDoAfterEvent>
    }

    private void OnEquip(EntityUid uid, YowieComponent comp, ref ClothingDidEquippedEvent args)
    {
        if (args.Clothing.Comp.Slots == Shared.Inventory.SlotFlags.OUTERCLOTHING)
        {
            comp.OuterLayerEquipped = true;
        }
    }
    private void OnUnequip(EntityUid uid, YowieComponent comp, ref ClothingDidUnequippedEvent args)
    {
        if (args.Clothing.Comp.Slots == Shared.Inventory.SlotFlags.OUTERCLOTHING)
        {
            comp.OuterLayerEquipped = false;
        }
    }

    private void OnMove(EntityUid uid, YowieComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (component.OuterLayerEquipped)
        {
            args.ModifySpeed(component.SoftSuitSpeedMultiplier);
        }
    }
}
