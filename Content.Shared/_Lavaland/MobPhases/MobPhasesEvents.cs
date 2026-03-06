namespace Content.Shared._Lavaland.MobPhases;

public readonly record struct MobPhaseChangedEvent
(
    int OldPhase,
    int NewPhase
);
