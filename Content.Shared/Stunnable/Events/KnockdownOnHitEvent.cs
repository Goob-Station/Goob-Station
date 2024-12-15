using Content.Shared._White.Standing;

namespace Content.Shared.Stunnable.Events;

[ByRefEvent]
public record struct KnockdownOnHitAttemptEvent(bool Cancelled, DropHeldItemsBehavior Behavior); // Goob edit
