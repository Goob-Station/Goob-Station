namespace Content.Goobstation.Common.DoAfter;

[ByRefEvent]
public readonly record struct CombatModeToggledEvent(EntityUid User, bool Activated);
