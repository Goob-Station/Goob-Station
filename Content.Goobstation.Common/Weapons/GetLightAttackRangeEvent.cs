namespace Content.Goobstation.Common.Weapons;

[ByRefEvent]
public record struct GetLightAttackRangeEvent(EntityUid? Target, EntityUid User, float Range, bool Cancel = false);


[ByRefEvent]
public record struct LightAttackSpecialInteractionEvent(EntityUid? Target, EntityUid User, float Range, bool Cancel = false);
