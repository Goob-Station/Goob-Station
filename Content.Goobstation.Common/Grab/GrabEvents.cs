namespace Content.Goobstation.Common.Grab;

// Can't have inventory relays because it must be in common...
[ByRefEvent]
public record struct RaiseGrabModifierEventEvent(
    EntityUid User,
    int Stage,
    int? NewStage = null,
    float Multiplier = 1f,
    float Modifier = 0f,
    float SpeedMultiplier = 1f);

[ByRefEvent]
public record struct FindGrabbingItemEvent(EntityUid? Grabbed = null, EntityUid? GrabbingItem = null);

[ByRefEvent]
public readonly record struct StopGrabbingItemPullEvent(EntityUid PulledUid);
