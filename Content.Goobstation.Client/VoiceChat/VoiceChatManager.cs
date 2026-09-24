using System.Net;
using System.Net.Sockets;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Goobstation.Client.VoiceChat;

public sealed class VoiceChatManager
{
    [Dependency] private readonly IClientNetManager _net = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IUriOpener _uriOpener = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private readonly List<ushort> _mutedSpeakers = new();

    private ISawmill _sawmill = default!;
    private bool _receiveDisabled;

    public event Action<MsgVoiceFrame>? FrameReceived;
    public event Action<MsgVoiceSpeakerInfo>? SpeakerInfoReceived;
    public event Action<MsgVoiceSelf>? SelfReceived;
    public event Action<bool>? WebConnectedChanged;

    public bool WebConnected { get; private set; }

    public void Initialize()
    {
        _sawmill = _logManager.GetSawmill("voice");

        _net.RegisterNetMessage<MsgVoiceFrame>(message => FrameReceived?.Invoke(message));
        _net.RegisterNetMessage<MsgVoiceLinkRequest>();
        _net.RegisterNetMessage<MsgVoiceLink>(OnLink);
        _net.RegisterNetMessage<MsgVoiceSettings>();
        _net.RegisterNetMessage<MsgVoiceStatus>(OnStatus);
        _net.RegisterNetMessage<MsgVoiceSpeakerInfo>(message => SpeakerInfoReceived?.Invoke(message));
        _net.RegisterNetMessage<MsgVoiceSelf>(message => SelfReceived?.Invoke(message));

        _net.Connected += OnConnected;
        _cfg.OnValueChanged(GoobCVars.VoiceChatHearSelf, OnHearSelfChanged);
        _cfg.OnValueChanged(GoobCVars.VoiceChatVolume, OnVolumeChanged);
        _cfg.OnValueChanged(GoobCVars.VoiceChatRadioMuted, OnRadioMutedChanged);
    }

    public void Shutdown()
    {
        _net.Connected -= OnConnected;
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatHearSelf, OnHearSelfChanged);
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatVolume, OnVolumeChanged);
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatRadioMuted, OnRadioMutedChanged);
    }

    public static HashSet<string> ParseMutedChannels(string value)
    {
        var channels = new HashSet<string>();
        foreach (var part in value.Split(','))
        {
            var channel = part.Trim();
            if (channel.Length > 0)
                channels.Add(channel);
        }

        return channels;
    }

    public void SetMutedSpeakers(IEnumerable<ushort> speakers)
    {
        _mutedSpeakers.Clear();
        _mutedSpeakers.AddRange(speakers);
        SendSettings();
    }

    public bool RequestLink()
    {
        if (!_net.IsConnected || !_cfg.GetCVar(GoobCVars.VoiceChatEnabled))
            return false;

        _net.ClientSendMessage(new MsgVoiceLinkRequest());
        return true;
    }

    private void OnConnected(object? sender, NetChannelArgs args)
    {
        _mutedSpeakers.Clear();
        SetWebConnected(false);
        SendSettings();
    }

    private void OnStatus(MsgVoiceStatus message)
    {
        SetWebConnected(message.Connected);
    }

    private void SetWebConnected(bool connected)
    {
        if (WebConnected == connected)
            return;

        WebConnected = connected;
        WebConnectedChanged?.Invoke(connected);
    }

    private void OnHearSelfChanged(bool hearSelf)
    {
        SendSettings();
    }

    private void OnRadioMutedChanged(string muted)
    {
        SendSettings();
    }

    private void OnVolumeChanged(float volume)
    {
        if (volume <= 0f != _receiveDisabled)
            SendSettings();
    }

    private void SendSettings()
    {
        if (!_net.IsConnected)
            return;

        _receiveDisabled = _cfg.GetCVar(GoobCVars.VoiceChatVolume) <= 0f;
        _net.ClientSendMessage(new MsgVoiceSettings
        {
            HearSelf = _cfg.GetCVar(GoobCVars.VoiceChatHearSelf),
            Receive = !_receiveDisabled,
            MutedChannels = new List<string>(ParseMutedChannels(_cfg.GetCVar(GoobCVars.VoiceChatRadioMuted))),
            MutedSpeakers = new List<ushort>(_mutedSpeakers),
        });
    }

    private void OnLink(MsgVoiceLink message)
    {
        var baseUrl = message.Url;
        if (string.IsNullOrEmpty(baseUrl))
        {
            if (_net.ServerChannel is not { } channel)
                return;

            var address = channel.RemoteEndPoint.Address;
            if (address.IsIPv4MappedToIPv6)
                address = address.MapToIPv4();

            var host = IPAddress.IsLoopback(address)
                ? "localhost"
                : address.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{address}]" : address.ToString();

            baseUrl = $"http://{host}:{message.StatusPort}/voice/";
        }

        try
        {
            _uriOpener.OpenUri($"{baseUrl}#{message.Token}");
        }
        catch (ArgumentException e)
        {
            _sawmill.Error($"Could not open voice chat page {baseUrl}: {e.Message}");
        }
    }
}
