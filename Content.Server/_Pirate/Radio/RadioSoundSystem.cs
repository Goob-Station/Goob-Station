using Content.Goobstation.Common.Traits;
using Content.Server.Chat.Systems;
using Content.Server.Radio;
using Content.Shared.Chat;
using Content.Shared.CCVar;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared._Pirate.Radio;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Pirate.Radio;

public sealed class RadioSoundSystem : EntitySystem
{
    private static readonly SoundSpecifier RadioTalkSound =
        new SoundPathSpecifier("/Audio/_Pirate/Radio/radio_talk.ogg", AudioParams.Default.WithVariation(0.10f));

    private static readonly SoundSpecifier RadioReceiveSound =
        new SoundPathSpecifier("/Audio/_Pirate/Radio/radio_receive.ogg", AudioParams.Default.WithVariation(0.10f));

    private static readonly SoundSpecifier RadioImportantSound =
        new SoundPathSpecifier("/Audio/_Pirate/Radio/radio_important.ogg", AudioParams.Default.WithVariation(0.10f));

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetConfigurationManager _netConfig = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly Dictionary<(NetUserId UserId, EntityUid RadioUid), TimeSpan> _normalAudioCooldowns = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeadsetComponent, PirateRadioSentEvent>(OnHeadsetSent);
        SubscribeLocalEvent<RadioMicrophoneComponent, PirateRadioSentEvent>(OnMicrophoneSent);
        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, PirateRadioSentEvent>(OnIntrinsicSent);
        SubscribeLocalEvent<HeadsetComponent, PirateRadioReceivedEvent>(OnHeadsetReceive);
        SubscribeLocalEvent<RadioSpeakerComponent, PirateRadioReceivedEvent>(OnSpeakerReceive);
        SubscribeLocalEvent<IntrinsicRadioReceiverComponent, PirateRadioReceivedEvent>(OnIntrinsicReceive);
        SubscribeLocalEvent<HeadsetComponent, EntityTerminatingEvent>(OnRadioTerminating);
        SubscribeLocalEvent<RadioMicrophoneComponent, EntityTerminatingEvent>(OnRadioTerminating);
        SubscribeLocalEvent<RadioSpeakerComponent, EntityTerminatingEvent>(OnRadioTerminating);
        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, EntityTerminatingEvent>(OnRadioTerminating);
        SubscribeLocalEvent<IntrinsicRadioReceiverComponent, EntityTerminatingEvent>(OnRadioTerminating);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        PruneCooldowns();
    }

    private void OnHeadsetSent(EntityUid uid, HeadsetComponent component, ref PirateRadioSentEvent args)
        => PlayTransmitSound(uid, args);

    private void OnMicrophoneSent(EntityUid uid, RadioMicrophoneComponent component, ref PirateRadioSentEvent args)
        => PlayTransmitSound(uid, args);

    private void OnIntrinsicSent(EntityUid uid, IntrinsicRadioTransmitterComponent component, ref PirateRadioSentEvent args)
        => PlayTransmitSound(uid, args);

    private void OnHeadsetReceive(EntityUid uid, HeadsetComponent component, ref PirateRadioReceivedEvent args)
    {
        var wearer = Transform(uid).ParentUid;
        PlayReceiveSound(uid, wearer, args.MessageSource, args.Channel.ID, args.Important);
    }

    private void OnIntrinsicReceive(EntityUid uid, IntrinsicRadioReceiverComponent component, ref PirateRadioReceivedEvent args)
        => PlayReceiveSound(uid, uid, args.MessageSource, args.Channel.ID, args.Important);

    private void OnSpeakerReceive(EntityUid uid, RadioSpeakerComponent component, ref PirateRadioReceivedEvent args)
    {
        if (HasComp<IntercomComponent>(uid))
        {
            PlayIntercomReceiveSound(uid, component, args.MessageSource, args.Channel.ID, args.Important);
            return;
        }

        var holder = Transform(uid).ParentUid;
        PlayReceiveSound(uid, holder, args.MessageSource, args.Channel.ID, args.Important);
    }

    private void PlayIntercomReceiveSound(
        EntityUid radioUid,
        RadioSpeakerComponent component,
        EntityUid messageSource,
        ProtoId<RadioChannelPrototype> channelId,
        bool important)
    {
        if (!TryGetReceiveSound(radioUid, channelId, important, out var sound))
            return;

        var range = component.SpeakNormally ? ChatSystem.VoiceRange : ChatSystem.WhisperMuffledRange;

        foreach (var (listener, _) in _lookup.GetEntitiesInRange<ActorComponent>(Transform(radioUid).Coordinates, range, LookupFlags.Uncontained))
        {
            if (ShouldSuppressSelfReceiveSound(messageSource, listener, channelId))
                continue;

            if (!TryGetActorSession(listener, out var session))
                continue;

            if (!TryStartNormalCooldown(radioUid, session))
                continue;

            _audio.PlayGlobal(sound, session);
        }
    }

    private void PlayReceiveSound(
        EntityUid radioUid,
        EntityUid listener,
        EntityUid messageSource,
        ProtoId<RadioChannelPrototype> channelId,
        bool important)
    {
        if (ShouldSuppressSelfReceiveSound(messageSource, listener, channelId))
            return;

        if (!TryGetActorSession(listener, out var session))
            return;

        if (!TryGetReceiveSound(radioUid, channelId, important, out var sound))
            return;

        if (!TryStartNormalCooldown(radioUid, session))
            return;

        _audio.PlayGlobal(sound, session);
    }

    private static bool ShouldSuppressSelfReceiveSound(
        EntityUid messageSource,
        EntityUid listener,
        ProtoId<RadioChannelPrototype> channelId)
    {
        return messageSource == listener && channelId == SharedChatSystem.CommonChannel;
    }

    private void PlayTransmitSound(EntityUid radioUid, PirateRadioSentEvent args)
    {
        if (!TryGetTransmitSound(radioUid, args.Channel.ID, out var sound))
            return;

        if (args.Frequency == GetCommonFrequency())
            return;

        if (!TryGetActorSession(args.MessageSource, out var session))
            return;

        if (!TryStartNormalCooldown(radioUid, session))
            return;

        _audio.PlayGlobal(sound, session);
    }

    private int GetCommonFrequency()
    {
        return _prototype.Index<RadioChannelPrototype>(SharedChatSystem.CommonChannel).Frequency;
    }

    private bool TryGetActorSession(EntityUid uid, out ICommonSession session)
    {
        session = default!;

        if (!uid.IsValid())
            return false;

        if (HasComp<DeafComponent>(uid))
            return false;

        if (!TryComp<ActorComponent>(uid, out var actor))
            return false;

        session = actor.PlayerSession;
        return true;
    }

    private bool TryStartNormalCooldown(EntityUid uid, ICommonSession session)
    {
        if (!_netConfig.GetClientCVar(session.Channel, CCVars.RadioSoundsEnabled))
            return false;

        var delay = _netConfig.GetClientCVar(session.Channel, CCVars.RadioSoundsDelay);

        if (delay <= 0)
            return true;

        var now = _timing.CurTime;
        var key = (session.UserId, uid);

        if (_normalAudioCooldowns.TryGetValue(key, out var until) && until > now)
            return false;

        _normalAudioCooldowns[key] = now + TimeSpan.FromSeconds(delay);
        return true;
    }

    private void OnRadioTerminating<T>(EntityUid uid, T component, ref EntityTerminatingEvent args)
    {
        if (_normalAudioCooldowns.Count == 0)
            return;

        var keysToRemove = new List<(NetUserId UserId, EntityUid RadioUid)>();

        foreach (var (key, _) in _normalAudioCooldowns)
        {
            if (key.RadioUid == uid)
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
        {
            _normalAudioCooldowns.Remove(key);
        }
    }

    private void PruneCooldowns()
    {
        if (_normalAudioCooldowns.Count == 0)
            return;

        var now = _timing.CurTime;
        var keysToRemove = new List<(NetUserId UserId, EntityUid RadioUid)>();

        foreach (var (key, until) in _normalAudioCooldowns)
        {
            if (until <= now || !Exists(key.RadioUid))
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
        {
            _normalAudioCooldowns.Remove(key);
        }
    }

    private bool TryGetTransmitSound(
        EntityUid radioUid,
        ProtoId<RadioChannelPrototype> channelId,
        out SoundSpecifier sound)
    {
        sound = RadioTalkSound;

        if (!TryComp<RadioSoundProfileComponent>(radioUid, out var profile))
            return true;

        if (profile.AllowedChannels.Count > 0 && !profile.AllowedChannels.Contains(channelId))
            return false;

        sound = profile.TransmitSound ?? RadioTalkSound;
        return true;
    }

    private bool TryGetReceiveSound(
        EntityUid radioUid,
        ProtoId<RadioChannelPrototype> channelId,
        bool important,
        out SoundSpecifier sound)
    {
        sound = important ? RadioImportantSound : RadioReceiveSound;

        if (!TryComp<RadioSoundProfileComponent>(radioUid, out var profile))
            return true;

        if (profile.AllowedChannels.Count > 0 && !profile.AllowedChannels.Contains(channelId))
            return false;

        sound = profile.ReceiveSound ?? sound;
        return true;
    }
}
