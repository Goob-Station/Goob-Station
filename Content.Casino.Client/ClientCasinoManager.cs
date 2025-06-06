using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Casino.Shared.Data;
using Content.Casino.Shared.Games;
using Content.Casino.Shared.Network;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Reflection;

namespace Content.Casino.Client;

public interface IClientCasinoManager
{
    /// <summary>
    /// Event fired when a game session starts.
    /// </summary>
    event Func<GameSession, Task>? GameStarted;

    /// <summary>
    /// Event fired when an action result is received.
    /// </summary>
    event Func<string, GameAction, GameActionResult, Task>? ActionResult;

    /// <summary>
    /// Event fired when available actions are updated.
    /// </summary>
    event Func<string, IReadOnlyList<GameAction>, Task>? ActionsUpdated;

    /// <summary>
    /// Event fired when a game session ends.
    /// </summary>
    event Func<string, GameActionResult, Task>? GameEnded;

    /// <summary>
    /// Get all registered client handlers.
    /// </summary>
    IReadOnlyDictionary<string, IGameClientHandler> RegisteredHandlers { get; }

    /// <summary>
    /// Get active game sessions.
    /// </summary>
    IReadOnlyDictionary<string, GameSession> ActiveSessions { get; }

    /// <summary>
    /// Start a new game session.
    /// </summary>
    Task StartGameAsync(string gameId, int initialBet);

    /// <summary>
    /// Execute an action in a game session.
    /// </summary>
    Task ExecuteActionAsync(string sessionId, GameAction action);

    /// <summary>
    /// End a game session.
    /// </summary>
    Task EndGameAsync(string sessionId);

    /// <summary>
    /// Get a specific client handler by type.
    /// </summary>
    T GetHandler<T>() where T : class, IGameClientHandler;

    /// <summary>
    /// Show the UI for a specific game.
    /// </summary>
    Task ShowGameUIAsync(string gameId);

    /// <summary>
    /// Hide the UI for a specific game.
    /// </summary>
    Task HideGameUIAsync(string gameId);
}

