namespace Content.Goobstation.Common.Weapons;

[ByRefEvent]
public record struct GetLightAttackRangeEvent(EntityUid? Target, EntityUid User, float Range);
