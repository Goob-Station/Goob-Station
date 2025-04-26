using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared.Nyanotrasen.Kitchen.UI
{
    [Serializable, NetSerializable]
    public sealed class DeepFryerBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly FixedPoint2 OilLevel;
        public readonly FixedPoint2 OilPurity;
        public readonly FixedPoint2 FryingOilThreshold;
        public readonly NetEntity[] ContainedEntities;

        public DeepFryerBoundUserInterfaceState(
            FixedPoint2 oilLevel,
            FixedPoint2 oilPurity,
            FixedPoint2 fryingOilThreshold,
            NetEntity[] containedEntities)
        {
            OilLevel = oilLevel;
            OilPurity = oilPurity;
            FryingOilThreshold = fryingOilThreshold;
            ContainedEntities = containedEntities;
        }
    }

    [Serializable, NetSerializable]
    public sealed class DeepFryerRemoveItemMessage : BoundUserInterfaceMessage
    {
        public readonly NetEntity Item;

        public DeepFryerRemoveItemMessage(NetEntity item)
        {
            Item = item;
        }
    }

    [Serializable, NetSerializable]
    public sealed class DeepFryerInsertItemMessage : BoundUserInterfaceMessage
    {
        public DeepFryerInsertItemMessage() { }
    }

    [Serializable, NetSerializable]
    public sealed class DeepFryerScoopVatMessage : BoundUserInterfaceMessage
    {
        public DeepFryerScoopVatMessage() { }
    }

    [Serializable, NetSerializable]
    public sealed class DeepFryerClearSlagMessage : BoundUserInterfaceMessage
    {
        public DeepFryerClearSlagMessage() { }
    }

    [Serializable, NetSerializable]
    public sealed class DeepFryerRemoveAllItemsMessage : BoundUserInterfaceMessage
    {
        public DeepFryerRemoveAllItemsMessage() { }
    }

    [NetSerializable, Serializable]
    public enum DeepFryerUiKey
    {
        Key
    }
}
