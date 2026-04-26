using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.StockMarket;

/// <summary>
/// Added to the station entity to track the stock market state.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class StationStockMarketComponent : Component
{
    /// <summary>
    /// Current state of each stock on the market.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<StockMarketPrototype>, StockEntry> Stocks = new();

    /// <summary>
    /// How often prices update.
    /// </summary>
    [DataField]
    public TimeSpan TickDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Next time prices will update.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextTick;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class StockEntry
{
    /// <summary>
    /// Current price per share in spesos.
    /// </summary>
    [DataField]
    public float Price;

    /// <summary>
    /// Price history for charting (most recent last). Capped at ~20 entries.
    /// </summary>
    [DataField]
    public List<float> PriceHistory = new();

    /// <summary>
    /// How many shares each cargo account owns.
    /// Key = cargo account prototype ID, Value = number of shares.
    /// </summary>
    [DataField]
    public Dictionary<string, int> OwnedShares = new();
}