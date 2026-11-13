using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.Store;

public partial class ListingData
{
    [DataField]
    public bool ResetRestockOnPurchase;

    [DataField]
    public TimeSpan? RestockAfterPurchase;

    /// <summary>
    /// When purchased, it will block refunds of these listings.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<ListingPrototype>> BlockRefundListings = new();

    [DataField]
    public bool RaiseProductEventOnMind;
}

public sealed partial class ListingDataWithCostModifiers
{
    /// <summary>
    /// Tracks listing cost on each purchase
    /// </summary>
    public List<Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2>> PurchaseCostHistory = new();
}
