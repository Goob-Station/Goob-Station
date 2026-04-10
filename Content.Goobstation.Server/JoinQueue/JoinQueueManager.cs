using System.Linq;
using Content.Server.Connection;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Shared.CCVar;
using Content.Goobstation.Shared.JoinQueue;
using Prometheus;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Goobstation.Common.CCVar;
using Content.Server._RMC14.LinkAccount;
using Content.Server.Database;
using Content.Goobstation.Common.JoinQueue;

namespace Content.Goobstation.Server.JoinQueue;

/// <summary>
///     Manages new player connections when the server is full and queues them up, granting access when a slot becomes free
/// </summary>
public sealed class JoinQueueManager : IJoinQueueManager
{
    private static readonly Gauge QueueCount = Metrics.CreateGauge(
        "join_queue_total_count",
        "Amount of players in queue.");

    private static readonly Counter QueueBypassCount = Metrics.CreateCounter(
        "join_queue_bypass_count",
        "Amount of players who bypassed queue by privileges.");

    private static readonly Histogram QueueTimings = Metrics.CreateHistogram(
        "join_queue_timings",
        "Timings of players in queue",
        new HistogramConfiguration()
        {
            LabelNames = new[] { "type" },
            Buckets = Histogram.ExponentialBuckets(1, 2, 14),
        });


    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IConnectionManager _connection = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IServerNetManager _net = default!;
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IGameMapManager _gameMapManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private readonly List<ICommonSession> _queue = new();
    private readonly List<ICommonSession> _patronQueue = new();

    /// <summary>
    ///     Rolling window of recent wait times in seconds for estimating queue wait.
    /// </summary>
    private readonly Queue<double> _recentWaitTimes = new();
    private const int MaxWaitTimeSamples = 20;

    /// <summary>
    ///     Holds queue positions for players who disconnected, allowing them to reclaim their spot if they reconnect within the grace period.
    /// </summary>
    private readonly Dictionary<NetUserId, QueueReservation> _reservations = new();

    private bool _isEnabled;
    private bool _patreonIsEnabled = true;

    /// <summary>
    ///     Interval for queue info refreshes
    /// </summary>
    private const float InfoRefreshIntervalSeconds = 30f;
    private float _infoRefreshTimer;

    public int PlayerInQueueCount => _queue.Count + _patronQueue.Count;
    public int ActualPlayersCount => _player.PlayerCount - PlayerInQueueCount;


    public void Initialize()
    {
        _net.RegisterNetMessage<QueueUpdateMessage>();

        _configuration.OnValueChanged(GoobCVars.QueueEnabled, OnQueueCVarChanged, true);
        _configuration.OnValueChanged(GoobCVars.PatreonSkip, OnPatronCvarChanged, true);
        _player.PlayerStatusChanged += OnPlayerStatusChanged;
        _userDb.AddOnFinishLoad(OnPlayerDataLoaded);
    }

    public void Update(float frameTime)
    {
        if (!_isEnabled || PlayerInQueueCount == 0)
            return;

        _infoRefreshTimer += frameTime;
        if (_infoRefreshTimer < InfoRefreshIntervalSeconds)
            return;

        _infoRefreshTimer = 0f;
        SendUpdateMessages();
    }


    private void OnQueueCVarChanged(bool value)
    {
        _isEnabled = value;

        if (!value)
        {
            foreach (var session in _queue)
                session.Channel.Disconnect("Queue was disabled");
            foreach (var session in _patronQueue)
                session.Channel.Disconnect("Queue was disabled");
        }
    }

