using Content.Goobstation.Common.MartialArts;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Grab;

[ByRefEvent]
public record struct GrabModifierEvent(EntityUid User, GrabStage Stage) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.GLOVES;

    public GrabStage? NewStage = null;

    public float Multiplier = 1f;

    public float Modifier = 0f;

    public float SpeedMultiplier = 1f;
}
