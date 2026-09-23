// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Store;

public sealed class ItemPurchasedEvent(EntityUid buyer) : EntityEventArgs
{
    public EntityUid Buyer = buyer;
}