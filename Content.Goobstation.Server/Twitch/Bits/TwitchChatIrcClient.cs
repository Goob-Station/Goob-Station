using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchChatIrcClient(
    string channel,
    string botLogin,
    string oauthToken,
    Action<string, string> onCommand)
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly string _channel = channel.Trim().TrimStart('#').ToLowerInvariant();
    private readonly string _botLogin = botLogin.Trim().ToLowerInvariant();
    private readonly string _oauthToken = oauthToken.Trim();
    private volatile bool _isConnected;

    public bool IsConnected => _isConnected;

    public void Start()
    {
        _ = Run();
    }

    public void Stop()
    {
        _cancellation.Cancel();
    }

    private async Task Run()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                await ConnectAndRead(_cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), _cancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
            finally
            {
                _isConnected = false;
            }
        }
    }

    private async Task ConnectAndRead(CancellationToken cancellation)
    {
        using var socket = new ClientWebSocket();
        await socket.ConnectAsync(new Uri("wss://irc-ws.chat.twitch.tv:443"), cancellation);
        await Send(socket, "CAP REQ :twitch.tv/tags twitch.tv/commands", cancellation);
        var password = _oauthToken.StartsWith("oauth:", StringComparison.OrdinalIgnoreCase)
            ? _oauthToken
            : $"oauth:{_oauthToken}";
        await Send(socket, $"PASS {password}", cancellation);
        await Send(socket, $"NICK {_botLogin}", cancellation);
        await Send(socket, $"JOIN #{_channel}", cancellation);

        var buffer = new byte[8192];
        var pending = new StringBuilder();
        while (socket.State == WebSocketState.Open && !cancellation.IsCancellationRequested)
        {
            var result = await socket.ReceiveAsync(buffer, cancellation);
            if (result.MessageType == WebSocketMessageType.Close)
                return;

            pending.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            if (!result.EndOfMessage)
                continue;

            var messages = pending.ToString().Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            pending.Clear();
            foreach (var message in messages)
            {
                if (message.Contains($" 366 {_botLogin} #{_channel} ", StringComparison.OrdinalIgnoreCase))
                    _isConnected = true;

                if (message.StartsWith("PING ", StringComparison.Ordinal))
                {
                    await Send(socket, "PONG " + message[5..], cancellation);
                    continue;
                }

                ParseMessage(message);
            }
        }
    }

    private static Task Send(ClientWebSocket socket, string message, CancellationToken cancellation)
    {
        return socket.SendAsync(
            Encoding.UTF8.GetBytes(message + "\r\n"),
            WebSocketMessageType.Text,
            true,
            cancellation);
    }

    private void ParseMessage(string message)
    {
        var marker = $" PRIVMSG #{_channel} :";
        var markerIndex = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
            return;

        var command = message[(markerIndex + marker.Length)..].Trim().ToLowerInvariant();
        if (command is not ("up" or "down" or "left" or "right" or "bite"))
            return;

        var displayName = "Twitch chat";
        if (message.StartsWith('@'))
        {
            var tagEnd = message.IndexOf(' ');
            var tags = tagEnd > 1 ? message[1..tagEnd] : string.Empty;
            foreach (var tag in tags.Split(';'))
            {
                if (!tag.StartsWith("display-name=", StringComparison.Ordinal))
                    continue;

                var value = tag[13..];
                if (!string.IsNullOrWhiteSpace(value))
                    displayName = value;
                break;
            }
        }

        onCommand(displayName, command);
    }
}
