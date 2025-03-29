using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Fishing.Events;

public sealed partial class ThrowFishingLureActionEvent : WorldTargetActionEvent;

public sealed partial class PullFishingLureActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed class ActiveFishingSpotComponentState : ComponentState
{
    public readonly float FishDifficulty;
    public bool IsActive;
    public TimeSpan? FishingStartTime;
    public NetEntity? AttachedFishingLure;

    public ActiveFishingSpotComponentState(float fishDifficulty, bool isActive, TimeSpan? fishingStartTime, NetEntity? attachedFishingLure)
    {
        FishDifficulty = fishDifficulty;
        IsActive = isActive;
        FishingStartTime = fishingStartTime;
        AttachedFishingLure = attachedFishingLure;
    }
}