public sealed class ClientCasinoManager : IClientCasinoManager, IPostInjectInit
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IDynamicTypeFactory _typeFactory = default!;
    [Dependency] private readonly IReflectionManager _reflectionManager = default!;

    private readonly Dictionary<string, IGameClientHandler> _registeredHandlers = new();
    private readonly Dictionary<string, GameSession> _activeSessions = new();
    private readonly Dictionary<string, TaskCompletionSource<GameStartedMessage>> _pendingGameStarts = new();
    private readonly Dictionary<string, TaskCompletionSource<ActionResultMessage>> _pendingActions = new();

    private readonly object _handlersLock = new();
    private readonly object _sessionsLock = new();
    private readonly object _gameStartsLock = new();
    private readonly object _actionsLock = new();

    public event Func<GameSession, Task>? GameStarted;
    public event Func<string, GameAction, GameActionResult, Task>? ActionResult;
    public event Func<string, IReadOnlyList<GameAction>, Task>? ActionsUpdated;
    public event Func<string, GameActionResult, Task>? GameEnded;

    public IReadOnlyDictionary<string, IGameClientHandler> RegisteredHandlers
    {
        get
        {
            lock (_handlersLock)
            {
                return new Dictionary<string, IGameClientHandler>(_registeredHandlers);
            }
        }
    }

    public IReadOnlyDictionary<string, GameSession> ActiveSessions
    {
        get
        {
            lock (_sessionsLock)
            {
                return new Dictionary<string, GameSession>(_activeSessions);
            }
        }
    }

    public void PostInject()
    {
        RegisterNetworkMessages();
        RegisterClientHandlers();
    }

    private void RegisterNetworkMessages()
    {
        _netManager.RegisterNetMessage<StartGameRequest>();
        _netManager.RegisterNetMessage<GameStartedMessage>(OnGameStarted);
        _netManager.RegisterNetMessage<ExecuteActionRequest>();
        _netManager.RegisterNetMessage<ActionResultMessage>(OnActionResult);
        _netManager.RegisterNetMessage<EndGameRequest>();
        _netManager.RegisterNetMessage<GameEndedMessage>(OnGameEnded);
    }

    private void RegisterClientHandlers()
    {
        var handlerTypes = _reflectionManager.GetAllChildren<IGameClientHandler>()
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                var handler = _typeFactory.CreateInstance<IGameClientHandler>(handlerType);
                handler.Initialize();

                lock (_handlersLock)
                {
                    _registeredHandlers[handler.GameId] = handler;
                }
            }
            catch (Exception ex)
            {
                // Log error - handler failed to register
                var logger = IoCManager.Resolve<ILogManager>().GetSawmill("casino");
                logger.Error($"Failed to register client handler {handlerType.Name}: {ex.Message}");
            }
        }
    }

    private async void OnGameStarted(GameStartedMessage message)
    {
        // Handle pending game start request
        TaskCompletionSource<GameStartedMessage>? tcs = null;
        lock (_gameStartsLock)
        {
            if (_pendingGameStarts.TryGetValue(message.GameId, out tcs))
            {
                _pendingGameStarts.Remove(message.GameId);
            }
        }

        tcs?.SetResult(message);

        if (!message.Success)
            return;

        // Create session object
        var session = new GameSession(
            message.SessionId,
            message.GameId,
            null!, // Player not available on client
            0, // Initial bet not needed on client
            DateTime.Now,
            message.SerializedGameState
        );

        lock (_sessionsLock)
        {
            _activeSessions[message.SessionId] = session;
        }

        // Notify handler
        IGameClientHandler? handler = null;
        lock (_handlersLock)
        {
            _registeredHandlers.TryGetValue(message.GameId, out handler);
        }

        if (handler != null)
        {
            await handler.OnGameStartedAsync(session);

            if (message.AvailableActions.Length > 0)
            {
                await handler.OnActionsUpdatedAsync(message.SessionId, message.AvailableActions);
            }
        }

        // Fire global event
        if (GameStarted != null)
        {
            await GameStarted.Invoke(session);
        }

        // Fire actions updated event
        if (ActionsUpdated != null && message.AvailableActions.Length > 0)
        {
            await ActionsUpdated.Invoke(message.SessionId, message.AvailableActions);
        }
    }

    private async void OnActionResult(ActionResultMessage message)
    {
        // Handle pending action request
        var actionKey = $"{message.SessionId}:{message.Action.ActionId}";
        TaskCompletionSource<ActionResultMessage>? tcs = null;

        lock (_actionsLock)
        {
            if (_pendingActions.TryGetValue(actionKey, out tcs))
            {
                _pendingActions.Remove(actionKey);
            }
        }

        tcs?.SetResult(message);

        // Get session and handler
        GameSession? session = null;
        IGameClientHandler? handler = null;

        lock (_sessionsLock)
        {
            _activeSessions.TryGetValue(message.SessionId, out session);
        }

        if (session != null)
        {
            lock (_handlersLock)
            {
                _registeredHandlers.TryGetValue(session.GameId, out handler);
            }
        }

        // Notify handler
        if (session != null && handler != null)
        {
            await handler.OnActionResultAsync(message.SessionId, message.Action, message.Result);

            if (message.UpdatedActions.Length > 0)
            {
                await handler.OnActionsUpdatedAsync(message.SessionId, message.UpdatedActions);
            }

            // If game is complete, clean up session
            if (message.Result.IsComplete)
            {
                await handler.OnGameEndedAsync(message.SessionId, message.Result);
                CleanupClientSession(message.SessionId);
            }
        }

        // Fire global events
        if (ActionResult != null)
        {
            await ActionResult.Invoke(message.SessionId, message.Action, message.Result);
        }

        if (ActionsUpdated != null && message.UpdatedActions.Length > 0)
        {
            await ActionsUpdated.Invoke(message.SessionId, message.UpdatedActions);
        }

        if (GameEnded != null && message.Result.IsComplete)
        {
            await GameEnded.Invoke(message.SessionId, message.Result);
        }
    }

    private async void OnGameEnded(GameEndedMessage message)
    {
        // Get session and handler before cleanup
        GameSession? session = null;
        IGameClientHandler? handler = null;

        lock (_sessionsLock)
        {
            _activeSessions.TryGetValue(message.SessionId, out session);
        }

        if (session != null)
        {
            lock (_handlersLock)
            {
                _registeredHandlers.TryGetValue(session.GameId, out handler);
            }
        }

        // Notify handler first
        if (session != null && handler != null)
        {
            await handler.OnGameEndedAsync(message.SessionId, message.FinalResult);
        }

        // Clean up session
        CleanupClientSession(message.SessionId);

        // Fire global event
        if (GameEnded != null)
        {
            await GameEnded.Invoke(message.SessionId, message.FinalResult);
        }
    }

    private void CleanupClientSession(string sessionId)
    {
        lock (_sessionsLock)
        {
            _activeSessions.Remove(sessionId);
        }
    }

    public async Task StartGameAsync(string gameId, int initialBet)
    {
        var tcs = new TaskCompletionSource<GameStartedMessage>();

        lock (_gameStartsLock)
        {
            _pendingGameStarts[gameId] = tcs;
        }

        var request = new StartGameRequest
        {
            GameId = gameId,
            InitialBet = initialBet
        };

        _netManager.ClientSendMessage(request);

        // Wait for response with timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            lock (_gameStartsLock)
            {
                _pendingGameStarts.Remove(gameId);
            }
            throw new CasinoTimeoutException("Game start request timed out");
        }

        var result = await tcs.Task;
        if (!result.Success)
        {
            throw new CasinoGameException(result.ErrorMessage, gameId, null);
        }
    }

    public async Task ExecuteActionAsync(string sessionId, GameAction action)
    {
        var actionKey = $"{sessionId}:{action.ActionId}";
        var tcs = new TaskCompletionSource<ActionResultMessage>();

        lock (_actionsLock)
        {
            _pendingActions[actionKey] = tcs;
        }

        var request = new ExecuteActionRequest
        {
            SessionId = sessionId,
            Action = action
        };

        _netManager.ClientSendMessage(request);

        // Wait for response with timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            lock (_actionsLock)
            {
                _pendingActions.Remove(actionKey);
            }
            throw new CasinoTimeoutException("Action request timed out");
        }

        var result = await tcs.Task;
        if (!result.Result.IsComplete && result.Result.Payout == 0 && !result.Result.Won)
        {
            // Check if this was an error (simple heuristic)
            if (string.IsNullOrEmpty(result.Result.Message))
            {
                throw new CasinoGameException("Action failed", null, sessionId);
            }
        }
    }

    public async Task EndGameAsync(string sessionId)
    {
        var request = new EndGameRequest
        {
            SessionId = sessionId
        };

        _netManager.ClientSendMessage(request);

        // End game is fire-and-forget, but we should clean up client state
        // The server will send back a GameEndedMessage which will trigger proper cleanup
        await Task.CompletedTask;
    }

    public T GetHandler<T>() where T : class, IGameClientHandler
    {
        var gameId = typeof(T).Name.ToLowerInvariant().Replace("clienthandler", "").Replace("client", "");

        lock (_handlersLock)
        {
            if (_registeredHandlers.TryGetValue(gameId, out var handler) && handler is T typedHandler)
                return typedHandler;

            // Fallback: search by type
            var foundHandler = _registeredHandlers.Values.OfType<T>().FirstOrDefault();
            if (foundHandler != null)
                return foundHandler;
        }

        throw new CasinoGameException($"Client handler of type {typeof(T).Name} is not registered");
    }

    public async Task ShowGameUIAsync(string gameId)
    {
        IGameClientHandler? handler = null;
        lock (_handlersLock)
        {
            _registeredHandlers.TryGetValue(gameId, out handler);
        }

        if (handler != null)
        {
            await handler.ShowGameUIAsync();
        }
    }

    public async Task HideGameUIAsync(string gameId)
    {
        IGameClientHandler? handler = null;
        lock (_handlersLock)
        {
            _registeredHandlers.TryGetValue(gameId, out handler);
        }

        if (handler != null)
        {
            await handler.HideGameUIAsync();
        }
    }
}
