using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Fishing.Components;

/// <summary>
/// Dynamic component, that is assigned to active fishing spots that are currently waiting for da fish.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveFishingSpotComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid AttachedFishingLure;

    [DataField, AutoNetworkedField]
    public TimeSpan FishingStartTime;

    /// <summary>
    /// If true, someone is pulling fish out of this spot.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;

    [DataField, AutoNetworkedField]
    public float FishDifficulty;

    /// <summary>
    /// All possible fishes to catch here
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector FishList;
}
