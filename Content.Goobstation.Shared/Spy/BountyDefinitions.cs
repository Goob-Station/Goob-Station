using Content.Shared.Objectives;
using Robust.Shared.Prototypes;

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
public struct SpyBounty
{
    public EntityUid? TargetEntity;
    public ProtoId<StealTargetGroupPrototype> TargetGroup;
    public EntityUid? Owner;
    public TimeSpan TimeAssigned;
    public TimeSpan? TimeCompleted;

    public SpyBounty(EntityUid targetEntity, ProtoId<StealTargetGroupPrototype> targetGroup)
    {
        TargetEntity = targetEntity;
        TargetGroup = targetGroup;
    }
}

// Black market listing
public struct BlackMarketListing
{
    public EntityUid Item;
    public int Price;
    public TimeSpan TimeListed;
    public bool Purchased;
}
