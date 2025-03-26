using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Clothing;

public sealed class GetStandingUpTimeMultiplierEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.FEET;

    public float Multiplier = 1f;
}
