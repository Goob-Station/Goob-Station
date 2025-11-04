using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Ranching.Food;

/// <summary>
/// This is used to mark a food as part of the Ranching Department.
/// Currently used for chicken food, but may be used on other animals in the feature (cows, sheep etc)
/// TODO: Make it so you can force-feed chickens with the feed sack
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RanchingFoodComponent : Component
{
    /// <summary>
    /// Which ranching animals can gain effects from this food
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = new();
};

/// <summary>
/// Raised on the entity that ate the ranching food
/// </summary>
/// <param name="Food"></param> The food provided
[ByRefEvent]
public record struct RanchingFoodEatenEvent(EntityUid Food);
