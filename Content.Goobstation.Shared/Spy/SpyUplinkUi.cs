using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Spy;

[Serializable, NetSerializable]
public enum SpyUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class SpyUplinkUpdateState(List<SpyBountyData> listings, TimeSpan time) : BoundUserInterfaceState
{
    public readonly List<SpyBountyData> Listings = listings;
    public readonly TimeSpan Time = time;
}

[Serializable, NetSerializable]
public sealed class SpyRequestUpdateInterfaceMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class SpyClaimBountyMessage(SpyBountyData bounty) : BoundUserInterfaceMessage
{
    public SpyBountyData Bounty = bounty;
}
