namespace Content.Shared.Stunnable.Events;

[ByRefEvent]
public record struct KnockdownOnHitAttemptEvent(bool Cancelled);
