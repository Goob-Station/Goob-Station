// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
