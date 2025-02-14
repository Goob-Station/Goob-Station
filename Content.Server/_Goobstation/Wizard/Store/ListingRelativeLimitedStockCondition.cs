using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Store;

/// <summary>
/// Only allows a listing to be purchased a certain amount of times, relative to some other listing's purchase amount.
/// If other listing purchase amount is greater than this listing purchase amount, it can be purchased.
/// </summary>
public sealed partial class ListingRelativeLimitedStockCondition : ListingCondition
{
    [DataField(required: true)]
    public ProtoId<ListingPrototype> RelativeListing;

    public override bool Condition(ListingConditionArgs args)
    {
        if (!args.EntityManager.TryGetComponent<StoreComponent>(args.StoreEntity, out var storeComp))
            return false;

        var allListings = storeComp.Listings;

        foreach (var listing in allListings)
        {
            if (listing.ID == RelativeListing.Id)
                return listing.PurchaseAmount > args.Listing.PurchaseAmount;
        }

        return false;
    }
}
