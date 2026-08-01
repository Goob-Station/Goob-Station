// SPDX-License-Identifier: AGPL-3.0-or-later


namespace Content.Shared._EinsteinEngines.TelescopicBaton;

[ByRefEvent]
public record struct KnockdownOnHitAttemptEvent(bool Cancelled, bool DropItems); // Goob edit

public sealed class KnockdownOnHitSuccessEvent(List<EntityUid> knockedDown) : EntityEventArgs // Goobstation
{
    public List<EntityUid> KnockedDown = knockedDown;
}
