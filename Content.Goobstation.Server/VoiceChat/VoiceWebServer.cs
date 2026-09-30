using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.VoiceChat;

public abstract record VoiceWebEvent(NetUserId User);

public sealed record VoiceWebConnectionChanged(NetUserId User) : VoiceWebEvent(User);

public sealed record VoiceWebFrame(NetUserId User, ushort Sequence, byte Flags, byte[] Payload, byte[] Raw) : VoiceWebEvent(User);

public sealed record VoiceWebEffectSelected(NetUserId User, VoiceEffect Effect) : VoiceWebEvent(User);

public sealed class VoiceWebServer : IDisposable
{
    public const WebSocketCloseStatus CloseInvalidToken = (WebSocketCloseStatus) 4001;
    public const WebSocketCloseStatus CloseReplaced = (WebSocketCloseStatus) 4002;
    public const WebSocketCloseStatus CloseShutdown = (WebSocketCloseStatus) 4003;

    private const int FrameMessageBytes = 3 + VoiceCodec.FrameBytes;
    private const int MaxHeaderBytes = 8192;
    private const int MaxMessageBytes = 4096;
    private const int MaxConnections = 1024;
    private const int MaxQueuedFrames = 4096;
    private const double FrameBurst = 30;
    private const double FramesPerSecond = 55;

    private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan KeepAliveTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CloseGrace = TimeSpan.FromSeconds(3);

    private readonly ISawmill _sawmill;
    private readonly Func<string, NetUserId?> _validateToken;
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly ConcurrentDictionary<NetUserId, Connection> _connections = new();
    private readonly ConcurrentQueue<VoiceWebEvent> _events = new();
    private readonly ConcurrentDictionary<NetUserId, VoiceEffectSettings> _userEffects = new();
    private readonly Dictionary<IPAddress, int> _addressCounts = new();
    private readonly object _addressLock = new();
    private int _connectionCount;
    private int _queuedFrames;
    private volatile bool _disposed;
    private volatile int _maxConnectionsPerAddress = 6;
    private volatile HashSet<IPAddress> _trustedProxies = new();

    public VoiceWebServer(IPEndPoint endpoint, Func<string, NetUserId?> validateToken, ISawmill sawmill)
    {
        _sawmill = sawmill;
        _validateToken = validateToken;
        _listener = new TcpListener(endpoint);
    }

    public IPEndPoint LocalEndpoint => (IPEndPoint) _listener.LocalEndpoint;

    public int MaxConnectionsPerAddress
    {
        set => _maxConnectionsPerAddress = Math.Max(1, value);
    }

    public void SetTrustedProxies(IEnumerable<IPAddress> proxies)
    {
        _trustedProxies = new HashSet<IPAddress>(proxies.Select(Normalize));
    }

    public void Start()
    {
        _listener.Start();
        _ = Task.Run(AcceptLoopAsync);
    }

    public bool TryDequeue(out VoiceWebEvent ev)
    {
        if (!_events.TryDequeue(out ev!))
            return false;

        if (ev is VoiceWebFrame)
            Interlocked.Decrement(ref _queuedFrames);

        return true;
    }

    public bool IsConnected(NetUserId user)
    {
        return _connections.ContainsKey(user);
    }

    public void SetEffect(NetUserId user, VoiceEffectSettings settings)
    {
        if (settings == default)
            _userEffects.TryRemove(user, out _);
        else
            _userEffects[user] = settings;
    }

    public void Send(NetUserId user, string json)
    {
        if (_connections.TryGetValue(user, out var connection))
            connection.Enqueue(new Outgoing(json, null, null));
    }

    public void Dispose()
    {
        _disposed = true;

        foreach (var connection in _connections.Values)
        {
            connection.Close(CloseShutdown, "Voice chat is shutting down.");
        }

        _cts.CancelAfter(CloseGrace);

        try
        {
            _listener.Stop();
        }
        catch (SocketException)
        {
        }
    }

    private async Task AcceptLoopAsync()
    {
        while (!_disposed)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(_cts.Token);
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException or InvalidOperationException)
            {
                return;
            }
            catch (SocketException e)
            {
                if (_disposed)
                    return;

                _sawmill.Warning($"Voice WebSocket accept failed: {e.Message}");
                continue;
            }

            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            if (Interlocked.Increment(ref _connectionCount) > MaxConnections)
            {
                Interlocked.Decrement(ref _connectionCount);
                return;
            }

            IPAddress? reservedAddress = null;
            try
            {
                client.NoDelay = true;
                var stream = client.GetStream();
                var remote = ((IPEndPoint) client.Client.RemoteEndPoint!).Address;

                HttpRequestHead? request;
                using (var handshakeCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token))
                {
                    handshakeCts.CancelAfter(HandshakeTimeout);
                    request = await ReadRequestAsync(stream, handshakeCts.Token);
                }

