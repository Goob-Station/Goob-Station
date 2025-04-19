using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Spy;

[Serializable, NetSerializable]
public enum SpyUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class SpyUplinkUpdateState : BoundUserInterfaceState
{
    public readonly HashSet<SpyBountyData> Listings;

    public SpyUplinkUpdateState(HashSet<SpyBountyData> listings)
    {
        Listings = listings;
    }
}

[Serializable, NetSerializable]
public sealed class SpyRequestUpdateInterfaceMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class SpyClaimBountyMessage : BoundUserInterfaceMessage
{
    public SpyBountyData Bounty;

    public SpyClaimBountyMessage(SpyBountyData bounty)
    {
        Bounty = bounty;
    }
}
