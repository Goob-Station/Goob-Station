using Robust.Shared.Prototypes;

namespace Content.Shared._durkcode.ServerCurrency;

[Prototype("tokenListing")]
public sealed class TokenListingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("name", required: true)]
    public string Name { get; private set; } = string.Empty;

    [DataField("label", required: true)]
    public string Label { get; private set; } = string.Empty;

    [DataField("description")]
    public string Description { get; private set; } = string.Empty;

    [DataField("price", required: true)]
    public int Price { get; private set; }

    [DataField("adminNote", required: true)]
    public string AdminNote { get; private set; } = string.Empty;
}