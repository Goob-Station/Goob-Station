using Content.Goobstation.Shared.Store;
using Content.Shared.Mind;
using Content.Shared.Store;

namespace Content.Goobstation.Server.Store.Conditions;

/// <summary>
/// Only allows a listing to be purchased a certain amount of times per individual buyer,
/// as opposed to <see cref="Content.Server.Store.Conditions.ListingLimitedStockCondition"/>
/// which limits the total across the whole store.
/// Useful for shared stores (e.g. gang lockers) where each member should still get their own copy.
/// </summary>
public sealed partial class ListingLimitedStockPerBuyerCondition : ListingCondition
{
    /// <summary>
    /// How many times each buyer may purchase this listing.
    /// </summary>
    [DataField("stock")]
    public int Stock = 1;

    public override bool Condition(ListingConditionArgs args)
    {
        var ent = args.EntityManager;

        // No mind means we can't attribute the purchase to anyone, so don't gate it.
        if (!ent.System<SharedMindSystem>().TryGetMind(args.Buyer, out var mindId, out _)
            || !ent.TryGetComponent<StorePurchaseRecordComponent>(mindId, out var record))
            return true;

        return record.Purchases.GetValueOrDefault(args.Listing.ID) < Stock;
    }
}
