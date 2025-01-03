using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.VendingMachines;

[Serializable, NetSerializable]
public sealed class ShopVendorPurchaseMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}
