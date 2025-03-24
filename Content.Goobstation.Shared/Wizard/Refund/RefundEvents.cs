using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.Refund;

[Serializable, NetSerializable]
public sealed class StoreRefundState(List<RefundListingData> listings, bool refundDisabled) : BoundUserInterfaceState
{
    public List<RefundListingData> Listings = listings;

    public bool RefundDisabled = refundDisabled;
}

[Serializable, NetSerializable]
public struct RefundListingData(NetEntity entity, string displayName)
{
    public NetEntity Entity = entity;

    public string DisplayName = displayName;
}

[Serializable, NetSerializable]
public sealed class StoreRefundListingMessage(NetEntity listingEntity) : BoundUserInterfaceMessage
{
    public NetEntity ListingEntity = listingEntity;
}

[Serializable, NetSerializable]
public sealed class StoreRefundAllListingsMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public enum RefundUiKey : byte
{
    Key
}
