// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.ServerCurrency;

[Prototype]
public sealed partial class TokenListingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField( required: true)]
    public string Name { get; private set; } = string.Empty;

    [DataField( required: true)]
    public string Label { get; private set; } = string.Empty;

    [DataField]
    public string Description { get; private set; } = string.Empty;

    [DataField(required: true)]
    public int Price { get; private set; }

    [DataField(required: true)]
    public string AdminNote { get; private set; } = string.Empty;
}
