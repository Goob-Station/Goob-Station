using Content.Shared._Goobstation.ServerCurrency.Events;


namespace Content.Client._Goobstation.ServerCurrency;

public sealed class ServerCurrencySystem : EntitySystem
{
    private static int _cachedBalance = -1;
    public event Action? BalanceChange;

    public override void Initialize()
    {
        SubscribeNetworkEvent<PlayerBalanceUpdateEvent>(UpdateBalance); // We should probably be using net messages here instead, but I dont feel like messing with that right now.
        RaiseNetworkEvent(new PlayerBalanceRequestEvent());
    }
    private void UpdateBalance(PlayerBalanceUpdateEvent ev)
    {
        _cachedBalance = ev.NewBalance;
        BalanceChange?.Invoke();
    }

    /// <summary>
    /// Checks if the player has enough currency to purchase something.
    /// </summary>
    /// <param name="amount">The amount of currency needed.</param>
    /// <param name="balance">The player's balance.</param>
    /// <returns>Returns true if the player has enough in their balance.</returns>
    public bool CanAfford(int amount, out int balance)
    {
        balance = _cachedBalance;
        return balance >= amount && balance - amount >= 0;
    }

    /// <summary>
    /// Gets a player's balance.
    /// </summary>
    /// <returns>The players balance.</returns>
    public int GetBalance()
    {
        return _cachedBalance;
    }


    /// <summary>
    /// Converts an integer to a string representing the count followed by the appropriate currency localization (singular or plural) defined in server_currency.ftl.
    /// Useful for displaying balances and prices.
    /// </summary>
    /// <param name="amount">The amount of currency to display.</param>
    /// <returns>A string containing the count and the correct form of the currency name.</returns>
    public string Stringify(int amount) => amount == 1
        ? $"{amount} {Loc.GetString("server-currency-name-singular")}"
        : $"{amount} {Loc.GetString("server-currency-name-plural")}";
}

