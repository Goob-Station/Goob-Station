using Content.Trauma.Shared.Ranching.Components;

namespace Content.Trauma.Shared.Ranching.Events;

/// <summary>
/// Raised on the mob when attempting to lay an egg
/// </summary>
/// <param name="Mob">The mob that is laying the egg</param>
/// <param name="HadFood">If the mob had enough food to lay an egg, used for EggRecipe's that only require happiness</param>
[ByRefEvent]
public record struct RanchingEggLayAttemptEvent(Entity<RanchingEggLayerComponent> Mob, bool HadFood);
