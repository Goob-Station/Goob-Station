using Content.Shared.Inventory;

namespace Content.Shared._White.Standing;

public sealed class GetStandingUpTimeMultiplierEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.FEET;

    public float Multiplier = 1f;
}
