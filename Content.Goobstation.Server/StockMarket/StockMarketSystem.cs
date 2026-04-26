using Content.Goobstation.Shared.StockMarket;
using Content.Server.Cargo.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Cargo.Prototypes;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.StockMarket;

public sealed class StockMarketSystem : SharedStockMarketSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    private const int MaxPriceHistory = 20;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationStockMarketComponent, MapInitEvent>(OnMarketMapInit);
        SubscribeLocalEvent<StockMarketConsoleComponent, StockMarketBuyMessage>(OnBuyMessage);
        SubscribeLocalEvent<StockMarketConsoleComponent, StockMarketSellMessage>(OnSellMessage);
    }

    private void OnMarketMapInit(EntityUid uid, StationStockMarketComponent comp, MapInitEvent args)
    {
        // Initialize all stocks from prototypes
        foreach (var proto in _proto.EnumeratePrototypes<StockMarketPrototype>())
        {
            if (comp.Stocks.ContainsKey(proto.ID))
                continue;

            comp.Stocks[proto.ID] = new StockEntry
            {
                Price = proto.BasePrice,
                PriceHistory = new List<float> { proto.BasePrice },
            };
        }

        comp.NextTick = _timing.CurTime + comp.TickDelay;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<StationStockMarketComponent>();

        while (query.MoveNext(out var uid, out var market))
        {
            if (now < market.NextTick)
                continue;

            market.NextTick = now + market.TickDelay;
            TickPrices(uid, market);
            UpdateAllConsoles(uid, market);
        }
    }

    private void TickPrices(EntityUid station, StationStockMarketComponent market)
    {
        foreach (var (stockId, entry) in market.Stocks)
        {
            if (!_proto.TryIndex<StockMarketPrototype>(stockId, out var proto))
                continue;

            // Geometric Brownian Motion-style random walk
            var randomFactor = (_random.NextFloat() * 2f - 1f) * proto.Volatility;
            var change = 1f + randomFactor + proto.Drift;
            entry.Price = Math.Clamp(entry.Price * change, proto.MinPrice, proto.MaxPrice);

            // Round to 2 decimal places
            entry.Price = MathF.Round(entry.Price, 2);

            // Maintain price history
            entry.PriceHistory.Add(entry.Price);
            if (entry.PriceHistory.Count > MaxPriceHistory)
                entry.PriceHistory.RemoveAt(0);
        }

        Dirty(station, market);
    }

    private void OnBuyMessage(EntityUid uid, StockMarketConsoleComponent console, StockMarketBuyMessage args)
    {
        if (args.Amount <= 0)
            return;

        var stationUid = _station.GetOwningStation(uid);
        if (stationUid == null)
            return;

        if (!TryComp<StationStockMarketComponent>(stationUid, out var market))
            return;

        if (!market.Stocks.TryGetValue(args.StockId, out var entry))
            return;

        var totalCost = (int) MathF.Ceiling(entry.Price * args.Amount);
        var accountId = console.Account;

        var balance = _cargo.GetBalanceFromAccount(stationUid.Value, accountId);
        if (balance < totalCost)
            return;

        // Deduct funds
        _cargo.UpdateBankAccount(stationUid.Value, -totalCost, accountId);

        // Add shares
        entry.OwnedShares.TryGetValue(accountId, out var currentShares);
        entry.OwnedShares[accountId] = currentShares + args.Amount;

        Dirty(stationUid.Value, market);
        UpdateAllConsoles(stationUid.Value, market);
    }

    private void OnSellMessage(EntityUid uid, StockMarketConsoleComponent console, StockMarketSellMessage args)
    {
        if (args.Amount <= 0)
            return;

        var stationUid = _station.GetOwningStation(uid);
        if (stationUid == null)
            return;

        if (!TryComp<StationStockMarketComponent>(stationUid, out var market))
            return;

        if (!market.Stocks.TryGetValue(args.StockId, out var entry))
            return;

        var accountId = console.Account;
        entry.OwnedShares.TryGetValue(accountId, out var currentShares);

        if (currentShares < args.Amount)
            return;

        // Add funds
        var totalValue = (int) MathF.Floor(entry.Price * args.Amount);
        _cargo.UpdateBankAccount(stationUid.Value, totalValue, accountId);

        // Remove shares
        entry.OwnedShares[accountId] = currentShares - args.Amount;
        if (entry.OwnedShares[accountId] <= 0)
            entry.OwnedShares.Remove(accountId);

        Dirty(stationUid.Value, market);
        UpdateAllConsoles(stationUid.Value, market);
    }

    private void UpdateAllConsoles(EntityUid station, StationStockMarketComponent market)
    {
        var consoleQuery = EntityQueryEnumerator<StockMarketConsoleComponent, TransformComponent>();
        while (consoleQuery.MoveNext(out var consoleUid, out var console, out var xform))
        {
            if (_station.GetOwningStation(consoleUid, xform) != station)
                continue;

            if (!_ui.IsUiOpen(consoleUid, StockMarketConsoleUiKey.Key))
                continue;

            var accountId = console.Account;
            var balance = _cargo.GetBalanceFromAccount(station, accountId);

            var stockData = new Dictionary<string, StockDisplayData>();
            foreach (var (stockId, entry) in market.Stocks)
            {
                if (!_proto.TryIndex<StockMarketPrototype>(stockId, out var proto))
                    continue;

                var previousPrice = entry.PriceHistory.Count >= 2
                    ? entry.PriceHistory[^2]
                    : entry.Price;

                entry.OwnedShares.TryGetValue(accountId, out var owned);

                stockData[stockId] = new StockDisplayData(
                    Loc.GetString(proto.Name),
                    Loc.GetString(proto.Description),
                    Loc.GetString(proto.Category),
                    entry.Price,
                    previousPrice,
                    new List<float>(entry.PriceHistory),
                    owned
                );
            }

            _ui.SetUiState(consoleUid, StockMarketConsoleUiKey.Key,
                new StockMarketConsoleState(stockData, accountId, balance));
        }
    }
}