using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Robust.Server.Player;
using Robust.Server.ServerStatus;
using Robust.Shared.ContentPack;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Twitch;

public sealed record TwitchChannelPairing(
    string ChannelId,
    string ChannelLogin,
    NetUserId Ss14UserId,
    string Ss14Username,
    DateTimeOffset LinkedAt);

public sealed record TwitchPairingChangedEvent(string ChannelId, string ChannelLogin, bool Paired);

public sealed class TwitchPairingSystem : EntitySystem
{
    private static readonly ResPath PairingsPath = new("/twitch_pairings.json");
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IResourceManager _resources = default!;
    [Dependency] private readonly ITwitchApiManager _twitchApi = default!;

    private readonly Dictionary<string, PendingPairing> _pending = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TwitchChannelPairing> _pairings = new(StringComparer.Ordinal);

    public IEnumerable<TwitchChannelPairing> Pairings => _pairings.Values;

    public override void Initialize()
    {
        base.Initialize();
        Load();
        _twitchApi.RegisterRoute(HttpMethod.Get, "/pairing/status", HandleStatus, TwitchApiAccess.ExtensionJwt);
        _twitchApi.RegisterRoute(HttpMethod.Post, "/pairing/start", HandleStart, TwitchApiAccess.ExtensionJwt);
    }

    public bool TryGetPairing(string channelId, out TwitchChannelPairing pairing)
    {
        return _pairings.TryGetValue(channelId, out pairing!);
    }

    public bool TryGetTargetSession(string channelId, out ICommonSession session, out TwitchChannelPairing pairing)
    {
        session = default!;
        if (!TryGetPairing(channelId, out pairing) ||
            !_players.TryGetSessionById(pairing.Ss14UserId, out var found))
        {
            return false;
        }

        session = found;
        return true;
    }

    public bool TryComplete(string code, string username, out TwitchChannelPairing? pairing, out string error)
    {
        PrunePending();
        pairing = null;
        code = NormalizeCode(code);
        if (!_pending.TryGetValue(code, out var pending))
        {
            error = "That Twitch pairing code is invalid or expired.";
            return false;
        }

        if (!_players.TryGetUserId(username, out var userId))
        {
            error = "That SS14 player must be connected before they can be linked.";
            return false;
        }

        _pending.Remove(code);
        if (_pairings.Remove(pending.ChannelId, out var channelPairing))
            RaiseLocalEvent(new TwitchPairingChangedEvent(channelPairing.ChannelId, channelPairing.ChannelLogin, false));

        foreach (var existing in _pairings.Values.Where(item => item.Ss14UserId == userId).ToArray())
        {
            _pairings.Remove(existing.ChannelId);
            RaiseLocalEvent(new TwitchPairingChangedEvent(existing.ChannelId, existing.ChannelLogin, false));
        }

        pairing = new TwitchChannelPairing(
            pending.ChannelId,
            pending.ChannelLogin,
            userId,
            username,
            DateTimeOffset.UtcNow);
        _pairings[pending.ChannelId] = pairing;
        Save();
        RaiseLocalEvent(new TwitchPairingChangedEvent(pairing.ChannelId, pairing.ChannelLogin, true));
        error = string.Empty;
        return true;
    }

    public bool TryUnlink(string channel, out TwitchChannelPairing? pairing)
    {
        pairing = _pairings.Values.FirstOrDefault(item =>
            string.Equals(item.ChannelId, channel, StringComparison.Ordinal) ||
            string.Equals(item.ChannelLogin, channel, StringComparison.OrdinalIgnoreCase));
        if (pairing == null)
            return false;

        _pairings.Remove(pairing.ChannelId);
        Save();
        RaiseLocalEvent(new TwitchPairingChangedEvent(pairing.ChannelId, pairing.ChannelLogin, false));
        return true;
    }

