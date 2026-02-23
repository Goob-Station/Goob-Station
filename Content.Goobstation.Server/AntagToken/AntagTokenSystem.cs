using System.Linq;
using System.Threading.Tasks;
using Content.Goobstation.Common.AntagToken;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.ServerCurrency;
using Content.Server.Administration.Logs;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.Players.RateLimiting;
using Content.Shared.Database;
using Content.Shared.Players.RateLimiting;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.AntagToken;

public sealed class ServerAntagTokenManager : IAntagTokenManager, IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IServerNetManager _net = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly PlayerRateLimitManager _rateLimitManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;

    private const string RateLimitKey = "AntagToken";

    private ISawmill _sawmill = default!;
    private readonly Dictionary<NetUserId, bool> _activeTokens = new();

    private float _weightMultiplier;
    private int _cooldownRounds;

    void IPostInjectInit.PostInject()
    {
        _sawmill = Logger.GetSawmill("antagtokens");

        _weightMultiplier = _cfg.GetCVar(GoobCVars.AntagTokenWeightMultiplier);
        _cooldownRounds = _cfg.GetCVar(GoobCVars.AntagTokenCooldownRounds);

        _cfg.OnValueChanged(GoobCVars.AntagTokenWeightMultiplier, v => _weightMultiplier = v);
        _cfg.OnValueChanged(GoobCVars.AntagTokenCooldownRounds, v => _cooldownRounds = v);

        _net.RegisterNetMessage<MsgAntagTokenCountUpdate>();
        _net.RegisterNetMessage<MsgAntagTokenCountRequest>(OnTokenCountRequest);
        _net.RegisterNetMessage<MsgAntagTokenActivate>(OnTokenActivate);
        _net.RegisterNetMessage<MsgAntagTokenDeactivate>(OnTokenDeactivate);

        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

        _rateLimitManager.Register(RateLimitKey,
            new RateLimitRegistration(
                GoobCVars.AntagTokenRateLimitPeriod,
                GoobCVars.AntagTokenRateLimitCount,
                null));
    }

    public void ClearActiveTokens()
    {
        _activeTokens.Clear();
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        try
        {
            if (args.NewStatus == SessionStatus.InGame ||
                args.NewStatus == SessionStatus.Connected)
            {
                await SendTokenCount(args.Session);
            }

            if (args.NewStatus == SessionStatus.Disconnected)
            {
                _activeTokens.Remove(args.Session.UserId);
            }
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to handle player status change for antag tokens: {e}");
        }
    }

    private async void OnTokenCountRequest(MsgAntagTokenCountRequest msg)
    {
        try
        {
            if (!_playerManager.TryGetSessionByChannel(msg.MsgChannel, out var session))
                return;

            if (_rateLimitManager.CountAction(session, RateLimitKey) != RateLimitStatus.Allowed)
                return;

            await SendTokenCount(session);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to handle antag token count request: {e}");
        }
    }

    private async void OnTokenActivate(MsgAntagTokenActivate msg)
    {
        try
        {
            if (!_playerManager.TryGetSessionByChannel(msg.MsgChannel, out var session))
                return;

            if (_rateLimitManager.CountAction(session, RateLimitKey) != RateLimitStatus.Allowed)
                return;

            if (session.Status == SessionStatus.InGame)
                return;

            var userId = session.UserId;
            var (count, lastConsumedRound) = await _db.GetAntagTokenState(userId);

            if (count <= 0)
            {
                await SendTokenCount(session);
                return;
            }

            if (_cooldownRounds > 0 && lastConsumedRound > 0)
            {
                var gameTicker = _entityManager.System<GameTicker>();
                var currentRound = gameTicker.RoundId;

                if (currentRound - lastConsumedRound < _cooldownRounds)
                {
                    await SendTokenCount(session);
                    return;
                }
            }

            _activeTokens[userId] = true;
            _adminLog.Add(LogType.AntagToken, LogImpact.Medium, $"Player {session.Name} activated an antag token.");
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to handle antag token activation: {e}");
        }
    }

    private void OnTokenDeactivate(MsgAntagTokenDeactivate msg)
    {
        if (!_playerManager.TryGetSessionByChannel(msg.MsgChannel, out var session))
            return;

        if (_rateLimitManager.CountAction(session, RateLimitKey) != RateLimitStatus.Allowed)
            return;

        if (_activeTokens.Remove(session.UserId))
            _adminLog.Add(LogType.AntagToken, LogImpact.Medium, $"Player {session.Name} deactivated their antag token.");
    }

    public bool HasActiveToken(NetUserId userId)
    {
        return _activeTokens.TryGetValue(userId, out var active) && active;
    }

    public float GetWeightMultiplier()
    {
        return _weightMultiplier;
    }

    public void DeactivateToken(NetUserId userId)
    {
        _activeTokens.Remove(userId);
    }

    public IReadOnlyCollection<NetUserId> GetActiveTokenUsers()
    {
        return _activeTokens.Keys.ToList();
    }

    public async void ConsumeToken(NetUserId userId, int roundId)
    {
        try
        {
            if (!HasActiveToken(userId))
                return;

            _activeTokens.Remove(userId);

            await _db.ConsumeAntagToken(userId, roundId);

            if (_playerManager.TryGetSessionById(userId, out var session))
            {
                _adminLog.Add(LogType.AntagToken, LogImpact.High, $"Player {session.Name} consumed an antag token on round {roundId}.");
                await SendTokenCount(session);
            }
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to consume antag token for {userId}: {e}");
        }
    }

    private async Task SendTokenCount(ICommonSession session)
    {
        var (count, lastConsumedRound) = await _db.GetAntagTokenState(session.UserId);
        var onCooldown = false;

        if (_cooldownRounds > 0 && lastConsumedRound > 0)
        {
            var gameTicker = _entityManager.System<GameTicker>();
            var currentRound = gameTicker.RoundId;
            onCooldown = currentRound - lastConsumedRound < _cooldownRounds;
        }

        session.Channel.SendMessage(new MsgAntagTokenCountUpdate
        {
            TokenCount = count,
            OnCooldown = onCooldown,
        });
    }

    // Client-side stubs
    int IAntagTokenManager.TokenCount => 0;
    bool IAntagTokenManager.OnCooldown => false;
    void IAntagTokenManager.RequestTokenCount() { }
    void IAntagTokenManager.SendActivate() { }
    void IAntagTokenManager.SendDeactivate() { }
}
