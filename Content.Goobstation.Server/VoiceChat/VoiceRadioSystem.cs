using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Chat;
using Content.Shared.Inventory;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Whitelist;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.VoiceChat;

public sealed class VoiceRadioSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(1);

    private readonly List<ProtoId<RadioChannelPrototype>> _channels = new();
    private readonly List<ProtoId<RadioChannelPrototype>> _receive = new();
    private readonly HashSet<EntityUid> _tracked = new();
    private readonly HashSet<EntityUid> _current = new();
    private bool _enabled;
    private bool _common;
    private TimeSpan _nextRefresh;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<VoiceRadioSelectEvent>(OnSelect);

        Subs.CVar(_cfg, GoobCVars.VoiceChatEnabled, _ => Refresh(), true);
        Subs.CVar(_cfg, GoobCVars.VoiceChatRadioEnabled, _ => Refresh(), true);
        Subs.CVar(_cfg, GoobCVars.VoiceChatRadioCommon, _ => Refresh(), true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextRefresh)
            return;

        _nextRefresh = _timing.CurTime + RefreshInterval;
        Refresh();
    }

    public bool TryGetTransmission(EntityUid speaker, out RadioChannelPrototype channel, out EntityUid radioSource)
    {
        return TryGetTransmission(speaker, false, out channel, out radioSource);
    }

    public bool TryGetTransmission(EntityUid speaker, bool force, out RadioChannelPrototype channel, out EntityUid radioSource)
    {
        channel = default!;
        radioSource = default;

        if (!_enabled ||
            !TryComp<VoiceRadioComponent>(speaker, out var voiceRadio) ||
            (voiceRadio.Active ?? (force && voiceRadio.Channels.Count > 0 ? (ProtoId<RadioChannelPrototype>?) voiceRadio.Channels[0] : null)) is not { } active ||
            !_common && active == SharedChatSystem.CommonChannel ||
            !_prototype.TryIndex(active, out var prototype) ||
            !CanTransmit(speaker, prototype, out radioSource))
        {
            return false;
        }

        channel = prototype;
        return true;
    }

    public bool TryGetMicrophoneChannel(ProtoId<RadioChannelPrototype> id, out RadioChannelPrototype channel)
    {
        channel = default!;
        if (!_enabled ||
            !_common && id == SharedChatSystem.CommonChannel ||
            !_prototype.TryIndex(id, out var prototype))
        {
            return false;
        }

        channel = prototype;
        return true;
    }

    public void GetReceivers(EntityUid radioSource, RadioChannelPrototype channel, List<EntityUid> receivers)
    {
        _radio.GetVoiceReceivers(radioSource, channel, receivers);
    }

    private void Refresh()
    {
        _enabled = _cfg.GetCVar(GoobCVars.VoiceChatEnabled) && _cfg.GetCVar(GoobCVars.VoiceChatRadioEnabled);
        _common = _cfg.GetCVar(GoobCVars.VoiceChatRadioCommon);

        _current.Clear();
        foreach (var session in _player.Sessions)
        {
            if (session.Status != SessionStatus.InGame || session.AttachedEntity is not { } uid)
                continue;

            _current.Add(uid);
            RefreshEntity(uid);
        }

        foreach (var uid in _tracked)
        {
            if (!_current.Contains(uid) && !TerminatingOrDeleted(uid))
                RemComp<VoiceRadioComponent>(uid);
        }

        _tracked.Clear();
        _tracked.UnionWith(_current);
    }

    private void RefreshEntity(EntityUid uid)
    {
        _channels.Clear();
        _receive.Clear();
        if (_enabled)
        {
            CollectChannels(uid);
            CollectReceive(uid);
        }

        if (_channels.Count == 0 && _receive.Count == 0)
        {
            RemComp<VoiceRadioComponent>(uid);
            return;
        }

        var voiceRadio = EnsureComp<VoiceRadioComponent>(uid);
        var active = voiceRadio.Active is { } current && _channels.Contains(current) ? current : (ProtoId<RadioChannelPrototype>?) null;
        if (voiceRadio.Channels.SequenceEqual(_channels) &&
            voiceRadio.Receive.SequenceEqual(_receive) &&
            voiceRadio.Active == active)
        {
            return;
        }

        voiceRadio.Channels = new List<ProtoId<RadioChannelPrototype>>(_channels);
        voiceRadio.Receive = new List<ProtoId<RadioChannelPrototype>>(_receive);
        voiceRadio.Active = active;
        Dirty(uid, voiceRadio);
    }

    private void CollectChannels(EntityUid uid)
    {
        if (TryComp<WearingHeadsetComponent>(uid, out var wearing) &&
            TryComp<HeadsetComponent>(wearing.Headset, out var headset) &&
            headset.Enabled &&
            TryComp<EncryptionKeyHolderComponent>(wearing.Headset, out var keys))
        {
            AddChannels(uid, keys.Channels);
        }

        if (TryComp<IntrinsicRadioTransmitterComponent>(uid, out var intrinsic))
            AddChannels(uid, intrinsic.Channels);

        _channels.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
    }

    private void AddChannels(EntityUid uid, IEnumerable<ProtoId<RadioChannelPrototype>> channels)
    {
        foreach (var id in channels)
        {
            if (!_common && id == SharedChatSystem.CommonChannel)
                continue;

            if (!_prototype.TryIndex(id, out var channel) ||
                !_whitelist.IsWhitelistPassOrNull(channel.SendWhitelist, uid))
            {
                continue;
            }

            if (!_channels.Contains(id))
                _channels.Add(id);
        }
    }

    private void CollectReceive(EntityUid uid)
    {
        if (TryComp<WearingHeadsetComponent>(uid, out var wearing) &&
            TryComp<ActiveRadioComponent>(wearing.Headset, out var headsetRadio))
        {
            AddReceive(uid, headsetRadio);
        }

        if (TryComp<ActiveRadioComponent>(uid, out var intrinsic))
            AddReceive(uid, intrinsic);

        foreach (var item in _inventory.GetHandOrInventoryEntities(uid))
        {
            if (TryComp<RadioSpeakerComponent>(item, out var speaker) && speaker.Enabled)
                AddReceive(uid, speaker.Channels);
        }

        _receive.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
    }

    private void AddReceive(EntityUid uid, ActiveRadioComponent radio)
    {
        if (!radio.ReceiveAllChannels)
        {
            AddReceive(uid, radio.Channels);
            return;
        }

        foreach (var channel in _prototype.EnumeratePrototypes<RadioChannelPrototype>())
        {
            AddReceive(uid, new[] { new ProtoId<RadioChannelPrototype>(channel.ID) });
        }
    }

    private void AddReceive(EntityUid uid, IEnumerable<ProtoId<RadioChannelPrototype>> channels)
    {
        foreach (var id in channels)
        {
            if (!_common && id == SharedChatSystem.CommonChannel)
                continue;

            if (!_prototype.TryIndex(id, out var channel) ||
                !_whitelist.IsWhitelistPassOrNull(channel.ReceiveWhitelist, uid))
            {
                continue;
            }

            if (!_receive.Contains(id))
                _receive.Add(id);
        }
    }

    private bool CanTransmit(EntityUid uid, RadioChannelPrototype channel, out EntityUid radioSource)
    {
        radioSource = default;
        if (!_whitelist.IsWhitelistPassOrNull(channel.SendWhitelist, uid))
            return false;

        if (TryComp<WearingHeadsetComponent>(uid, out var wearing) &&
            TryComp<HeadsetComponent>(wearing.Headset, out var headset) &&
            headset.Enabled &&
            TryComp<EncryptionKeyHolderComponent>(wearing.Headset, out var keys) &&
            keys.Channels.Contains(channel.ID))
        {
            radioSource = wearing.Headset;
            return true;
        }

        if (TryComp<IntrinsicRadioTransmitterComponent>(uid, out var intrinsic) &&
            intrinsic.Channels.Contains(channel.ID))
        {
            radioSource = uid;
            return true;
        }

        return false;
    }

    private void OnSelect(VoiceRadioSelectEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } uid ||
            !TryComp<VoiceRadioComponent>(uid, out var voiceRadio))
        {
            return;
        }

        if (ev.Channel is { } channel && !voiceRadio.Channels.Contains(channel))
            return;

        if (voiceRadio.Active == ev.Channel)
            return;

        voiceRadio.Active = ev.Channel;
        Dirty(uid, voiceRadio);
    }
}
