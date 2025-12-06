namespace Content.Goobstation.Common.Shuttles;

[ByRefEvent]
public record struct DroneGetLinkedShuttleEvent(EntityUid? Found = null);
