using Content.Goobstation.Shared.StockMarket;

namespace Content.Goobstation.Client.StockMarket;

public sealed class StockMarketConsoleBoundUserInterface : BoundUserInterface
{
    private StockMarketConsoleWindow? _window;

    public StockMarketConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _window = new StockMarketConsoleWindow();
        _window.OpenCentered();
        _window.OnClose += Close;

        _window.OnBuyPressed += (stockId, amount) =>
            SendMessage(new StockMarketBuyMessage(stockId, amount));

        _window.OnSellPressed += (stockId, amount) =>
            SendMessage(new StockMarketSellMessage(stockId, amount));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is StockMarketConsoleState castState)
            _window?.UpdateState(castState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        _window?.Dispose();
    }
}
