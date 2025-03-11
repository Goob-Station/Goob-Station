using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Shared._Goobstation.Fishing.Components;

[RegisterComponent]
public sealed partial class FishingSpotComponent : Component
{
    /// <summary>
    /// All possible fishes to catch here
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector FishList;

    /// <summary>
    /// Default time for fish to occur
    /// </summary>
    [DataField]
    public float FishDefaultTimer;

    /// <summary>
    /// Variety number that FishDefaultTimer can go up or down to randomly
    /// </summary>
    [DataField]
    public float FishTimerVariety;

    /// <summary>
    /// How hard it is to caught something from here
    /// </summary>
    [DataField]
    public float FishDifficulty = -0.1f;

    [DataField]
    public float FishDifficultyVariety = -0.05f;
}
