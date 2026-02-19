namespace Content.Goobstation.Common.Grab;

// Can't have inventory relays because it must be in common...
[ByRefEvent]
public record struct GrabAttemptEvent(
    EntityUid Puller,
    bool IgnoreCombatMode = false,
    GrabStage? GrabStageOverride = null,
    float EscapeAttemptModifier = 1f)
{
    public bool Grabbed = false;
}

[ByRefEvent]
public record struct GrabAttemptReleaseEvent(
    EntityUid? user,
    EntityUid puller)
{
    public bool Released = false;
}

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
