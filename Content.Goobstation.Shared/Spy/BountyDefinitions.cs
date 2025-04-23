using Content.Shared.Objectives;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Spy;

[Prototype]
public sealed class SpyBountyRewardPrototype : IPrototype // beyond any syndie uplink item 🧌
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public EntProtoId Reward;
}

// Active bounty instance
[Serializable, NetSerializable]
[Virtual, DataDefinition]
public sealed partial class SpyBountyData
{
    public NetEntity TargetEntity;
    public ProtoId<StealTargetGroupPrototype> TargetGroup;
    public bool Claimed;
    public TimeSpan TimeAssigned;
    public TimeSpan? TimeCompleted;
    public ListingData RewardListing;
    public SpyBountyDifficulty Difficulty;

    public SpyBountyData(NetEntity targetEntity, ProtoId<StealTargetGroupPrototype> targetGroup, ListingData rewardListing, SpyBountyDifficulty difficulty)
    {
        TargetEntity = targetEntity;
        TargetGroup = targetGroup;
        RewardListing = rewardListing;
        Difficulty = difficulty;
    }
}

[Serializable, NetSerializable]
public enum SpyBountyDifficulty
{
    Easy,
    Medium,
    Hard,
}