    private async Task HandleStart(IStatusHandlerContext context)
    {
        if (!_twitchApi.TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        if (identity.Role != TwitchExtensionRole.Broadcaster)
        {
            await RespondError(context, HttpStatusCode.Forbidden, "broadcaster_required", "Only the broadcaster can create a pairing code.");
            return;
        }

        var request = await _twitchApi.ReadJsonAsync<StartPairingRequest>(context);
        var channelLogin = NormalizeChannelLogin(request?.ChannelLogin);
        if (channelLogin == null)
        {
            await RespondError(context, HttpStatusCode.BadRequest, "channel_login_invalid", "A valid Twitch channel login is required.");
            return;
        }

        var response = await _twitchApi.RunOnMainThread(() => StartPairing(identity.ChannelId, channelLogin));
        await context.RespondJsonAsync(response);
    }

    private async Task HandleStatus(IStatusHandlerContext context)
    {
        if (!_twitchApi.TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("An authenticated Twitch identity was not available.");

        var response = await _twitchApi.RunOnMainThread(() =>
        {
            if (!TryGetPairing(identity.ChannelId, out var pairing))
                return new PairingStatusResponse(false, identity.ChannelId, null, null);

            return new PairingStatusResponse(true, pairing.ChannelId, pairing.ChannelLogin, pairing.Ss14Username);
        });
        await context.RespondJsonAsync(response);
    }

    private StartPairingResponse StartPairing(string channelId, string channelLogin)
    {
        PrunePending();
        foreach (var (existingCode, pending) in _pending.ToArray())
        {
            if (pending.ChannelId == channelId)
                _pending.Remove(existingCode);
        }

        string code;
        do
        {
            var characters = new char[6];
            for (var i = 0; i < characters.Length; i++)
                characters[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
            code = new string(characters);
        } while (_pending.ContainsKey(code));

        var expiresAt = DateTimeOffset.UtcNow + CodeLifetime;
        _pending[code] = new PendingPairing(channelId, channelLogin, expiresAt);
        return new StartPairingResponse(code, expiresAt);
    }

    private void PrunePending()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (code, pending) in _pending.ToArray())
        {
            if (pending.ExpiresAt <= now)
                _pending.Remove(code);
        }
    }

    private void Load()
    {
        if (!_resources.UserData.TryReadAllText(PairingsPath, out var json))
            return;

        try
        {
            var stored = JsonSerializer.Deserialize<StoredPairing[]>(json) ?? [];
            foreach (var item in stored)
            {
                var login = NormalizeChannelLogin(item.ChannelLogin);
                if (string.IsNullOrWhiteSpace(item.ChannelId) || login == null || item.Ss14UserId == Guid.Empty)
                    continue;

                _pairings[item.ChannelId] = new TwitchChannelPairing(
                    item.ChannelId,
                    login,
                    new NetUserId(item.Ss14UserId),
                    item.Ss14Username,
                    item.LinkedAt);
            }
        }
        catch (JsonException exception)
        {
            Log.Error($"Could not load Twitch pairings: {exception.Message}");
        }
    }

    private void Save()
    {
        var stored = _pairings.Values
            .OrderBy(pairing => pairing.ChannelLogin, StringComparer.OrdinalIgnoreCase)
            .Select(pairing => new StoredPairing(
                pairing.ChannelId,
                pairing.ChannelLogin,
                pairing.Ss14UserId.UserId,
                pairing.Ss14Username,
                pairing.LinkedAt))
            .ToArray();
        _resources.UserData.WriteAllText(PairingsPath, JsonSerializer.Serialize(stored));
    }

    private static string NormalizeCode(string code)
    {
        return new string(code.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }

    private static string? NormalizeChannelLogin(string? login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return null;

        login = login.Trim().TrimStart('#').ToLowerInvariant();
        return login.Length is >= 1 and <= 25 && login.All(character => char.IsAsciiLetterOrDigit(character) || character == '_')
            ? login
            : null;
    }

    private static Task RespondError(IStatusHandlerContext context, HttpStatusCode status, string error, string message)
    {
        return context.RespondJsonAsync(new ApiError(error, message), status);
    }

    private sealed record PendingPairing(string ChannelId, string ChannelLogin, DateTimeOffset ExpiresAt);

    private sealed record StoredPairing(
        string ChannelId,
        string ChannelLogin,
        Guid Ss14UserId,
        string Ss14Username,
        DateTimeOffset LinkedAt);

    private sealed record StartPairingRequest(
        [property: JsonPropertyName("channelLogin")] string? ChannelLogin);

    private sealed record StartPairingResponse(
        [property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("expiresAt")] DateTimeOffset ExpiresAt);

    private sealed record PairingStatusResponse(
        [property: JsonPropertyName("paired")] bool Paired,
        [property: JsonPropertyName("channelId")] string ChannelId,
        [property: JsonPropertyName("channelLogin")] string? ChannelLogin,
        [property: JsonPropertyName("ss14Username")] string? Ss14Username);

    private sealed record ApiError(
        [property: JsonPropertyName("error")] string Error,
        [property: JsonPropertyName("message")] string Message);
}
