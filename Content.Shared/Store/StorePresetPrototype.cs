// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared.Store;

/// <summary>
///     Specifies generic info for initializing a store.
/// </summary>
[Prototype]
[DataDefinition]
public sealed partial class StorePresetPrototype : IPrototype
{
    [ViewVariables] [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// The name displayed at the top of the store window
    /// </summary>
    [DataField("storeName", required: true)]
    public string StoreName { get; private set; } = string.Empty;

    /// <summary>
    /// The categories that this store can access
    /// </summary>
    [DataField("categories")]
    public HashSet<ProtoId<StoreCategoryPrototype>> Categories { get; private set; } = new();

    /// <summary>
    /// The inital balance that the store initializes with.
    /// </summary>
    [DataField("initialBalance")]
    public Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2>? InitialBalance { get; private set; }

    /// <summary>
    /// The currencies that are accepted in the store
    /// </summary>
    [DataField("currencyWhitelist")]
    public HashSet<ProtoId<CurrencyPrototype>> CurrencyWhitelist { get; private set; } = new();
}
