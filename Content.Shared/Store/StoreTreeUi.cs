using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Store;

[Serializable, NetSerializable]
public sealed class StoreTreeUpdateState(
    ProtoId<StoreTreePrototype> tree,
    HashSet<ListingDataWithCostModifiers> listings,
    HashSet<ProtoId<ListingPrototype>> purchased,
    Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> balance) : BoundUserInterfaceState
{
    public readonly ProtoId<StoreTreePrototype> Tree = tree;
    public readonly HashSet<ListingDataWithCostModifiers> Listings = listings;
    public readonly HashSet<ProtoId<ListingPrototype>> Purchased = purchased;
    public readonly Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> Balance = balance;
}
