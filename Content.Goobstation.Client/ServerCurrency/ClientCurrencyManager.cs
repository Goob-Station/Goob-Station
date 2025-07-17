using Content.Goobstation.Common.ServerCurrency;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.ServerCurrency;

public sealed class ClientCurrencyManager : ICommonCurrencyManager, IEntityEventSubscriber, IPostInjectInit
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPlayerManager _playMan = default!;

    private static int _cachedBalance = -1;
    public event Action? ClientBalanceChange;
    public event Action<PlayerBalanceChangeEvent>? BalanceChange;

    public void Initialize() {}

    public void PostInject()
    {
        _playMan.PlayerStatusChanged += OnStatusChanged;
    }

    private void OnStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        /*
         * This looks fucked, so I'll explain
         * With durk server currency currently it's reliant on events, events rely on something called an EventBus.
         * Client part of this hellhole is made entirely by an in-sim EntitySystem.
         * At the time PostInject is run - EntityManager did not yet initialize EventBus.
         * So we have to subscribe to events between "We connected to video game" and "We are in the video game"
         *
         * Also I really wanted to manually try out EventBus subscriptions outside EntitySystems.
         */
        if (e.NewStatus == SessionStatus.Connected)
            _ent.EventBus.SubscribeSessionEvent<PlayerBalanceUpdateEvent>(EventSource.Network, this, UpdateBalance);

        if (e.NewStatus != SessionStatus.InGame)
            return;

        var ev = new PlayerBalanceRequestEvent();
        _ent.EventBus.RaiseEvent(EventSource.Network, ev);
    }

    private void UpdateBalance(PlayerBalanceUpdateEvent msg, EntitySessionEventArgs args)
    {
        _cachedBalance = msg.NewBalance;
        ClientBalanceChange?.Invoke();
    }

    public void Shutdown()
    {
        throw new NotImplementedException();
    }

    public bool CanAfford(NetUserId? userId, int amount, out int balance)
    {
        balance = _cachedBalance;
        return balance >= amount && balance - amount >= 0;
    }

    /// <inheritdoc/>
    public string Stringify(int amount) => amount == 1
        ? $"{amount} {Loc.GetString("server-currency-name-singular")}"
        : $"{amount} {Loc.GetString("server-currency-name-plural")}";

    public int AddCurrency(NetUserId userId, int amount)
    {
        throw new NotImplementedException();
    }

    public int RemoveCurrency(NetUserId userId, int amount)
    {
        throw new NotImplementedException();
    }

    public (int, int) TransferCurrency(NetUserId sourceUserId, NetUserId targetUserId, int amount)
    {
        throw new NotImplementedException();
    }

    public int SetBalance(NetUserId userId, int amount)
    {
        throw new NotImplementedException();
    }

    public int GetBalance(NetUserId? userId = null)
    {
        return _cachedBalance;
    }
}
