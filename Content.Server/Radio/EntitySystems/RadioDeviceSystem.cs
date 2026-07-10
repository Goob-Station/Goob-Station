// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._Pirate.Radio.Components;
using Content.Server.Chat.Systems;
using Content.Server.Interaction;
using Content.Server._EinsteinEngines.Language;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Shared.Chat;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Power;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Radio.EntitySystems;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.Chat;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Radio.Components;
using Content.Shared.UserInterface; // Pirate - Handheld Radios port
using Content.Shared._Pirate.Radio; // Pirate - Handheld Radios port
using Robust.Server.GameObjects; // Pirate - Handheld Radios port
using Robust.Shared.Network; // Pirate - Handheld Radios port
using Robust.Shared.Player; // Pirate - Handheld Radios port
using Robust.Shared.Prototypes;

namespace Content.Server.Radio.EntitySystems;

/// <summary>
///     This system handles radio speakers and microphones (which together form a hand-held radio).
/// </summary>
public sealed class RadioDeviceSystem : SharedRadioDeviceSystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly InteractionSystem _interaction = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly LanguageSystem _language = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!; // Goob
    [Dependency] private readonly UserInterfaceSystem _ui = default!; // Pirate - Handheld Radios port
    [Dependency] private readonly INetManager _netMan = default!; // Pirate - Handheld Radios port

    // Used to prevent a shitter from using a bunch of radios to spam chat.
    private HashSet<(string, EntityUid, RadioChannelPrototype)> _recentlySent = new();

    // Pirate start - Handheld Radios port
    private const int MinRadioFrequency = 1000;
    private const int MaxRadioFrequency = 3000;
    // Pirate end - Handheld Radios port

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RadioMicrophoneComponent, ComponentInit>(OnMicrophoneInit);
        SubscribeLocalEvent<RadioMicrophoneComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<RadioMicrophoneComponent, ActivateInWorldEvent>(OnActivateMicrophone);
        SubscribeLocalEvent<RadioMicrophoneComponent, ListenEvent>(OnListen);
        SubscribeLocalEvent<RadioMicrophoneComponent, ListenAttemptEvent>(OnAttemptListen);
        SubscribeLocalEvent<RadioMicrophoneComponent, PowerChangedEvent>(OnPowerChanged);

        SubscribeLocalEvent<RadioSpeakerComponent, ComponentInit>(OnSpeakerInit);
        SubscribeLocalEvent<RadioSpeakerComponent, ActivateInWorldEvent>(OnActivateSpeaker);
        SubscribeLocalEvent<RadioSpeakerComponent, RadioReceiveEvent>(OnReceiveRadio);

        SubscribeLocalEvent<IntercomComponent, EncryptionChannelsChangedEvent>(OnIntercomEncryptionChannelsChanged);
        SubscribeLocalEvent<IntercomComponent, ToggleIntercomMicMessage>(OnToggleIntercomMic);
        SubscribeLocalEvent<IntercomComponent, ToggleIntercomSpeakerMessage>(OnToggleIntercomSpeaker);
        SubscribeLocalEvent<IntercomComponent, SelectIntercomChannelMessage>(OnSelectIntercomChannel);

        // Pirate start - Handheld Radios port
        SubscribeLocalEvent<RadioMicrophoneComponent, BeforeActivatableUIOpenEvent>(OnBeforeHandheldRadioUiOpen);
        SubscribeLocalEvent<RadioMicrophoneComponent, ToggleHandheldRadioMicMessage>(OnToggleHandheldRadioMic);
        SubscribeLocalEvent<RadioMicrophoneComponent, ToggleHandheldRadioSpeakerMessage>(OnToggleHandheldRadioSpeaker);
        SubscribeLocalEvent<RadioMicrophoneComponent, SelectHandheldRadioFrequencyMessage>(OnChangeHandheldRadioFrequency);
        SubscribeLocalEvent<IntercomComponent, MapInitEvent>(OnMapInit);
        // Pirate end - Handheld Radios port
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _recentlySent.Clear();
    }


    #region Component Init
    private void OnMicrophoneInit(EntityUid uid, RadioMicrophoneComponent component, ComponentInit args)
    {
        if (component.Enabled)
            EnsureComp<ActiveListenerComponent>(uid).Range = component.ListenRange;
        else
            RemCompDeferred<ActiveListenerComponent>(uid);
    }

    private void OnSpeakerInit(EntityUid uid, RadioSpeakerComponent component, ComponentInit args)
    {
        if (component.Enabled)
            EnsureComp<ActiveRadioComponent>(uid).Channels.UnionWith(component.Channels);
        else
            RemCompDeferred<ActiveRadioComponent>(uid);
    }
    #endregion

    #region Toggling
    private void OnActivateMicrophone(EntityUid uid, RadioMicrophoneComponent component, ActivateInWorldEvent args)
    {
        if (!args.Complex)
            return;

        if (!component.ToggleOnInteract)
            return;

        ToggleRadioMicrophone(uid, args.User, args.Handled, component);
        args.Handled = true;
    }

    private void OnActivateSpeaker(EntityUid uid, RadioSpeakerComponent component, ActivateInWorldEvent args)
    {
        if (!args.Complex)
            return;

        if (!component.ToggleOnInteract)
            return;

        ToggleRadioSpeaker(uid, args.User, args.Handled, component);
        args.Handled = true;
    }
    private void OnPowerChanged(EntityUid uid, RadioMicrophoneComponent component, ref PowerChangedEvent args)
    {
        if (args.Powered)
            return;
        SetMicrophoneEnabled(uid, null, false, true, component);
    }


    public override void SetMicrophoneEnabled(EntityUid uid, EntityUid? user, bool enabled, bool quiet = false, RadioMicrophoneComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (component.PowerRequired && !this.IsPowered(uid, EntityManager))
            return;

        component.Enabled = enabled;

        if (!quiet && user != null)
        {
            var state = Loc.GetString(component.Enabled ? "handheld-radio-component-on-state" : "handheld-radio-component-off-state");
            var message = Loc.GetString("handheld-radio-component-on-use", ("radioState", state));
            _popup.PopupEntity(message, user.Value, user.Value);
        }

        _appearance.SetData(uid, RadioDeviceVisuals.Broadcasting, component.Enabled);
        if (component.Enabled)
            EnsureComp<ActiveListenerComponent>(uid).Range = component.ListenRange;
        else
            RemCompDeferred<ActiveListenerComponent>(uid);
    }

    #endregion

    private void OnExamine(EntityUid uid, RadioMicrophoneComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var proto = _protoMan.Index<RadioChannelPrototype>(component.BroadcastChannel);

        using (args.PushGroup(nameof(RadioMicrophoneComponent)))
        {
            // Pirate start - Handheld Radios port
            args.PushMarkup(Loc.GetString("handheld-radio-component-on-examine", ("frequency", GetHandheldRadioFrequency(uid, proto)), ("color", proto.Color.ToHex())));
            args.PushMarkup(Loc.GetString("handheld-radio-component-channel-examine", ("channel", proto.LocalizedName), ("color", proto.Color.ToHex())));
            // Pirate end - Handheld Radios port
        }
    }

    private void OnListen(EntityUid uid, RadioMicrophoneComponent component, ListenEvent args)
    {
        if (HasComp<RadioSpeakerComponent>(args.Source))
            return; // no feedback loops please.

        var channel = _protoMan.Index<RadioChannelPrototype>(component.BroadcastChannel)!;
        if (_recentlySent.Add((args.Message, args.Source, channel)))
            _radio.SendRadioMessage(args.Source, args.Message, channel, uid, /*Pirate-handheld-radios-start*/ frequency: GetHandheldRadioFrequency(uid, channel) /*Pirate-handheld-radios-end*/);
    }

    private void OnAttemptListen(EntityUid uid, RadioMicrophoneComponent component, ListenAttemptEvent args)
    {
        if (component.PowerRequired && !this.IsPowered(uid, EntityManager)
            || component.UnobstructedRequired && !_interaction.InRangeUnobstructed(args.Source, uid, 0))
        {
            args.Cancel();
        }
    }

    private void OnReceiveRadio(EntityUid uid, RadioSpeakerComponent component, ref RadioReceiveEvent args)
    {
        if (uid == args.RadioSource || component.PowerRequired && !_power.IsPowered(uid)) // Goobstation, powered required
            return;

        var nameEv = new TransformSpeakerNameEvent(args.MessageSource, Name(args.MessageSource));
        RaiseLocalEvent(args.MessageSource, nameEv);

        var name = Loc.GetString("speech-name-relay",
            ("speaker", Name(uid)),
            ("originalName", nameEv.VoiceName));

        // log to chat so people can identity the speaker/source, but avoid clogging ghost chat if there are many radios
        var message = args.OriginalChatMsg.Message; // The chat system will handle the rest and re-obfuscate if needed.

        _chat.TrySendInGameICMessage(uid,
            message,
            component.SpeakNormally ? InGameICChatType.Speak : InGameICChatType.Whisper, // Goobstation - radio host
            ChatTransmitRange.GhostRangeLimit,
            nameOverride: name,
            checkRadioPrefix: component.SpeakNormally,
            languageOverride: args.Language); // Einstein Engines - Languages
    }

    private void OnIntercomEncryptionChannelsChanged(Entity<IntercomComponent> ent, ref EncryptionChannelsChangedEvent args)
    {
        ent.Comp.SupportedChannels = args.Component.Channels.Select(p => new ProtoId<RadioChannelPrototype>(p)).ToList();

        var channel = args.Component.DefaultChannel;
        if (ent.Comp.CurrentChannel != null && ent.Comp.SupportedChannels.Contains(ent.Comp.CurrentChannel.Value))
            channel = ent.Comp.CurrentChannel;

        SetIntercomChannel(ent, channel);
    }

    private void OnToggleIntercomMic(Entity<IntercomComponent> ent, ref ToggleIntercomMicMessage args)
    {
        if (ent.Comp.RequiresPower && !this.IsPowered(ent, EntityManager))
            return;

        SetMicrophoneEnabled(ent, args.Actor, args.Enabled, true);
        ent.Comp.MicrophoneEnabled = args.Enabled;
        Dirty(ent);
    }

    private void OnToggleIntercomSpeaker(Entity<IntercomComponent> ent, ref ToggleIntercomSpeakerMessage args)
    {
        if (ent.Comp.RequiresPower && !this.IsPowered(ent, EntityManager))
            return;

        SetSpeakerEnabled(ent, args.Actor, args.Enabled, true);
        ent.Comp.SpeakerEnabled = args.Enabled;
        Dirty(ent);
    }

    private void OnSelectIntercomChannel(Entity<IntercomComponent> ent, ref SelectIntercomChannelMessage args)
    {
        if (ent.Comp.RequiresPower && !this.IsPowered(ent, EntityManager))
            return;

        if (!_protoMan.TryIndex<RadioChannelPrototype>(args.Channel, out var channel) || !ent.Comp.SupportedChannels.Contains(args.Channel)) // Pirate - Handheld Radios port
            return;

        SetIntercomChannel(ent, args.Channel);
    }

    private void SetIntercomChannel(Entity<IntercomComponent> ent, ProtoId<RadioChannelPrototype>? channel)
    {
        ent.Comp.CurrentChannel = channel;

        if (channel == null)
        {
            SetSpeakerEnabled(ent, null, false);
            SetMicrophoneEnabled(ent, null, false);
            ent.Comp.MicrophoneEnabled = false;
            ent.Comp.SpeakerEnabled = false;
            Dirty(ent);
            return;
        }

        if (TryComp<RadioMicrophoneComponent>(ent, out var mic))
        {
            mic.BroadcastChannel = channel.Value;
            if(_protoMan.TryIndex<RadioChannelPrototype>(channel.Value, out var channelProto)) // Pirate - Handheld Radios port
                SetHandheldRadioFrequency(ent, GetHandheldRadioFrequency(ent, channelProto)); // Pirate - Handheld Radios port
        }

        if (TryComp<RadioSpeakerComponent>(ent, out var speaker))
            speaker.Channels = new() { channel.Value };
        Dirty(ent);
    }

    // Pirate start - Handheld Radios port
    #region Handheld Radio

    private void OnBeforeHandheldRadioUiOpen(Entity<RadioMicrophoneComponent> microphone, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateHandheldRadioUi(microphone);
    }

    private void OnToggleHandheldRadioMic(Entity<RadioMicrophoneComponent> microphone, ref ToggleHandheldRadioMicMessage args)
    {
        if (!args.Actor.Valid)
            return;

        SetMicrophoneEnabled(microphone, args.Actor, args.Enabled, true);
        UpdateHandheldRadioUi(microphone);
    }

    private void OnToggleHandheldRadioSpeaker(Entity<RadioMicrophoneComponent> microphone, ref ToggleHandheldRadioSpeakerMessage args)
    {
        if (!args.Actor.Valid)
            return;

        SetSpeakerEnabled(microphone, args.Actor, args.Enabled, true);
        UpdateHandheldRadioUi(microphone);
    }

    private void OnChangeHandheldRadioFrequency(Entity<RadioMicrophoneComponent> microphone, ref SelectHandheldRadioFrequencyMessage args)
    {
        if (!args.Actor.Valid)
            return;

        if (args.Frequency >= MinRadioFrequency && args.Frequency <= MaxRadioFrequency)
            SetHandheldRadioFrequency(microphone, args.Frequency);
        UpdateHandheldRadioUi(microphone);
    }

    private void UpdateHandheldRadioUi(Entity<RadioMicrophoneComponent> radio)
    {
        var speakerComp = CompOrNull<RadioSpeakerComponent>(radio);
        var channel = _protoMan.Index<RadioChannelPrototype>(radio.Comp.BroadcastChannel);
        var frequency = GetHandheldRadioFrequency(radio, channel);

        var micEnabled = radio.Comp.Enabled;
        var speakerEnabled = speakerComp?.Enabled ?? false;
        var state = new HandheldRadioBoundUIState(micEnabled, speakerEnabled, frequency);
        if (TryComp<UserInterfaceComponent>(radio, out var uiComp))
            _ui.SetUiState((radio.Owner, uiComp), HandheldRadioUiKey.Key, state);
    }

    private int GetHandheldRadioFrequency(EntityUid uid, RadioChannelPrototype channel)
    {
        if (TryComp<HandheldRadioFrequencyComponent>(uid, out var radioFrequency)
            && radioFrequency.Frequency != 0)
        {
            return radioFrequency.Frequency;
        }

        return channel.Frequency;
    }

    private void SetHandheldRadioFrequency(EntityUid uid, int frequency)
    {
        EnsureComp<HandheldRadioFrequencyComponent>(uid).Frequency = frequency;
    }

    #endregion
    private void OnMapInit(EntityUid uid, IntercomComponent ent, MapInitEvent args)
    {
        if (ent.CurrentChannel != null &&
                _protoMan.TryIndex(ent.CurrentChannel, out var channel) &&
                HasComp<RadioMicrophoneComponent>(uid))
        {
            SetHandheldRadioFrequency(uid, channel.Frequency);
        }
    }
    // Pirate end - Handheld Radios port
}
