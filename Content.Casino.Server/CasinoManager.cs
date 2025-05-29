using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Content.Casino.Shared.Games;
using Content.Casino.Shared.Network;
using Content.Server._durkcode.ServerCurrency;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Reflection;

namespace Content.Casino.Server;

public interface IServerCasinoManager
{
    /// <summary>
    /// Get all registered games.
    /// </summary>
    IReadOnlyDictionary<string, ICasinoGame> RegisteredGames { get; }

    /// <summary>
    /// Get all active game sessions.
    /// </summary>
    IReadOnlyDictionary<string, GameSession> ActiveSessions { get; }

    /// <summary>
    /// Start a new game session.
    /// </summary>
    Task<(bool Success, GameSession? Session, string ErrorMessage)> StartGameAsync(
        ICommonSession player,
        string gameId,
        int initialBet,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an action in a game session.
    /// </summary>
    Task<(bool Success, GameActionResult Result, string ErrorMessage)> ExecuteActionAsync(
        string sessionId,
        GameAction action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// End a game session.
    /// </summary>
    Task<(bool Success, GameActionResult FinalResult, string ErrorMessage)> EndGameAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific game by type.
    /// </summary>
    T GetGame<T>() where T : class, ICasinoGame;

    /// <summary>
    /// Check if a player can afford the bet.
    /// </summary>
    bool CanAffordBet(ICommonSession player, int bet);

    /// <summary>
    /// Process a bet payment.
    /// </summary>
    bool ProcessBet(ICommonSession player, int bet);

    /// <summary>
    /// Process a payout.
    /// </summary>
    void ProcessPayout(ICommonSession player, int payout);
}

public sealed class ServerCasinoManager : IServerCasinoManager, IPostInjectInit
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly ServerCurrencyManager _currencyManager = default!;
    [Dependency] private readonly IDynamicTypeFactory _typeFactory = default!;
    [Dependency] private readonly IReflectionManager _reflectionManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    private readonly ConcurrentDictionary<string, ICasinoGame> _registeredGames = new();
    private readonly ConcurrentDictionary<string, GameSession> _activeSessions = new();
    private readonly ConcurrentDictionary<string, SessionLockWrapper> _sessionLocks = new();

    // Track sessions being cleaned up to prevent double disposal
    private readonly ConcurrentDictionary<string, bool> _sessionsBeingCleaned = new();

    public IReadOnlyDictionary<string, ICasinoGame> RegisteredGames => _registeredGames;
    public IReadOnlyDictionary<string, GameSession> ActiveSessions => _activeSessions;

    /// <summary>
    /// Wrapper for semaphore that tracks disposal state to prevent ObjectDisposedException
    /// </summary>
    private sealed class SessionLockWrapper : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private volatile bool _disposed = false;
        private readonly object _disposeLock = new();

        public SessionLockWrapper()
        {
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<IDisposable?> TryWaitAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) return null;

            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                if (_disposed)
                {
                    // If disposed after acquiring, release and return null
                    _semaphore.Release();
                    return null;
                }
                return new SemaphoreReleaser(_semaphore, this);
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed) return;
                _disposed = true;
                _semaphore.Dispose();
            }
        }

        private sealed class SemaphoreReleaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private readonly SessionLockWrapper _wrapper;
            private bool _released = false;

            public SemaphoreReleaser(SemaphoreSlim semaphore, SessionLockWrapper wrapper)
            {
                _semaphore = semaphore;
                _wrapper = wrapper;
            }

            public void Dispose()
            {
                if (_released || _wrapper._disposed) return;
                _released = true;

                try
                {
                    _semaphore.Release();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore - semaphore was already disposed
                }
            }
        }
    }

    public void PostInject()
    {
        RegisterNetworkMessages();
        RegisterGames();
    }

    private void RegisterNetworkMessages()
    {
        _netManager.RegisterNetMessage<StartGameRequest>(OnStartGameRequest);
        _netManager.RegisterNetMessage<GameStartedMessage>();
        _netManager.RegisterNetMessage<ExecuteActionRequest>(OnExecuteActionRequest);
        _netManager.RegisterNetMessage<ActionResultMessage>();
        _netManager.RegisterNetMessage<EndGameRequest>(OnEndGameRequest);
        _netManager.RegisterNetMessage<GameEndedMessage>();
    }

    private void RegisterGames()
    {
        var gameTypes = _reflectionManager.GetAllChildren<ICasinoGame>()
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var gameType in gameTypes)
        {
            try
            {
                var game = _typeFactory.CreateInstance<ICasinoGame>(gameType);
                game.Initialize();
                _registeredGames[game.GameId] = game;
            }
            catch (Exception ex)
            {
                // Log error - game failed to register
                Console.WriteLine($"Failed to register game {gameType.Name}: {ex.Message}");
            }
        }
    }

    private async void OnStartGameRequest(StartGameRequest message)
    {
        var player = _playerManager.GetSessionByChannel(message.MsgChannel);
        var (success, session, errorMessage) = await StartGameAsync(player, message.GameId, message.InitialBet);

        var response = new GameStartedMessage
        {
            SessionId = session?.SessionId ?? string.Empty,
            GameId = message.GameId,
            Success = success,
            ErrorMessage = errorMessage
        };

        if (success && session != null)
        {
            var actions = await _registeredGames[message.GameId].GetAvailableActionsAsync(session.SessionId);
            response.AvailableActions = actions.ToArray();
            response.SerializedGameState = session.GameState?.ToString();
        }

        _netManager.ServerSendMessage(response, message.MsgChannel);
    }

    private async void OnExecuteActionRequest(ExecuteActionRequest message)
    {
        var (success, result, errorMessage) = await ExecuteActionAsync(message.SessionId, message.Action);

        var response = new ActionResultMessage
        {
            SessionId = message.SessionId,
            Action = message.Action,
            Result = success ? result : new GameActionResult(false, false, 0, errorMessage)
        };

        if (success && _activeSessions.TryGetValue(message.SessionId, out var session))
        {
            var actions = await _registeredGames[session.GameId].GetAvailableActionsAsync(session.SessionId);
            response.UpdatedActions = actions.ToArray();

            var player = session.Player;
            _netManager.ServerSendMessage(response, player.Channel);
        }
        else if (_activeSessions.TryGetValue(message.SessionId, out var failedSession))
        {
            // Send error response even if action failed
            _netManager.ServerSendMessage(response, failedSession.Player.Channel);
        }
    }

    private async void OnEndGameRequest(EndGameRequest message)
    {
        var (success, finalResult, errorMessage) = await EndGameAsync(message.SessionId);

        // Always send response, even for sessions that don't exist
        var response = new GameEndedMessage
        {
            SessionId = message.SessionId,
            FinalResult = success ? finalResult : new GameActionResult(true, false, 0, errorMessage)
        };

        // Try to get the session to find the player channel
        if (_activeSessions.TryGetValue(message.SessionId, out var session))
        {
            _netManager.ServerSendMessage(response, session.Player.Channel);
        }
        else
        {
            // Session not found, but we still need to respond to the requesting player
            var player = _playerManager.GetSessionByChannel(message.MsgChannel);
            if (player != null)
            {
                _netManager.ServerSendMessage(response, player.Channel);
            }
        }
    }

    public async Task<(bool Success, GameSession? Session, string ErrorMessage)> StartGameAsync(
        ICommonSession player,
        string gameId,
        int initialBet,
        CancellationToken cancellationToken = default)
    {
        if (!_registeredGames.TryGetValue(gameId, out var game))
            return (false, null, $"Game '{gameId}' not found");

        if (initialBet < game.MinBet || initialBet > game.MaxBet)
            return (false, null, $"Bet must be between {game.MinBet} and {game.MaxBet}");

        if (!CanAffordBet(player, initialBet))
            return (false, null, "Insufficient funds");

        if (!ProcessBet(player, initialBet))
            return (false, null, "Failed to process bet");

        try
        {
            var session = await game.StartGameAsync(player, initialBet, cancellationToken);
            _activeSessions[session.SessionId] = session;
            _sessionLocks[session.SessionId] = new SessionLockWrapper();

            return (true, session, string.Empty);
        }
        catch (Exception ex)
        {
            // Refund the bet if game creation failed
            ProcessPayout(player, initialBet);
            return (false, null, $"Failed to start game: {ex.Message}");
        }
    }

    public async Task<(bool Success, GameActionResult Result, string ErrorMessage)> ExecuteActionAsync(
        string sessionId,
        GameAction action,
        CancellationToken cancellationToken = default)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session))
            return (false, default, "Session not found");

        if (!_registeredGames.TryGetValue(session.GameId, out var game))
            return (false, default, "Game not found");

        if (!_sessionLocks.TryGetValue(sessionId, out var sessionLockWrapper))
            return (false, default, "Session lock not found");

        // Prevent actions on sessions being cleaned up
        if (_sessionsBeingCleaned.ContainsKey(sessionId))
            return (false, default, "Session is being terminated");

        using var lockHandle = await sessionLockWrapper.TryWaitAsync(cancellationToken);
        if (lockHandle == null)
            return (false, default, "Session is being terminated");

        try
        {
            // Double-check session still exists after acquiring lock
            if (!_activeSessions.ContainsKey(sessionId))
                return (false, default, "Session no longer exists");

            // Check if this action requires payment
            var actionCost = await game.GetActionCostAsync(sessionId, action, cancellationToken);

            if (actionCost.RequiresPayment)
            {
                if (!CanAffordBet(session.Player, actionCost.Cost))
                    return (false, default, "Insufficient funds for action");

                if (!ProcessBet(session.Player, actionCost.Cost))
                    return (false, default, "Failed to process payment for action");
            }

            var result = await game.ExecuteActionAsync(sessionId, action, cancellationToken);

            // Process payout if won
            if (result.Won && result.Payout > 0)
            {
                ProcessPayout(session.Player, result.Payout);
            }

            // Clean up session if game is complete
            if (result.IsComplete)
            {
                // Schedule cleanup without blocking current operation
                _ = Task.Run(() => CleanupSessionAsync(sessionId));
            }

            return (true, result, string.Empty);
        }
        catch (Exception ex)
        {
            // If we charged for an action but it failed, refund it
            try
            {
                var actionCost = await game.GetActionCostAsync(sessionId, action, cancellationToken);
                if (actionCost.RequiresPayment)
                {
                    ProcessPayout(session.Player, actionCost.Cost); // Refund
                }
            }
            catch
            {
                // Ignore errors during refund attempt
            }

            return (false, default, $"Action failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, GameActionResult FinalResult, string ErrorMessage)> EndGameAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session))
            return (false, new GameActionResult(true, false, 0, "Session not found"), "Session not found");

        if (!_registeredGames.TryGetValue(session.GameId, out var game))
            return (false, new GameActionResult(true, false, 0, "Game not found"), "Game not found");

        try
        {
            // Tell the game to clean up its state
            await game.EndGameAsync(sessionId, cancellationToken);

            // Clean up our session tracking
            await CleanupSessionAsync(sessionId);

            var finalResult = new GameActionResult(true, false, 0, "Game ended by player");
            return (true, finalResult, string.Empty);
        }
        catch (Exception ex)
        {
            // Even if cleanup fails, remove the session from our tracking
            await CleanupSessionAsync(sessionId);
            return (false, new GameActionResult(true, false, 0, $"Failed to end game: {ex.Message}"), $"Failed to end game: {ex.Message}");
        }
    }

    private async Task CleanupSessionAsync(string sessionId)
    {
        // Mark session as being cleaned up to prevent new operations
        _sessionsBeingCleaned[sessionId] = true;

        // Small delay to allow any in-progress operations to complete
        await Task.Delay(100);

        try
        {
            _activeSessions.TryRemove(sessionId, out _);

            if (_sessionLocks.TryRemove(sessionId, out var lockWrapper))
            {
                lockWrapper.Dispose();
            }
        }
        finally
        {
            // Remove from cleanup tracking
            _sessionsBeingCleaned.TryRemove(sessionId, out _);
        }
    }

    public T GetGame<T>() where T : class, ICasinoGame
    {
        var gameId = typeof(T).Name.ToLowerInvariant().Replace("game", "");

        if (_registeredGames.TryGetValue(gameId, out var game) && game is T typedGame)
            return typedGame;

        // Fallback: search by type
        var foundGame = _registeredGames.Values.OfType<T>().FirstOrDefault();
        if (foundGame != null)
            return foundGame;

        throw new InvalidOperationException($"Game of type {typeof(T).Name} is not registered");
    }

    public bool CanAffordBet(ICommonSession player, int bet)
    {
        return _currencyManager.GetBalance(player.UserId) >= bet;
    }

    public bool ProcessBet(ICommonSession player, int bet)
    {
        if (!CanAffordBet(player, bet))
            return false;

        _currencyManager.RemoveCurrency(player.UserId, bet);
        return true;
    }

    public void ProcessPayout(ICommonSession player, int payout)
    {
        if (payout > 0)
        {
            _currencyManager.AddCurrency(player.UserId, payout);
        }
    }
}
