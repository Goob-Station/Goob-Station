using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Casino.Shared.Data;
using Content.Casino.Shared.Games;
using Content.Server._durkcode.ServerCurrency;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Reflection;

namespace Content.Casino.Server;

public interface IServerCasinoManager
{
    public HashSet<Escrow> Escrows { get; }
    public bool TryCreateStake(ICommonSession session, int stake, [NotNullWhen(true)] out Escrow? escrow);
    public void ResolveStake(int gameId, GameResult result);
    public T GetGame<T>() where T : ICasinoGame;
}

public sealed class CasinoManager : IServerCasinoManager, IPostInjectInit
{
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly ServerCurrencyManager _curr = default!;
    [Dependency] private readonly IDynamicTypeFactory _dynType = default!;
    [Dependency] private readonly IReflectionManager _refl = default!;

    private readonly HashSet<Escrow> _escrows = new();
    private readonly Dictionary<Type, ICasinoGame> _registeredGames = new();

    private int _gameId = 0;

    public void PostInject()
    {
        RegisterGames();
    }

    public HashSet<Escrow> Escrows => _escrows;

    public bool TryCreateStake(ICommonSession session, int stake, [NotNullWhen(true)] out Escrow? escrow)
    {
        escrow = null;
        if (!TryTake(session, stake))
            return false;

        escrow = new Escrow(session, _gameId, stake);
        _escrows.Add(escrow.Value);
        _gameId++;
        return true;
    }

    public void ResolveStake(int gameId, GameResult result)
    {
        if (result.Won)
            Payout(gameId, result.Payout);

        DestroyStake(gameId);
    }

    private void DestroyStake(int gameId)
    {
        _escrows.RemoveWhere(e => e.GameId == gameId);
    }

    #region Currency

    private bool TryTake(ICommonSession session, int stake)
    {
        if (_curr.GetBalance(session.UserId) < stake)
            return false;

        _curr.RemoveCurrency(session.UserId, stake);
        return true;
    }

    private void Payout(int gameId, int payout)
    {
        var escrow = Escrows.SingleOrDefault(e => e.GameId == gameId);
        _curr.AddCurrency(escrow.Session.UserId, payout);
    }

    #endregion

    #region Registration

    private void RegisterGames()
    {
        var gameTypes = _refl.GetAllChildren<ICasinoGame>();

        foreach (var gameType in gameTypes)
        {
            // Fucking ram it
            var game = _dynType.CreateInstance<ICasinoGame>(gameType);
            game.Initialize();
            _registeredGames[gameType] = game;
        }
    }

    public T GetGame<T>() where T : ICasinoGame
    {
        if (_registeredGames.TryGetValue(typeof(T), out var game))
        {
            return (T)game;
        }

        throw new InvalidOperationException($"Game of type {typeof(T).Name} is not registered");
    }

    public IEnumerable<ICasinoGame> GetAllGames()
    {
        return _registeredGames.Values;
    }

    #endregion
}