    private void OnPatronCvarChanged(bool value)
        => _patreonIsEnabled = value;


    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Disconnected)
        {
            var wasInPatronQueue = _patronQueue.Remove(e.Session);
            var wasInQueue = !wasInPatronQueue && _queue.Remove(e.Session);

            if (wasInPatronQueue || wasInQueue)
            {
                var graceSeconds = _configuration.GetCVar(GoobCVars.QueueReconnectGraceSeconds);
                if (graceSeconds > 0)
                {
                    _reservations[e.Session.UserId] = new QueueReservation(
                        DateTime.UtcNow,
                        wasInPatronQueue);
                }

                QueueTimings.WithLabels("Unwaited").Observe((DateTime.UtcNow - e.Session.ConnectedTime).TotalSeconds);
            }

            if (!wasInPatronQueue && !wasInQueue && e.OldStatus != SessionStatus.InGame)
                return;

            ProcessQueue(e.Session.ConnectedTime);
        }
        else if (e.NewStatus == SessionStatus.Connected)
        {
            if (!_isEnabled)
                SendToGame(e.Session);
        }
    }


    private async void OnPlayerDataLoaded(ICommonSession session)
    {
        if (!_isEnabled)
            return;

        var isPrivileged = await _connection.HasPrivilegedJoin(session.UserId);
        var isPatron = _linkAccount.GetPatron(session)?.Tier != null;
        var currentOnline = _player.PlayerCount - 1;
        var haveFreeSlot = currentOnline < _configuration.GetCVar(CCVars.SoftMaxPlayers);
        if (isPrivileged || haveFreeSlot)
        {
            SendToGame(session);
            _reservations.Remove(session.UserId);

            if (isPrivileged && !haveFreeSlot)
                QueueBypassCount.Inc();

            return;
        }

        if (_reservations.Remove(session.UserId, out var reservation))
        {
            var graceSeconds = _configuration.GetCVar(GoobCVars.QueueReconnectGraceSeconds);
            if ((DateTime.UtcNow - reservation.DisconnectTime).TotalSeconds <= graceSeconds)
            {
                if (reservation.WasPatron && _patreonIsEnabled)
                    _patronQueue.Insert(0, session);
                else
                    _queue.Insert(0, session);

                ProcessQueue(session.ConnectedTime);
                return;
            }
        }

        if (isPatron && _patreonIsEnabled)
            _patronQueue.Add(session);
        else
            _queue.Add(session);

        ProcessQueue(session.ConnectedTime);
    }

    private void ProcessQueue(DateTime connectedTime)
    {
        var players = ActualPlayersCount;

        var softMax = _configuration.GetCVar(CCVars.SoftMaxPlayers);

        while (players < softMax && (_patronQueue.Count > 0 || _queue.Count > 0))
        {
            ICommonSession session;
            if (_patronQueue.Count > 0)
            {
                session = _patronQueue[0];
                _patronQueue.RemoveAt(0);
            }
            else
            {
                session = _queue[0];
                _queue.RemoveAt(0);
            }

            RecordWaitTime(session);
            SendToGame(session);
            QueueTimings.WithLabels("Waited").Observe((DateTime.UtcNow - connectedTime).TotalSeconds);
            players++;
        }

        CleanupExpiredReservations();
        SendUpdateMessages();
        QueueCount.Set(_queue.Count + _patronQueue.Count);
    }

    private void RecordWaitTime(ICommonSession session)
    {
        var waitSeconds = (DateTime.UtcNow - session.ConnectedTime).TotalSeconds;
        _recentWaitTimes.Enqueue(waitSeconds);
        while (_recentWaitTimes.Count > MaxWaitTimeSamples)
            _recentWaitTimes.Dequeue();
    }

    private float GetEstimatedWaitForPosition(int position)
    {
        if (_recentWaitTimes.Count == 0)
            return -1f;

        var avg = _recentWaitTimes.Average();
        return (float) (avg * ((double) position / Math.Max(PlayerInQueueCount, 1)));
    }

    private void SendUpdateMessages()
    {
        var totalInQueue = _patronQueue.Count + _queue.Count;
        var currentPosition = 1;

        var mapName = _gameMapManager.GetSelectedMap()?.MapName ?? "Unknown";
        var gameMode = "Unknown";
        var roundDurationMinutes = 0;

        if (_entityManager.System<GameTicker>() is { } ticker)
        {
            var preset = ticker.CurrentPreset ?? ticker.Preset;
            if (preset != null)
                gameMode = Loc.GetString(preset.ModeTitle);

            if (ticker.RunLevel >= GameRunLevel.InRound)
            {
                var elapsed = _gameTiming.CurTime - ticker.RoundStartTimeSpan;
                roundDurationMinutes = (int) elapsed.TotalMinutes;
            }
        }

        var serverPlayerCount = ActualPlayersCount;
        var maxPlayerCount = _configuration.GetCVar(CCVars.SoftMaxPlayers);

        var playerNames = new List<string>(totalInQueue);
        foreach (var session in _patronQueue)
            playerNames.Add(session.Name);
        foreach (var session in _queue)
            playerNames.Add(session.Name);

        for (var i = 0; i < _patronQueue.Count; i++, currentPosition++)
        {
            _patronQueue[i].Channel.SendMessage(new QueueUpdateMessage
            {
                Total = totalInQueue,
                Position = currentPosition,
                IsPatron = true,
                EstimatedWaitSeconds = GetEstimatedWaitForPosition(currentPosition),
                MapName = mapName,
                GameMode = gameMode,
                ServerPlayerCount = serverPlayerCount,
                MaxPlayerCount = maxPlayerCount,
                RoundDurationMinutes = roundDurationMinutes,
                YourName = _patronQueue[i].Name,
                PlayerNames = playerNames,
            });
        }

        for (var i = 0; i < _queue.Count; i++, currentPosition++)
        {
            _queue[i].Channel.SendMessage(new QueueUpdateMessage
            {
                Total = totalInQueue,
                Position = currentPosition,
                IsPatron = false,
                EstimatedWaitSeconds = GetEstimatedWaitForPosition(currentPosition),
                MapName = mapName,
                GameMode = gameMode,
                ServerPlayerCount = serverPlayerCount,
                MaxPlayerCount = maxPlayerCount,
                RoundDurationMinutes = roundDurationMinutes,
                YourName = _queue[i].Name,
                PlayerNames = playerNames,
            });
        }
    }

    private void CleanupExpiredReservations()
    {
        var graceSeconds = _configuration.GetCVar(GoobCVars.QueueReconnectGraceSeconds);
        var now = DateTime.UtcNow;
        var expired = new List<NetUserId>();

        foreach (var (userId, reservation) in _reservations)
        {
            if ((now - reservation.DisconnectTime).TotalSeconds > graceSeconds)
                expired.Add(userId);
        }

        foreach (var userId in expired)
            _reservations.Remove(userId);
    }

    private void SendToGame(ICommonSession session)
    {
        Timer.Spawn(0, () => _player.JoinGame(session));
    }

    private sealed record QueueReservation(DateTime DisconnectTime, bool WasPatron);
}
