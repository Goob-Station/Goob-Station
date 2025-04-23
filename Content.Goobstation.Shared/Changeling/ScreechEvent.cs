using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Changeling;

public sealed class SoundSuppressionPresenceEvent : EntityEventArgs, IInventoryRelayEvent
{
    public bool SoundProtected = false;
    public SlotFlags TargetSlots => SlotFlags.EARS | SlotFlags.HEAD;
}
