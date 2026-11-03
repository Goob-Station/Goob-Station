namespace Content.Goobstation.Shared.Supermatter.Events;

[ByRefEvent]
public readonly record struct SupermatterConsumedEvent(EntityUid Supermatter);
