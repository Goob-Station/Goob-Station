using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Mobs;

namespace Content.Goobstation.Shared.Clothing;

public record struct ClothingAutoInjectRelayedEvent(EntityUid Target, MobState NewState) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}

