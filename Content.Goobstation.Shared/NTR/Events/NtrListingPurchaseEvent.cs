using Content.Shared.FixedPoint;
using Content.Shared.Store;

namespace Content.Goobstation.Shared.NTR.Events;

public sealed class NtrListingPurchaseEvent
{
    public FixedPoint2 Cost;

    public NtrListingPurchaseEvent(FixedPoint2 cost)
    {
        Cost = cost;
    }
}
