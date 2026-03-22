using Content.Trauma.Shared.Ranching.Components;

namespace Content.Trauma.Shared.Ranching.Events;

/// <summary>
/// Raised on the mob when attempting to lay an egg
/// </summary>
/// <param name="Mob">The mob that is laying the egg</param>
[ByRefEvent]
public record struct RanchingEggLayAttemptEvent(Entity<RanchingEggLayerComponent> Mob);
