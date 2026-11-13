// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;

namespace Content.Goobstation.Shared.NTR.Events;

public sealed class NtrListingPurchaseEvent(FixedPoint2 cost)
{
    public FixedPoint2 Cost = cost;
}
