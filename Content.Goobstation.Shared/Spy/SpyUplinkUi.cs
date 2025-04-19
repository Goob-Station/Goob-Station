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
