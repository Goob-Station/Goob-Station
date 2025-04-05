namespace Content.Goobstation.Common.Wizard.Store;

public sealed class ItemPurchasedEvent(EntityUid buyer) : EntityEventArgs
{
    public EntityUid Buyer = buyer;
}
