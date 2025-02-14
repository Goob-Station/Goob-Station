namespace Content.Server._Goobstation.Wizard.Store;

public sealed class ItemPurchasedEvent(EntityUid buyer) : EntityEventArgs
{
    public EntityUid Buyer = buyer;
}
