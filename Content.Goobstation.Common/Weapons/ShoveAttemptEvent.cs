namespace Content.Goobstation.Common.Weapons;

[ByRefEvent]
public record struct ShoveAttemptEvent(EntityUid User, bool Cancelled = false);
