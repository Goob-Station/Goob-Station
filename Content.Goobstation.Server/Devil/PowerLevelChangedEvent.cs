namespace Content.Goobstation.Server.Devil;

/// <summary>
/// Raised on a devil when their power level changes.
/// </summary>
/// <param name="User">The Devil whos power level is changing</param>
/// <param name="NewLevel">The new level they are reaching.</param>
[ByRefEvent]
public record struct PowerLevelChangedEvent(EntityUid User, int NewLevel);
