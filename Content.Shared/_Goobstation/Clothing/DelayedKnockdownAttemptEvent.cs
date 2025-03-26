using Content.Shared.Inventory;

namespace Content.Shared._Goobstation.Clothing;

public sealed class DelayedKnockdownAttemptEvent : CancellableEntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.OUTERCLOTHING;

    public float DelayDelta = 0f;

    public float KnockdownTimeDelta = 0f;
}
