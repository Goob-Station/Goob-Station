using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Chemistry;

public sealed class VaporCheckEyeProtectionEvent : EntityEventArgs, IInventoryRelayEvent
{
    public bool Protected;
    public SlotFlags TargetSlots => SlotFlags.EYES | SlotFlags.MASK | SlotFlags.HEAD;
}
