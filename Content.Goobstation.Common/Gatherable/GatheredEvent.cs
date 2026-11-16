namespace Content.Goobstation.Common.Gatherable;

[ByRefEvent]
public readonly record struct GatheredEvent(EntityUid? Gatherer);
