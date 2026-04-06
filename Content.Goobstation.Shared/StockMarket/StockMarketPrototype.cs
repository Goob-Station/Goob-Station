using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.StockMarket;

/// <summary>
/// Defines a stock that can be traded on the cargo stock market.
/// </summary>
[Prototype]
public sealed partial class StockMarketPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Display name localization key.
    /// </summary>
    [DataField(required: true)]
    public LocId Name { get; private set; } = string.Empty;

    /// <summary>
    /// Description localization key.
    /// </summary>
    [DataField(required: true)]
    public LocId Description { get; private set; } = string.Empty;

    /// <summary>
    /// Category for UI grouping (e.g. "Commodity", "Corporation").
    /// </summary>
    [DataField(required: true)]
    public LocId Category { get; private set; } = string.Empty;

    /// <summary>
    /// Starting price in spesos.
    /// </summary>
    [DataField(required: true)]
    public float BasePrice { get; private set; }

    /// <summary>
    /// How much the price can swing per tick (0.0 to 1.0).
    /// A volatility of 0.05 means up to +/- 5% per tick.
    /// </summary>
    [DataField]
    public float Volatility { get; private set; } = 0.03f;

    /// <summary>
    /// Long-term price drift per tick. 0 = no drift.
    /// </summary>
    [DataField]
    public float Drift { get; private set; } = 0f;

    /// <summary>
    /// Minimum price floor.
    /// </summary>
    [DataField]
    public float MinPrice { get; private set; } = 5f;

    /// <summary>
    /// Maximum price ceiling.
    /// </summary>
    [DataField]
    public float MaxPrice { get; private set; } = 10000f;
}
