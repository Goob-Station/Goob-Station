using Robust.Shared.Network;

namespace Content.Shared._Goobstation.ServerCurrency;

public interface ISharedServerCurrencyManager
{
    bool CanAfford(NetUserId userId, int amount, out int balance);
    int GetBalance(NetUserId userId);
    string Stringify(int amount);
}

