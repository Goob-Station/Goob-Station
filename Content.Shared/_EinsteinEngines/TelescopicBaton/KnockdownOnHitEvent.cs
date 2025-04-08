using Content.Goobstation.Common.Standing;

namespace Content.Shared._EinsteinEngines.TelescopicBaton;

[ByRefEvent]
public record struct KnockdownOnHitAttemptEvent(bool Cancelled, DropHeldItemsBehavior Behavior); // Goob edit

public sealed class KnockdownOnHitSuccessEvent(List<EntityUid> knockedDown) : EntityEventArgs // Goobstation
{
    public List<EntityUid> KnockedDown = knockedDown;
}
