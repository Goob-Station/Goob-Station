namespace Content.Goobstation.Common.Cuffs;

[ByRefEvent]
public record struct ModifyCuffDurationEvent(EntityUid Target, float Duration);
