using Robust.Shared.Map;

namespace Content.Goobstation.Common.Solutions;

[ByRefEvent]
public readonly record struct SpillableCreatePuddleOnHitEvent(
    EntityUid User,
    EntityCoordinates Coords,
    float Amount);
