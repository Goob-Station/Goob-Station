namespace Content.Shared._Lavaland.MobPhases;

/// <summary>
/// Raised on the mob whenever its phase changes.
/// </summary>
[ByRefEvent]
public readonly record struct MobPhaseChangedEvent(int OldPhase, int NewPhase);
