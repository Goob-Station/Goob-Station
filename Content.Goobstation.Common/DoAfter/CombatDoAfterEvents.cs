namespace Content.Goobstation.Common.DoAfter;

[ByRefEvent]
public readonly record struct CombatModeToggledEvent(EntityUid User, bool Activated);

[ByRefEvent]
public readonly record struct EnsnaredEvent(EntityUid Target);
