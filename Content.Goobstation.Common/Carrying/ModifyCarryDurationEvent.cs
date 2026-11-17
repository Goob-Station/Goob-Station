namespace Content.Goobstation.Common.Carrying;

[ByRefEvent]
public record struct ModifyCarryDurationEvent(EntityUid Carried, float Multiplier = 1f);
