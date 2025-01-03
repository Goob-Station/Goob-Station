using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.VendingMachines;

/// <summary>
/// Similar to <c>VendingMachineInventoryPrototype</c> but for <see cref="ShopVendorComponent"/>.
/// </summary>
[Prototype]
public sealed class ShopInventoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The item listings for sale.
    /// </summary>
    [DataField(required: true)]
    public List<ShopListing> Listings = new();
}

[DataRecord, Serializable]
public record struct ShopListing(EntProtoId Id, uint Cost);
