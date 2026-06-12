using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.RecoveryPassword;
using Content.Server.Database;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.RecoveryPassword;

public sealed class RecoveryPasswordManager : IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private const int CurrentAlgoVersion = 1; // if we ever change for some reason
    private const int Iterations = 210_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;
    private const int MaxPasswordLength = 128;

    private static readonly TimeSpan MinRequestInterval = TimeSpan.FromSeconds(0.5);

    private ISawmill _sawmill = default!;

    private readonly Dictionary<NetUserId, bool> _hasPassword = [];
    private readonly Dictionary<NetUserId, TimeSpan> _lastRequest = [];

    private bool Enabled => _cfg.GetCVar(GoobCVars.RecoveryPasswordEnabled);
    private int MinLength => _cfg.GetCVar(GoobCVars.RecoveryPasswordMinLength);

    private async Task LoadData(ICommonSession player, CancellationToken cancel)
    {
        var has = await _db.HasRecoveryPassword(player.UserId, cancel);
        cancel.ThrowIfCancellationRequested();
        _hasPassword[player.UserId] = has;
    }

    private void FinishLoad(ICommonSession player)
    {
        SendStatus(player.Channel, player.UserId);
    }

    private void ClientDisconnected(ICommonSession player)
    {
        _hasPassword.Remove(player.UserId);
        _lastRequest.Remove(player.UserId);
    }

    private void SendStatus(INetChannel channel, NetUserId user)
    {
        var msg = new MsgRecoveryPasswordStatus
        {
            HasPassword = _hasPassword.GetValueOrDefault(user),
            Enabled = Enabled,
            MinLength = MinLength,
        };
        _net.ServerSendMessage(msg, channel);
    }

    private async void OnSetRequest(MsgSetRecoveryPassword message)
    {
        var channel = message.MsgChannel;
        var user = channel.UserId;

        var now = _timing.RealTime;
        if (_lastRequest.TryGetValue(user, out var last) && last + MinRequestInterval > now)
            return;
        _lastRequest[user] = now;

        var result = await TrySetPassword(user, channel.UserName, message.Password);

        _net.ServerSendMessage(new MsgRecoveryPasswordResult { Result = result }, channel);
        SendStatus(channel, user);
    }

    private async Task<RecoveryPasswordSetResult> TrySetPassword(NetUserId user, string username, string password)
    {
        try
        {
            if (!Enabled)
                return RecoveryPasswordSetResult.Disabled;

            if (_hasPassword.GetValueOrDefault(user))
                return RecoveryPasswordSetResult.AlreadySet;

            if (password.Length < MinLength)
                return RecoveryPasswordSetResult.TooShort;
            if (password.Length > MaxPasswordLength)
                return RecoveryPasswordSetResult.TooLong;

            var salt = RandomNumberGenerator.GetBytes(SaltBytes);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);

            var stored = await _db.SetRecoveryPassword(user, username, salt, hash, Iterations, CurrentAlgoVersion);
            if (!stored)
            {
                _hasPassword[user] = true;
                return RecoveryPasswordSetResult.AlreadySet;
            }

            _hasPassword[user] = true;

            _sawmill.Info($"Player {username} ({user}) set an account recovery password.");
            return RecoveryPasswordSetResult.Success;
        }
        catch (Exception e)
        {
            _sawmill.Error($"Error setting recovery password for {user}: {e}");
            return RecoveryPasswordSetResult.Error;
        }
    }

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("recovery_password");

        _net.RegisterNetMessage<MsgSetRecoveryPassword>(OnSetRequest);
        _net.RegisterNetMessage<MsgRecoveryPasswordStatus>();
        _net.RegisterNetMessage<MsgRecoveryPasswordResult>();

        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnFinishLoad(FinishLoad);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
    }
}
