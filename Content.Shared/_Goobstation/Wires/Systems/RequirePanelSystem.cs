using Content.Shared._Goobstation.Wires.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Wires;

namespace Content.Shared._Goobstation.Wires.Systems;

public sealed partial class RequirePanelSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemSlotsRequirePanelComponent, ItemSlotInsertAttemptEvent>(ItemSlotInsertAttempt);
        SubscribeLocalEvent<ItemSlotsRequirePanelComponent, ItemSlotEjectAttemptEvent>(ItemSlotEjectAttempt);
    }

    private void ItemSlotInsertAttempt(Entity<ItemSlotsRequirePanelComponent> entity, ref ItemSlotInsertAttemptEvent args)
    {
        args.Cancelled = !CheckPanelStateForItemSlot(entity, args.Slot.ID);
    }
    private void ItemSlotEjectAttempt(Entity<ItemSlotsRequirePanelComponent> entity, ref ItemSlotEjectAttemptEvent args)
    {
        args.Cancelled = !CheckPanelStateForItemSlot(entity, args.Slot.ID);
    }

    public bool CheckPanelStateForItemSlot(Entity<ItemSlotsRequirePanelComponent> entity, string? slot)
    {
        var (uid, comp) = entity;

        if (slot == null)
            return false;

        // If slot not require wire panel - don't cancel interaction
        if (!comp.Slots.TryGetValue(slot, out var isRequireOpen))
            return false;

        if (!TryComp<WiresPanelComponent>(uid, out var wiresPanel))
            return false;

        return wiresPanel.Open == isRequireOpen;
    }
}
