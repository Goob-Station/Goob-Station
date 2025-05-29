using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Casino.Server.Data;
using Content.Casino.Shared.Cvars;
using Content.Casino.Shared.Data;
using Content.Casino.Shared.Games;
using Content.Casino.Shared.Network;
using Content.Server._durkcode.ServerCurrency;
using Robust.Shared.Configuration;
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
    Task<StartGameResult> StartGameAsync(
        ICommonSession player,
        string gameId,
        int initialBet,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an action in a game session.
    /// </summary>
    Task<ExecuteActionResult> ExecuteActionAsync(
        string sessionId,
        GameAction action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// End a game session.
    /// </summary>
    Task<EndGameResult> EndGameAsync(
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
    [Dependency] private readonly IConfigurationManager _configuration = default!;

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
            if (_disposed)
                return null;

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
                if (_disposed)
                    return;

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
                if (_released || _wrapper._disposed)
                    return;

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

    // Updated network message handlers to use the new records
    private async void OnStartGameRequest(StartGameRequest message)
    {
        var player = _playerManager.GetSessionByChannel(message.MsgChannel);
        var result = await StartGameAsync(player, message.GameId, message.InitialBet);

        var response = new GameStartedMessage
        {
            SessionId = result.Session?.SessionId ?? string.Empty,
            GameId = message.GameId,
            Success = result.Success,
            ErrorMessage = result.ErrorMessage
        };

        if (result is { Success: true, Session: not null })
        {
            var actions = await _registeredGames[message.GameId].GetAvailableActionsAsync(result.Session.SessionId);
            response.AvailableActions = actions.ToArray();
            response.SerializedGameState = result.Session.GameState?.ToString();
        }

        _netManager.ServerSendMessage(response, message.MsgChannel);
    }

    private async void OnExecuteActionRequest(ExecuteActionRequest message)
    {
        var result = await ExecuteActionAsync(message.SessionId, message.Action);

        var response = new ActionResultMessage
        {
            SessionId = message.SessionId,
            Action = message.Action,
            Result = result.Success ? result.Result : new GameActionResult(false, false, 0, result.ErrorMessage)
        };

        if (result.Success && _activeSessions.TryGetValue(message.SessionId, out var session))
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
        var result = await EndGameAsync(message.SessionId);

        // Always send response, even for sessions that don't exist
        var response = new GameEndedMessage
        {
            SessionId = message.SessionId,
            FinalResult = result.FinalResult
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

    public async Task<StartGameResult> StartGameAsync(
        ICommonSession player,
        string gameId,
        int initialBet,
        CancellationToken cancellationToken = default)
    {
        if (!_registeredGames.TryGetValue(gameId, out var game))
            return StartGameResult.Failed($"Game '{gameId}' not found");

        if (initialBet < game.MinBet || initialBet > game.MaxBet)
            return StartGameResult.Failed($"Bet must be between {game.MinBet} and {game.MaxBet}");

        if (!CanAffordBet(player, initialBet))
            return StartGameResult.Failed("Insufficient funds");

        if (!ProcessBet(player, initialBet))
            return StartGameResult.Failed("Failed to process bet");

        if (!_configuration.GetCVar(CasinoCVars.CasinoEnabled))
            return StartGameResult.Failed("Casino is disabled.");

        try
        {
            var session = await game.StartGameAsync(player, initialBet, cancellationToken);
            _activeSessions[session.SessionId] = session;
            _sessionLocks[session.SessionId] = new SessionLockWrapper();

            return StartGameResult.Successful(session);
        }
        catch (Exception ex)
        {
            // Refund the bet if game creation failed
            ProcessPayout(player, initialBet);
            return StartGameResult.Failed($"Failed to start game: {ex.Message}");
        }
    }

    public async Task<ExecuteActionResult> ExecuteActionAsync(
        string sessionId,
        GameAction action,
        CancellationToken cancellationToken = default)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session))
            return ExecuteActionResult.Failed("Session not found");

        if (!_registeredGames.TryGetValue(session.GameId, out var game))
            return ExecuteActionResult.Failed("Game not found");

        if (!_sessionLocks.TryGetValue(sessionId, out var sessionLockWrapper))
            return ExecuteActionResult.Failed("Session lock not found");

        // Prevent actions on sessions being cleaned up
        if (_sessionsBeingCleaned.ContainsKey(sessionId))
            return ExecuteActionResult.Failed("Session is being terminated");

        using var lockHandle = await sessionLockWrapper.TryWaitAsync(cancellationToken);
        if (lockHandle == null)
            return ExecuteActionResult.Failed("Session is being terminated");

        try
        {
            // Double-check session still exists after acquiring lock
            if (!_activeSessions.ContainsKey(sessionId))
                return ExecuteActionResult.Failed("Session no longer exists");

            // Check if this action requires payment
            var actionCost = await game.GetActionCostAsync(sessionId, action, cancellationToken);

            if (actionCost.RequiresPayment)
            {
                if (!CanAffordBet(session.Player, actionCost.Cost))
                    return ExecuteActionResult.Failed("Insufficient funds for action");

                if (!ProcessBet(session.Player, actionCost.Cost))
                    return ExecuteActionResult.Failed("Failed to process payment for action");
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

            return ExecuteActionResult.Successful(result);
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

            return ExecuteActionResult.Failed($"Action failed: {ex.Message}");
        }
    }
    public async Task<EndGameResult> EndGameAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session))
            return EndGameResult.Failed("Session not found");

        if (!_registeredGames.TryGetValue(session.GameId, out var game))
            return EndGameResult.Failed("Game not found");

        try
        {
            // Tell the game to clean up its state
            await game.EndGameAsync(sessionId, cancellationToken);

            // Clean up our session tracking
            await CleanupSessionAsync(sessionId);

            var finalResult = new GameActionResult(true, false, 0, "Game ended by player");
            return EndGameResult.Successful(finalResult);
        }
        catch (Exception ex)
        {
            // Even if cleanup fails, remove the session from our tracking
            await CleanupSessionAsync(sessionId);
            return EndGameResult.Failed($"Failed to end game: {ex.Message}");
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
