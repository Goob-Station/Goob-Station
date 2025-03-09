using Content.Shared.Inventory;

namespace Content.Shared._Goobstation.Flashbang;

public sealed class GetFlashbangedEvent(float range) : EntityEventArgs, IInventoryRelayEvent
{
    public float ProtectionRange = range;

    public SlotFlags TargetSlots => SlotFlags.EARS | SlotFlags.HEAD;
}
public sealed class AreaFlashEvent(float range, float distance, EntityUid target) : EntityEventArgs
{
    public float Range = range;

    public float Distance = distance;

    public EntityUid Target = target;
}

public sealed class FlashDurationMultiplierEvent : EntityEventArgs, IInventoryRelayEvent
{
    public float Multiplier = 1f;

    public SlotFlags TargetSlots => SlotFlags.EYES | SlotFlags.HEAD | SlotFlags.MASK;
}
