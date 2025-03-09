using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Shared._Goobstation.Fishing.Components;

/// <summary>
/// Dynamic component, that is assigned to active fishing spots that are currently waiting for da fish.
/// </summary>
[RegisterComponent]
public sealed partial class ActiveFishingSpotComponent : Component
{
    [ViewVariables]
    public EntityUid AttachedFishingLure;

    [DataField]
    public float Accumulator;

    /// <summary>
    /// If true, someone is pulling fish out of this spot.
    /// </summary>
    [DataField]
    public bool IsActive;

    [DataField]
    public float FishDifficulty;

    /// <summary>
    /// All possible fishes to catch here
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector FishList;
}
