using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.StockMarket;

public abstract class SharedStockMarketSystem : EntitySystem
{
}

/// <summary>
/// UI key for the stock market console.
/// </summary>
[Serializable, NetSerializable]
public enum StockMarketConsoleUiKey : byte
{
    Key
}

/// <summary>
/// State sent from server to client to populate the stock market console UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class StockMarketConsoleState : BoundUserInterfaceState
{
    public Dictionary<string, StockDisplayData> Stocks;
    public string ActiveAccount;
    public int AccountBalance;

    public StockMarketConsoleState(
        Dictionary<string, StockDisplayData> stocks,
        string activeAccount,
        int accountBalance)
    {
        Stocks = stocks;
        ActiveAccount = activeAccount;
        AccountBalance = accountBalance;
    }
}

/// <summary>
/// Display data for a single stock, sent to the client.
/// </summary>
[Serializable, NetSerializable]
public sealed class StockDisplayData
{
    public string Name;
    public string Description;
    public string Category;
    public float Price;
    public float PreviousPrice;
    public List<float> PriceHistory;
    public int OwnedShares;

    public StockDisplayData(
        string name,
        string description,
        string category,
        float price,
        float previousPrice,
        List<float> priceHistory,
        int ownedShares)
    {
        Name = name;
        Description = description;
        Category = category;
        Price = price;
        PreviousPrice = previousPrice;
        PriceHistory = priceHistory;
        OwnedShares = ownedShares;
    }
}

/// <summary>
/// Client requests to buy shares.
/// </summary>
[Serializable, NetSerializable]
public sealed class StockMarketBuyMessage : BoundUserInterfaceMessage
{
    public string StockId;
    public int Amount;

    public StockMarketBuyMessage(string stockId, int amount)
    {
        StockId = stockId;
        Amount = amount;
    }
}

/// <summary>
/// Client requests to sell shares.
/// </summary>
[Serializable, NetSerializable]
public sealed class StockMarketSellMessage : BoundUserInterfaceMessage
{
    public string StockId;
    public int Amount;

    public StockMarketSellMessage(string stockId, int amount)
    {
        StockId = stockId;
        Amount = amount;
    }
}