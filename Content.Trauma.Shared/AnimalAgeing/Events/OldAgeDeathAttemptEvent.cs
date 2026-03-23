namespace Content.Trauma.Shared.AnimalAgeing.Events;

/// <summary>
/// Raise on the mob when attempting to kill it via old age
/// </summary>
/// <param name="Mob">The mob raised on</param>
[ByRefEvent]
public record struct OldAgeDeathAttemptEvent(Entity<AnimalAgeingComponent> Mob, bool Cancelled = false);
