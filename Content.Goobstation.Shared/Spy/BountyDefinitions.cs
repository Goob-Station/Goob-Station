using Content.Shared.Objectives;
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
    public NetEntity? TargetEntity;
    public ProtoId<StealTargetGroupPrototype> TargetGroup;
    public NetEntity? Owner;
    public TimeSpan TimeAssigned;
    public TimeSpan? TimeCompleted;

    public SpyBountyData(NetEntity targetEntity, ProtoId<StealTargetGroupPrototype> targetGroup)
    {
        TargetEntity = targetEntity;
        TargetGroup = targetGroup;
    }
}

// Black market listing
public struct BlackMarketListing
{
    public NetEntity Item;
    public int Price;
    public TimeSpan TimeListed;
    public bool Purchased;
}
