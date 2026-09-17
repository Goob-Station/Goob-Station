using Content.Shared.Mobs;

namespace Content.Shared._Goobstation.Sleep;

/// <summary>
/// Raised whenever entity almost went to sleep
/// </summary>
[ByRefEvent]
public record struct SleepOverrideEvent(MobState MobState = MobState.Alive);
