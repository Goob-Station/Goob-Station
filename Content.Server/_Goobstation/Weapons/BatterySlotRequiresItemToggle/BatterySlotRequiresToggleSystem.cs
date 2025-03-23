using Content.Shared.Containers.ItemSlots;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Server._Goobstation.Weapons.BatterySlotRequiresItemToggle;

public sealed class BatterySlotRequiresToggleSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatterySlotRequiresToggleComponent, ItemToggledEvent>(OnToggle);
    }

    private void OnToggle(Entity<BatterySlotRequiresToggleComponent> ent, ref ItemToggledEvent args)
    {
        if (!TryComp<ItemSlotsComponent>(ent, out var itemslots)
            || !_itemSlotsSystem.TryGetSlot(ent, ent.Comp.ItemSlot, out var slot, itemslots))
            return;

        _itemSlotsSystem.SetLock(ent, slot, !args.Activated, itemslots);
    }
}
