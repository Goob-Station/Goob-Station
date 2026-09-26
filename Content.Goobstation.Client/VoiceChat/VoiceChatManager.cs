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
    public event Action<bool>? DeafenedChanged;
    public event Action? LinkChanged;

    public bool WebConnected { get; private set; }

    public bool MicMuted { get; private set; }

    public bool Deafened { get; private set; }

    public string? PageUrl { get; private set; }

    public string? LinkCode { get; private set; }

    public string? LinkUrl => PageUrl == null || LinkCode == null ? null : $"{PageUrl}?code={LinkCode}";

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
        _net.RegisterNetMessage<MsgVoicePushToTalk>();
        _net.RegisterNetMessage<MsgVoiceMicMute>();

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

    public void SendPushToTalk(bool pressed, bool radio)
    {
        if (_net.IsConnected)
            _net.ClientSendMessage(new MsgVoicePushToTalk { Pressed = pressed, Radio = radio });
    }

    public void SetMicMuted(bool muted)
    {
        MicMuted = muted;
        SendMicMute();
    }

    public void SetDeafened(bool deafened)
    {
        if (Deafened == deafened)
            return;

        Deafened = deafened;
        SendMicMute();
        SendSettings();
        DeafenedChanged?.Invoke(deafened);
    }

    public bool RequestLink()
    {
        if (!_net.IsConnected || !_cfg.GetCVar(GoobCVars.VoiceChatEnabled))
            return false;

        _net.ClientSendMessage(new MsgVoiceLinkRequest());
        return true;
    }

    public void OpenLink()
    {
        if (LinkUrl is not { } url)
            return;

        try
        {
            _uriOpener.OpenUri(url);
        }
        catch (ArgumentException e)
        {
            _sawmill.Error($"Could not open voice chat page {PageUrl}: {e.Message}");
        }
    }

    private void OnConnected(object? sender, NetChannelArgs args)
    {
        _mutedSpeakers.Clear();
        PageUrl = null;
        LinkCode = null;
        LinkChanged?.Invoke();
        SetWebConnected(false);
        SendSettings();
        SendMicMute();
    }

    private void SendMicMute()
    {
        if (_net.IsConnected)
            _net.ClientSendMessage(new MsgVoiceMicMute { Muted = MicMuted || Deafened });
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
            Receive = !_receiveDisabled && !Deafened,
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

        PageUrl = baseUrl;
        LinkCode = message.Token;
        LinkChanged?.Invoke();
    }
}
