namespace Content.Goobstation.Shared.Cargo;

public sealed class CargoOrderApprovedEvent(EntityUid orderEntity, string productId, NetEntity requester) : EntityEventArgs
{
    public readonly EntityUid OrderEntity = orderEntity;
    public readonly string ProductId = productId;
    public readonly NetEntity Requester = requester;
}
