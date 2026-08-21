using System.Net.WebSockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchChatIrcClient : IAsyncDisposable
{
    private readonly string _botLogin;
    private readonly string _oauthToken;
    private readonly Action<string, string, string> _onCommand;
    private readonly HashSet<string> _desiredChannels = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _joinedChannels = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _channelLock = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly CancellationTokenSource _cancellation = new();
    private ClientWebSocket? _socket;
    private Task? _runTask;

    public TwitchChatIrcClient(string botLogin, string oauthToken, Action<string, string, string> onCommand)
    {
        _botLogin = botLogin.Trim().ToLowerInvariant();
        _oauthToken = oauthToken.Trim();
        _onCommand = onCommand;
    }

    public void Start()
    {
        _runTask ??= Task.Run(RunAsync);
    }

    public void JoinChannel(string channel)
    {
        channel = NormalizeChannel(channel);
        if (channel.Length == 0)
            return;

        lock (_channelLock)
            _desiredChannels.Add(channel);

        _ = SendAsync($"JOIN #{channel}", _cancellation.Token);
    }

    public void LeaveChannel(string channel)
    {
        channel = NormalizeChannel(channel);
        if (channel.Length == 0)
            return;

        lock (_channelLock)
        {
            _desiredChannels.Remove(channel);
            _joinedChannels.Remove(channel);
        }

        _ = SendAsync($"PART #{channel}", _cancellation.Token);
    }

    public bool IsConnected(string channel)
    {
        channel = NormalizeChannel(channel);
        lock (_channelLock)
            return _joinedChannels.Contains(channel);
    }

    private async Task RunAsync()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                await ConnectAndReadAsync(_cancellation.Token);
            }
            catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
            {
                break;
            }
            catch
            {
            }

            lock (_channelLock)
                _joinedChannels.Clear();

            if (!_cancellation.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), _cancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task ConnectAndReadAsync(CancellationToken cancellationToken)
    {
        _socket?.Dispose();
        _socket = new ClientWebSocket();
        await _socket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), cancellationToken);
        await SendAsync("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership", cancellationToken);
        var password = _oauthToken.StartsWith("oauth:", StringComparison.OrdinalIgnoreCase)
            ? _oauthToken
            : $"oauth:{_oauthToken}";
        await SendAsync($"PASS {password}", cancellationToken);
        await SendAsync($"NICK {_botLogin}", cancellationToken);

        string[] channels;
        lock (_channelLock)
            channels = _desiredChannels.ToArray();

        foreach (var channel in channels)
            await SendAsync($"JOIN #{channel}", cancellationToken);

        var buffer = new byte[8192];
        var pending = new StringBuilder();

        while (_socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await _socket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            pending.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            if (!result.EndOfMessage)
                continue;

            var messages = pending.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            pending.Clear();
            foreach (var message in messages)
                await HandleMessageAsync(message, cancellationToken);
        }
    }

    private async Task HandleMessageAsync(string message, CancellationToken cancellationToken)
    {
        if (message.Contains("Login authentication failed", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("Improperly formatted auth", StringComparison.OrdinalIgnoreCase))
        {
            if (_socket != null)
                await _socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Authentication failed", cancellationToken);
            return;
        }

        if (message.StartsWith("PING ", StringComparison.Ordinal))
        {
            await SendAsync("PONG " + message[5..], cancellationToken);
            return;
        }

        if (message.Contains(" RECONNECT", StringComparison.Ordinal))
        {
            if (_socket != null)
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnect", cancellationToken);
            return;
        }

        var joinIndex = message.IndexOf(" 366 ", StringComparison.Ordinal);
        if (joinIndex >= 0)
        {
            var channelStart = message.IndexOf('#', joinIndex);
            if (channelStart >= 0)
            {
                var channelEnd = message.IndexOf(' ', channelStart);
                var joinedChannel = message[(channelStart + 1)..(channelEnd < 0 ? message.Length : channelEnd)];
                lock (_channelLock)
                    _joinedChannels.Add(joinedChannel);
            }
            return;
        }

        var roomStateIndex = message.IndexOf(" ROOMSTATE #", StringComparison.Ordinal);
        if (roomStateIndex >= 0)
        {
            MarkJoined(message[(roomStateIndex + 12)..]);
            return;
        }

        var ownJoinIndex = message.IndexOf(" JOIN #", StringComparison.Ordinal);
        if (ownJoinIndex >= 0 &&
            message.Contains($":{_botLogin}!", StringComparison.OrdinalIgnoreCase))
        {
            MarkJoined(message[(ownJoinIndex + 7)..]);
            return;
        }

        var marker = " PRIVMSG #";
        var markerIndex = message.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
            return;

        var channelStartIndex = markerIndex + marker.Length;
        var channelEndIndex = message.IndexOf(" :", channelStartIndex, StringComparison.Ordinal);
        if (channelEndIndex < 0)
            return;

        var channel = message[channelStartIndex..channelEndIndex];
        lock (_channelLock)
        {
            if (!_desiredChannels.Contains(channel))
                return;
        }

        var prefixStart = message.IndexOf(':');
        var prefixEnd = message.IndexOf('!', prefixStart + 1);
        if (prefixStart < 0 || prefixEnd < 0)
            return;

        var viewer = message[(prefixStart + 1)..prefixEnd];
        var command = message[(channelEndIndex + 2)..].Trim().ToLowerInvariant();
        if (command is not ("up" or "down" or "left" or "right" or "bite"))
            return;

        if (message.StartsWith('@'))
        {
            var tagEnd = message.IndexOf(' ');
            var tags = tagEnd > 1 ? message[1..tagEnd] : string.Empty;
            foreach (var tag in tags.Split(';'))
            {
                if (!tag.StartsWith("display-name=", StringComparison.Ordinal))
                    continue;

                var displayName = tag[13..];
                if (!string.IsNullOrWhiteSpace(displayName))
                    viewer = displayName;
                break;
            }
        }

        _onCommand(channel, viewer, command);
    }

    private void MarkJoined(string value)
    {
        var end = value.IndexOf(' ');
        var channel = value[..(end < 0 ? value.Length : end)].Trim().TrimStart('#');
        if (channel.Length == 0)
            return;

        lock (_channelLock)
        {
            if (_desiredChannels.Contains(channel))
                _joinedChannels.Add(channel);
        }
    }

    private async Task SendAsync(string message, CancellationToken cancellationToken)
    {
        var socket = _socket;
        if (socket?.State != WebSocketState.Open)
            return;

        var bytes = Encoding.UTF8.GetBytes(message + "\r\n");
        var lockTaken = false;
        try
        {
            await _sendLock.WaitAsync(cancellationToken);
            lockTaken = true;
            if (socket.State == WebSocketState.Open)
                await socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
        }
        catch (WebSocketException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            if (lockTaken)
                _sendLock.Release();
        }
    }

    private static string NormalizeChannel(string channel)
    {
        return channel.Trim().TrimStart('#').ToLowerInvariant();
    }

    public async ValueTask DisposeAsync()
    {
        _cancellation.Cancel();
        if (_runTask != null)
        {
            try
            {
                await _runTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        _socket?.Dispose();
        _sendLock.Dispose();
        _cancellation.Dispose();
    }
}
