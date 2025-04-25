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
    public readonly List<SpyBountyData> Listings;
    public readonly TimeSpan Time;

    public SpyUplinkUpdateState(List<SpyBountyData> listings, TimeSpan time)
    {
        Listings = listings;
        Time = time;
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
