using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server.Store.Components;

public sealed partial class StoreRefundComponent
{
    [ViewVariables, DataField]
    public ListingDataWithCostModifiers? Data;

    [ViewVariables, DataField]
    public Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> BalanceSpent = new();
}