                if (request == null)
                    return;

                var address = ResolveAddress(remote, request.Headers);
                if (!TryReserveAddress(address))
                {
                    await WriteResponseAsync(stream, "429 Too Many Requests", "Too many voice connections from this address.");
                    return;
                }

                reservedAddress = address;

                if (!TryGetWebSocketKey(request, out var key))
                {
                    await WriteResponseAsync(stream, "426 Upgrade Required", "This endpoint only accepts WebSocket connections from the voice chat page.");
                    return;
                }

                await WriteHandshakeAsync(stream, key);

                using var socket = WebSocket.CreateFromStream(stream, new WebSocketCreationOptions
                {
                    IsServer = true,
                    KeepAliveInterval = KeepAliveInterval,
                    KeepAliveTimeout = KeepAliveTimeout,
                });

                await RunConnectionAsync(socket);
            }
            catch (Exception e) when (e is IOException or SocketException or WebSocketException or OperationCanceledException or ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                _sawmill.Error($"Unhandled error in voice WebSocket connection: {e}");
            }
            finally
            {
                if (reservedAddress != null)
                    ReleaseAddress(reservedAddress);

                Interlocked.Decrement(ref _connectionCount);
            }
        }
    }

    private async Task RunConnectionAsync(WebSocket socket)
    {
        using var connection = new Connection(socket, _cts.Token);
        var buffer = new byte[MaxMessageBytes];

        NetUserId? user;
        using (var authCts = CancellationTokenSource.CreateLinkedTokenSource(connection.Token))
        {
            authCts.CancelAfter(HandshakeTimeout);
            user = await AuthenticateAsync(socket, buffer, authCts.Token);
        }

        if (user == null)
        {
            await CloseQuietlyAsync(socket, CloseInvalidToken, "This voice chat link is invalid or has expired.");
            return;
        }

        connection.User = user.Value;

        if (_connections.TryGetValue(user.Value, out var previous))
            previous.Close(CloseReplaced, "Voice chat was opened in another tab.");

        _connections[user.Value] = connection;
        connection.Enqueue(new Outgoing("{\"t\":\"auth_ok\"}", null, null));
        _events.Enqueue(new VoiceWebConnectionChanged(user.Value));

        var sendTask = SendLoopAsync(connection);
        try
        {
            await ReceiveLoopAsync(connection, buffer);
        }
        finally
        {
            connection.Cancel();

            if (_connections.TryRemove(new KeyValuePair<NetUserId, Connection>(user.Value, connection)))
                _events.Enqueue(new VoiceWebConnectionChanged(user.Value));

            try
            {
                await sendTask;
            }
            catch (Exception e) when (e is OperationCanceledException or WebSocketException or IOException or ObjectDisposedException)
            {
            }
        }
    }

    private async Task<NetUserId?> AuthenticateAsync(WebSocket socket, byte[] buffer, CancellationToken ct)
    {
        var (type, count) = await ReceiveMessageAsync(socket, buffer, ct);
        if (type != WebSocketMessageType.Text)
            return null;

        try
        {
            using var doc = JsonDocument.Parse(buffer.AsMemory(0, count));
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("t", out var kind) || kind.GetString() != "auth" ||
                !root.TryGetProperty("token", out var token) || token.GetString() is not { } tokenString)
            {
                return null;
            }

            return _validateToken(tokenString);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task ReceiveLoopAsync(Connection connection, byte[] buffer)
    {
        var socket = connection.Socket;
        while (!connection.Token.IsCancellationRequested)
        {
            var (type, count) = await ReceiveMessageAsync(socket, buffer, connection.Token);

            switch (type)
            {
                case WebSocketMessageType.Close:
                    if (socket.State == WebSocketState.CloseReceived)
                        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, connection.Token);
                    return;
                case null:
                    await CloseQuietlyAsync(socket, WebSocketCloseStatus.MessageTooBig, "Message too large.");
                    return;
                case WebSocketMessageType.Binary:
                    HandleFrame(connection, buffer.AsSpan(0, count));
                    break;
                case WebSocketMessageType.Text:
                    HandleText(connection, buffer, count);
                    break;
            }
        }
    }

    private void HandleFrame(Connection connection, ReadOnlySpan<byte> message)
    {
        if (message.Length != FrameMessageBytes)
            return;

        var payload = message[3..];
        if (!VoiceCodec.IsValidFrame(payload) || !connection.TryConsumeFrame())
            return;

        if (Interlocked.Increment(ref _queuedFrames) > MaxQueuedFrames)
        {
            Interlocked.Decrement(ref _queuedFrames);
            return;
        }

        var effect = _userEffects.GetValueOrDefault(connection.User);
        var raw = payload.ToArray();
        var data = connection.ApplyEffect(payload, effect);

        var sequence = (ushort) (message[0] | (message[1] << 8));
        _events.Enqueue(new VoiceWebFrame(connection.User, sequence, message[2], data, raw));
    }

    private void HandleText(Connection connection, byte[] buffer, int count)
    {
        try
        {
            using var doc = JsonDocument.Parse(buffer.AsMemory(0, count));
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("t", out var kind) || kind.GetString() != "effect" ||
                !root.TryGetProperty("effect", out var value) ||
                !Enum.TryParse<VoiceEffect>(value.GetString(), true, out var effect) ||
                !Enum.IsDefined(effect))
            {
                return;
            }

            _events.Enqueue(new VoiceWebEffectSelected(connection.User, effect));
        }
        catch (JsonException)
        {
        }
    }

    private static async Task SendLoopAsync(Connection connection)
    {
        var socket = connection.Socket;
        await foreach (var item in connection.Outbox.Reader.ReadAllAsync(connection.Token))
        {
            if (item.Close is { } status)
            {
                connection.CancelAfter(CloseGrace);
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                    await socket.CloseOutputAsync(status, item.Reason, connection.Token);
                return;
            }

            if (item.Text != null && socket.State == WebSocketState.Open)
                await socket.SendAsync(Encoding.UTF8.GetBytes(item.Text), WebSocketMessageType.Text, true, connection.Token);
        }
    }

    private static async Task<(WebSocketMessageType? Type, int Count)> ReceiveMessageAsync(WebSocket socket, byte[] buffer, CancellationToken ct)
    {
        var count = 0;
        while (true)
        {
            if (count >= buffer.Length)
                return (null, count);

            var result = await socket.ReceiveAsync(buffer.AsMemory(count), ct);
            if (result.MessageType == WebSocketMessageType.Close)
                return (WebSocketMessageType.Close, 0);

            count += result.Count;
            if (result.EndOfMessage)
                return (result.MessageType, count);
        }
    }

    private static async Task CloseQuietlyAsync(WebSocket socket, WebSocketCloseStatus status, string reason)
    {
        try
        {
            using var cts = new CancellationTokenSource(CloseGrace);
            if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await socket.CloseOutputAsync(status, reason, cts.Token);
        }
        catch (Exception e) when (e is OperationCanceledException or WebSocketException or IOException or ObjectDisposedException)
        {
        }
    }

    private static async Task<HttpRequestHead?> ReadRequestAsync(NetworkStream stream, CancellationToken ct)
    {
        var buffer = new byte[MaxHeaderBytes];
        var length = 0;
        while (length < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(length), ct);
            if (read == 0)
                return null;

            length += read;
            var end = buffer.AsSpan(0, length).IndexOf("\r\n\r\n"u8);
            if (end < 0)
                continue;

            if (end + 4 != length)
                return null;

            return HttpRequestHead.Parse(Encoding.ASCII.GetString(buffer, 0, end));
        }

        return null;
    }

    private static bool TryGetWebSocketKey(HttpRequestHead request, out string key)
    {
        key = string.Empty;

        if (request.Method != "GET" ||
            !request.HasToken("Upgrade", "websocket") ||
            !request.HasToken("Connection", "upgrade") ||
            request.GetHeader("Sec-WebSocket-Version") != "13")
        {
            return false;
        }

        var value = request.GetHeader("Sec-WebSocket-Key")?.Trim();
        if (string.IsNullOrEmpty(value) || value.Length != 24)
            return false;

        key = value;
        return true;
    }

    private static async Task WriteHandshakeAsync(NetworkStream stream, string key)
    {
        var accept = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        var response =
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            $"Sec-WebSocket-Accept: {accept}\r\n\r\n";

        await stream.WriteAsync(Encoding.ASCII.GetBytes(response));
    }

    private static async Task WriteResponseAsync(NetworkStream stream, string status, string body)
    {
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var head =
            $"HTTP/1.1 {status}\r\n" +
            "Connection: close\r\n" +
            "Content-Type: text/plain; charset=utf-8\r\n" +
            $"Content-Length: {bodyBytes.Length}\r\n\r\n";

        await stream.WriteAsync(Encoding.ASCII.GetBytes(head));
        await stream.WriteAsync(bodyBytes);
    }

    private IPAddress ResolveAddress(IPAddress remote, Dictionary<string, string> headers)
    {
        if (!IsTrustedProxy(remote))
            return remote;

        if (headers.TryGetValue("X-Real-IP", out var real) && IPAddress.TryParse(real.Trim(), out var realParsed))
            return realParsed;

        if (headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var last = forwarded.Split(',')[^1].Trim();
            if (IPAddress.TryParse(last, out var parsed))
                return parsed;
        }

        return remote;
    }

    private static IPAddress Normalize(IPAddress address)
    {
        return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
    }

    private bool IsTrustedProxy(IPAddress address)
    {
        address = Normalize(address);

        if (IPAddress.IsLoopback(address) || _trustedProxies.Contains(address))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
            return address.IsIPv6LinkLocal || address.IsIPv6UniqueLocal;

        var bytes = address.GetAddressBytes();
        return bytes[0] == 10 ||
               bytes[0] == 172 && (bytes[1] & 0xF0) == 16 ||
               bytes[0] == 192 && bytes[1] == 168 ||
               bytes[0] == 169 && bytes[1] == 254;
    }

    private bool TryReserveAddress(IPAddress address)
    {
        lock (_addressLock)
        {
            _addressCounts.TryGetValue(address, out var count);
            if (count >= _maxConnectionsPerAddress)
                return false;

            _addressCounts[address] = count + 1;
            return true;
        }
    }

    private void ReleaseAddress(IPAddress address)
    {
        lock (_addressLock)
        {
            if (!_addressCounts.TryGetValue(address, out var count))
                return;

            if (count <= 1)
                _addressCounts.Remove(address);
            else
                _addressCounts[address] = count - 1;
        }
    }

    private readonly record struct Outgoing(string? Text, WebSocketCloseStatus? Close, string? Reason);

    private sealed class Connection : IDisposable
    {
        public readonly WebSocket Socket;
        public readonly Channel<Outgoing> Outbox = Channel.CreateBounded<Outgoing>(new BoundedChannelOptions(64)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
        });

        public NetUserId User;

        private readonly CancellationTokenSource _cts;
        private readonly VoiceEffectProcessor _effects = new();
        private readonly short[] _pcm = new short[VoiceCodec.FrameSamples];
        private VoiceEffectSettings _lastEffect;
        private int _predictor;
        private int _index;
        private double _frameBudget = FrameBurst;
        private long _lastRefill = Stopwatch.GetTimestamp();

        public Connection(WebSocket socket, CancellationToken serverToken)
        {
            Socket = socket;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(serverToken);
        }

        public CancellationToken Token => _cts.Token;

        public void Enqueue(Outgoing item)
        {
            Outbox.Writer.TryWrite(item);
        }

        public void Close(WebSocketCloseStatus status, string reason)
        {
            Enqueue(new Outgoing(null, status, reason));
            CancelAfter(CloseGrace);
        }

        public void Cancel()
        {
            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void CancelAfter(TimeSpan delay)
        {
            try
            {
                _cts.CancelAfter(delay);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public byte[] ApplyEffect(ReadOnlySpan<byte> payload, VoiceEffectSettings effect)
        {
            if (effect != _lastEffect)
            {
                _lastEffect = effect;
                _predictor = 0;
                _index = 0;
            }

            if (effect == default || !VoiceCodec.Decode(payload, _pcm))
                return payload.ToArray();

            _effects.Process(_pcm, 0, _pcm.Length, effect);

            var output = new byte[VoiceCodec.FrameBytes];
            VoiceCodec.Encode(_pcm, ref _predictor, ref _index, output);
            return output;
        }

        public bool TryConsumeFrame()
        {
            var now = Stopwatch.GetTimestamp();
            var elapsed = (now - _lastRefill) / (double) Stopwatch.Frequency;
            _lastRefill = now;
            _frameBudget = Math.Min(FrameBurst, _frameBudget + elapsed * FramesPerSecond);

            if (_frameBudget < 1)
                return false;

            _frameBudget -= 1;
            return true;
        }

        public void Dispose()
        {
            Outbox.Writer.TryComplete();
            _cts.Dispose();
        }
    }

    private sealed class HttpRequestHead
    {
        public string Method = string.Empty;
        public Dictionary<string, string> Headers = new(StringComparer.OrdinalIgnoreCase);

        public static HttpRequestHead? Parse(string text)
        {
            var lines = text.Split("\r\n");
            var requestLine = lines[0].Split(' ');
            if (requestLine.Length != 3 || !requestLine[2].StartsWith("HTTP/1.", StringComparison.Ordinal))
                return null;

            var head = new HttpRequestHead { Method = requestLine[0] };
            for (var i = 1; i < lines.Length; i++)
            {
                var separator = lines[i].IndexOf(':');
                if (separator <= 0)
                    continue;

                var name = lines[i][..separator].Trim();
                var value = lines[i][(separator + 1)..].Trim();
                head.Headers[name] = head.Headers.TryGetValue(name, out var existing) ? $"{existing}, {value}" : value;
            }

            return head;
        }

        public string? GetHeader(string name)
        {
            return Headers.GetValueOrDefault(name);
        }

        public bool HasToken(string header, string token)
        {
            if (GetHeader(header) is not { } value)
                return false;

            foreach (var part in value.Split(','))
            {
                if (part.Trim().Equals(token, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
