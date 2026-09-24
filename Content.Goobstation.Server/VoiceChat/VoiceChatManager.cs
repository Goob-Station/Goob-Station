using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Server.Player;
using Robust.Server.ServerStatus;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.VoiceChat;

public readonly record struct VoiceWebState(string Name, bool InGame, bool CanSpeak, bool Muted, bool PushToTalk, bool Broadcasting, bool VoiceChanger, string? Radio);

public sealed class VoiceChatManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IServerNetManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IStatusHost _statusHost = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private const string PagePath = "/voice";
    private const string ConfigPlaceholder = "/*VOICE_CONFIG*/";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly ConcurrentDictionary<string, NetUserId> _tokens = new();
    private readonly Dictionary<NetUserId, string> _userTokens = new();
    private readonly HashSet<NetUserId> _hearSelf = new();
    private readonly HashSet<NetUserId> _notReceiving = new();
    private readonly Dictionary<string, (byte[] Data, string ContentType)> _files = new();

    private ISawmill _sawmill = default!;
    private VoiceWebServer? _server;
    private string _indexTemplate = string.Empty;
    private volatile bool _enabled;
    private volatile string _webSocketUrl = string.Empty;
    private volatile int _webSocketPort;

    public void Initialize()
    {
        _sawmill = _logManager.GetSawmill("voice");

        _net.RegisterNetMessage<MsgVoiceFrame>();
        _net.RegisterNetMessage<MsgVoiceLink>();
        _net.RegisterNetMessage<MsgVoiceLinkRequest>(OnLinkRequest);
        _net.RegisterNetMessage<MsgVoiceSettings>(OnSettings);
        _net.RegisterNetMessage<MsgVoiceStatus>();
        _net.RegisterNetMessage<MsgVoiceSpeakerInfo>();
        _net.Disconnect += OnDisconnect;

        LoadWebFiles();
        _statusHost.AddHandler(HandleHttpRequestAsync);

        _cfg.OnValueChanged(GoobCVars.VoiceChatWebSocketUrl, url => _webSocketUrl = url.Trim(), true);
        _cfg.OnValueChanged(GoobCVars.VoiceChatTrustedProxies, _ => ApplyLimits());
        _cfg.OnValueChanged(GoobCVars.VoiceChatMaxConnectionsPerIp, _ => ApplyLimits());
        _cfg.OnValueChanged(GoobCVars.VoiceChatWebSocketBind, _ => RestartServer());
        _cfg.OnValueChanged(GoobCVars.VoiceChatEnabled, OnEnabledChanged, true);
    }

    public void Shutdown()
    {
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatEnabled, OnEnabledChanged);
        _net.Disconnect -= OnDisconnect;
        StopServer();
    }

    public bool TryDequeueEvent(out VoiceWebEvent ev)
    {
        if (_server != null)
            return _server.TryDequeue(out ev);

        ev = default!;
        return false;
    }

    public bool IsWebConnected(NetUserId user)
    {
        return _server?.IsConnected(user) ?? false;
    }

    public bool HearsSelf(NetUserId user)
    {
        return _hearSelf.Contains(user);
    }

    public bool Receives(NetUserId user)
    {
        return !_notReceiving.Contains(user);
    }

    public void SendState(NetUserId user, VoiceWebState state)
    {
        _server?.Send(user, JsonSerializer.Serialize(new
        {
            t = "state",
            name = state.Name,
            inGame = state.InGame,
            canSpeak = state.CanSpeak,
            muted = state.Muted,
            pushToTalk = state.PushToTalk,
            broadcasting = state.Broadcasting,
            voiceChanger = state.VoiceChanger,
            radio = state.Radio,
        }, JsonOptions));
    }

    public void SendStatus(NetUserId user)
    {
        if (_player.TryGetSessionById(user, out var session))
            SendStatus(session.Channel);
    }

    private void SendStatus(INetChannel channel)
    {
        _net.ServerSendMessage(new MsgVoiceStatus { Connected = IsWebConnected(channel.UserId) }, channel);
    }

    public void SetEffect(NetUserId user, VoiceEffectSettings settings)
    {
        _server?.SetEffect(user, settings);
    }

    private void OnEnabledChanged(bool enabled)
    {
        _enabled = enabled;
        if (enabled)
            StartServer();
        else
            StopServer();
    }

    private void RestartServer()
    {
        if (!_enabled)
            return;

        StopServer();
        StartServer();
    }

    private void StartServer()
    {
        if (_server != null)
            return;

        var bind = _cfg.GetCVar(GoobCVars.VoiceChatWebSocketBind);
        if (!TryParseEndpoint(bind, out var endpoint))
        {
            _sawmill.Error($"Invalid voice.ws_bind value '{bind}', expected host:port.");
            return;
        }

        try
        {
            var server = new VoiceWebServer(endpoint, ValidateToken, _sawmill);
            server.Start();
            _server = server;
            ApplyLimits();
            _webSocketPort = server.LocalEndpoint.Port;
            _sawmill.Info($"Voice chat WebSocket listening on {server.LocalEndpoint}.");
        }
        catch (SocketException e)
        {
            _sawmill.Error($"Failed to start voice chat WebSocket on {endpoint}: {e.Message}");
        }
    }

    private void ApplyLimits()
    {
        if (_server == null)
            return;

        _server.MaxConnectionsPerAddress = _cfg.GetCVar(GoobCVars.VoiceChatMaxConnectionsPerIp);

        var proxies = new List<IPAddress>();
        foreach (var entry in _cfg.GetCVar(GoobCVars.VoiceChatTrustedProxies).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (IPAddress.TryParse(entry, out var address))
                proxies.Add(address);
            else
                _sawmill.Warning($"Ignoring invalid voice.trusted_proxies entry '{entry}'.");
        }

        _server.SetTrustedProxies(proxies);
    }

    private void StopServer()
    {
        _server?.Dispose();
        _server = null;
    }

    private NetUserId? ValidateToken(string token)
    {
        return _tokens.TryGetValue(token, out var user) ? user : null;
    }

    private void OnLinkRequest(MsgVoiceLinkRequest message)
    {
        if (!_enabled || _server == null)
            return;

        var user = message.MsgChannel.UserId;
        if (_userTokens.Remove(user, out var previous))
            _tokens.TryRemove(previous, out _);

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        _tokens[token] = user;
        _userTokens[user] = token;

        _net.ServerSendMessage(new MsgVoiceLink
        {
            Url = GetPublicUrl(),
            Token = token,
            StatusPort = GetStatusPort(),
        }, message.MsgChannel);
    }

    private void OnSettings(MsgVoiceSettings message)
    {
        var user = message.MsgChannel.UserId;

        if (message.HearSelf)
            _hearSelf.Add(user);
        else
            _hearSelf.Remove(user);

        if (message.Receive)
            _notReceiving.Remove(user);
        else
            _notReceiving.Add(user);

        SendStatus(message.MsgChannel);
    }

    private void OnDisconnect(object? sender, NetDisconnectedArgs args)
    {
        _hearSelf.Remove(args.Channel.UserId);
        _notReceiving.Remove(args.Channel.UserId);
    }

    private string GetPublicUrl()
    {
        var configured = _cfg.GetCVar(GoobCVars.VoiceChatPublicUrl).Trim();
        if (configured.Length > 0)
            return configured.EndsWith('/') ? configured : configured + "/";

        return TryConvertHubUrl(_cfg.GetCVar(CVars.HubServerUrl), out var url) ? url : string.Empty;
    }

    private int GetStatusPort()
    {
        var bind = _cfg.GetCVar(CVars.StatusBind);
        var separator = bind.LastIndexOf(':');
        if (separator >= 0 && int.TryParse(bind[(separator + 1)..], out var port))
            return port;

        return _cfg.GetCVar(CVars.NetPort);
    }

    private static bool TryConvertHubUrl(string hubUrl, out string url)
    {
        url = string.Empty;
        if (!Uri.TryCreate(hubUrl.Trim(), UriKind.Absolute, out var uri))
            return false;

        var (scheme, defaultPort) = uri.Scheme switch
        {
            "ss14s" => ("https", 443),
            "https" => ("https", 443),
            "ss14" => ("http", 1212),
            "http" => ("http", 80),
            _ => (string.Empty, 0),
        };

        if (scheme.Length == 0)
            return false;

        var port = uri.IsDefaultPort || uri.Port < 0 ? defaultPort : uri.Port;
        url = new UriBuilder(scheme, uri.Host, port, uri.AbsolutePath.TrimEnd('/') + PagePath + "/").Uri.ToString();
        return true;
    }

    private static bool TryParseEndpoint(string value, out IPEndPoint endpoint)
    {
        endpoint = default!;
        var separator = value.LastIndexOf(':');
        if (separator <= 0 || !int.TryParse(value[(separator + 1)..], out var port) || port is < 0 or > 65535)
            return false;

        var host = value[..separator].Trim('[', ']');
        IPAddress? address = host switch
        {
            "*" or "0.0.0.0" => IPAddress.Any,
            "localhost" => IPAddress.Loopback,
            _ => IPAddress.TryParse(host, out var parsed) ? parsed : null,
        };

        if (address == null)
            return false;

        endpoint = new IPEndPoint(address, port);
        return true;
    }

    private void LoadWebFiles()
    {
        var assembly = typeof(VoiceChatManager).Assembly;
        foreach (var (name, contentType) in new[]
                 {
                     ("index.html", "text/html; charset=utf-8"),
                     ("voice.js", "text/javascript; charset=utf-8"),
                     ("voice-worklet.js", "text/javascript; charset=utf-8"),
                     ("voice-worker.js", "text/javascript; charset=utf-8"),
                     ("noto-sans-regular.woff2", "font/woff2"),
                     ("noto-sans-bold.woff2", "font/woff2"),
                     ("boxfont-round.woff2", "font/woff2"),
                 })
        {
            using var stream = assembly.GetManifestResourceStream($"VoiceChat.Web.{name}");
            if (stream == null)
            {
                _sawmill.Error($"Missing embedded voice chat web file {name}.");
                continue;
            }

            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            _files[name] = (memory.ToArray(), contentType);
        }

        if (_files.TryGetValue("index.html", out var index))
            _indexTemplate = Encoding.UTF8.GetString(index.Data);
    }

    private async Task<bool> HandleHttpRequestAsync(IStatusHandlerContext context)
    {
        var path = context.Url.AbsolutePath;
        if (!path.StartsWith(PagePath, StringComparison.OrdinalIgnoreCase) ||
            path.Length > PagePath.Length && path[PagePath.Length] != '/')
        {
            return false;
        }

        if (!context.IsGetLike)
        {
            await context.RespondErrorAsync(HttpStatusCode.MethodNotAllowed);
            return true;
        }

        if (!_enabled)
        {
            await context.RespondAsync("Voice chat is not enabled on this server.", HttpStatusCode.NotFound);
            return true;
        }

        if (path.Length == PagePath.Length)
        {
            context.ResponseHeaders["Location"] = "voice/";
            await context.RespondAsync(string.Empty, HttpStatusCode.MovedPermanently);
            return true;
        }

        var file = path[(PagePath.Length + 1)..];
        if (file.Length == 0)
            file = "index.html";

        context.ResponseHeaders["Cache-Control"] = "no-cache";
        context.ResponseHeaders["X-Content-Type-Options"] = "nosniff";
        context.ResponseHeaders["Referrer-Policy"] = "no-referrer";

        if (file == "index.html")
        {
            var config = JsonSerializer.Serialize(new { wsUrl = _webSocketUrl, wsPort = _webSocketPort });
            await context.RespondAsync(_indexTemplate.Replace(ConfigPlaceholder, config), HttpStatusCode.OK, "text/html; charset=utf-8");
            return true;
        }

        if (_files.TryGetValue(file, out var entry))
        {
            await context.RespondAsync(entry.Data, HttpStatusCode.OK, entry.ContentType);
            return true;
        }

        if (file == "ws")
        {
            await context.RespondAsync("The voice chat WebSocket is not proxied here. Forward /voice/ws to voice.ws_bind in your reverse proxy.", HttpStatusCode.NotFound);
            return true;
        }

        await context.RespondAsync("Not Found", HttpStatusCode.NotFound);
        return true;
    }
}
