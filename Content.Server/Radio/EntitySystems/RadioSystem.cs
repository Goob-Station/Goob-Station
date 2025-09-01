// SPDX-FileCopyrightText: 2020 Bright <nsmoak10@yahoo.com>
// SPDX-FileCopyrightText: 2020 Bright0 <55061890+Bright0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Swept <jamesurquhartwebb@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 adamsong <adamsong@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SkaldetSkaeg <impotekh@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 CerberusWolfie <wb.johnb.willis@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 John Willis <143434770+CerberusWolfie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server._EinsteinEngines.Language;
using Content.Server.Power.Components;
using Content.Server.Radio.Components;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared._EinsteinEngines.Language;
using Content.Shared._EinsteinEngines.Language.Systems;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Speech;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Replays;
using Robust.Shared.Utility;
using Content.Shared.Whitelist;

namespace Content.Server.Radio.EntitySystems;

/// <summary>
///     This system handles intrinsic radios and the general process of converting radio messages into chat messages.
/// </summary>
public sealed class RadioSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly IReplayRecordingManager _replay = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly LanguageSystem _language = default!; // Einstein Engines - Language
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!; // Goobstation - Whitelisted radio channels

    // set used to prevent radio feedback loops.
    private readonly HashSet<string> _messages = new();

    private EntityQuery<TelecomExemptComponent> _exemptQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IntrinsicRadioReceiverComponent, RadioReceiveEvent>(OnIntrinsicReceive);
        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, EntitySpokeEvent>(OnIntrinsicSpeak);
        SubscribeLocalEvent<IntrinsicRadioReceiverComponent, RadioReceiveAttemptEvent>(OnIntrinsicReceiveAttempt); // Goobstation

        _exemptQuery = GetEntityQuery<TelecomExemptComponent>();
    }

    private void OnIntrinsicSpeak(EntityUid uid, IntrinsicRadioTransmitterComponent component, EntitySpokeEvent args)
    {
        if (args.Channel != null
            && component.Channels.Contains(args.Channel.ID)
            && _whitelist.IsWhitelistPassOrNull(args.Channel.SendWhitelist, uid)) // Goobstation - Whitelisted radio channels
        {
            SendRadioMessage(uid, args.Message, args.Channel, uid, args.Language); // Einstein Engines - Language
            args.Channel = null; // prevent duplicate messages from other listeners.
        }
    }

    private void OnIntrinsicReceive(EntityUid uid, IntrinsicRadioReceiverComponent component, ref RadioReceiveEvent args)
    {
        if (TryComp(uid, out ActorComponent? actor))
        {
            // Einstein Engines - Languages begin
            var listener = component.Owner;
            var msg = args.OriginalChatMsg;

            if (listener != null && !_language.CanUnderstand(listener, args.Language.ID))
                msg = args.LanguageObfuscatedChatMsg;

            _netMan.ServerSendMessage(new MsgChatMessage { Message = msg }, actor.PlayerSession.Channel);
            // Einstein Engines - Languages end
        }
    }

    // Goobstation - Whitelisted radio channels
    private void OnIntrinsicReceiveAttempt(EntityUid uid, IntrinsicRadioReceiverComponent component, ref RadioReceiveAttemptEvent args)
    {
        args.Cancelled = _whitelist.IsWhitelistFail(args.Channel.ReceiveWhitelist, uid);
    }

    /// <summary>
    /// Send radio message to all active radio listeners
    /// </summary>
    public void SendRadioMessage(
        EntityUid messageSource,
        string message,
        ProtoId<RadioChannelPrototype> channel,
        EntityUid radioSource,
        LanguagePrototype? language = null,
        bool escapeMarkup = true)
    {
        SendRadioMessage(messageSource, message, _prototype.Index(channel), radioSource, escapeMarkup: escapeMarkup, language: language); // Einstein Engines - Language
    }

    /// <summary>
    /// Send radio message to all active radio listeners
    /// </summary>
    /// <param name="messageSource">Entity that spoke the message</param>
    /// <param name="radioSource">Entity that picked up the message and will send it, e.g. headset</param>
    public void SendRadioMessage(
        EntityUid messageSource,
        string message,
        RadioChannelPrototype channel,
        EntityUid radioSource,
        LanguagePrototype? language = null,
        bool escapeMarkup = true)
    {
        // Einstein Engines - Language begin
        if (language == null)
            language = _language.GetLanguage(messageSource);

        if (!language.SpeechOverride.AllowRadio)
            return;
        // Einstein Engines - Language end

        // TODO if radios ever garble / modify messages, feedback-prevention needs to be handled better than this.
        if (!_messages.Add(message))
            return;

        var evt = new TransformSpeakerNameEvent(messageSource, MetaData(messageSource).EntityName);
        RaiseLocalEvent(messageSource, evt);

        var name = evt.VoiceName;
        name = FormattedMessage.EscapeText(name);

        SpeechVerbPrototype speech;
        if (evt.SpeechVerb != null && _prototype.TryIndex(evt.SpeechVerb, out var evntProto))
            speech = evntProto;
        else
            speech = _chat.GetSpeechVerb(messageSource, message);

        var content = escapeMarkup
            ? FormattedMessage.EscapeText(message)
            : message;

        // var wrappedMessage = Loc.GetString(speech.Bold ? "chat-radio-message-wrap-bold" : "chat-radio-message-wrap",
        //     ("color", channel.Color),
        //     ("fontType", speech.FontId),
        //     ("fontSize", speech.FontSize),
        //     ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
        //     ("channel", $"\\[{channel.LocalizedName}\\]"),
        //     ("name", name),
        //     ("message", content));
        var wrappedMessage = WrapRadioMessage(messageSource, channel, name, content, language); // Einstein Engines - Language

        // most radios are relayed to chat, so lets parse the chat message beforehand
        // var chat = new ChatMessage(
        //     ChatChannel.Radio,
        //     message,
        //     wrappedMessage,
        //     NetEntity.Invalid,
        //     null);
        // var chatMsg = new MsgChatMessage { Message = chat };
        // var ev = new RadioReceiveEvent(message, messageSource, channel, radioSource, chatMsg);
        var msg = new ChatMessage(ChatChannel.Radio, content, wrappedMessage, NetEntity.Invalid, null); // Einstein Engines - Language

        // Einstein Engines - Language begin
        var obfuscated = _language.ObfuscateSpeech(content, language);
        var obfuscatedWrapped = WrapRadioMessage(messageSource, channel, name, obfuscated, language);
        var notUdsMsg = new ChatMessage(ChatChannel.Radio, obfuscated, obfuscatedWrapped, NetEntity.Invalid, null);
        var ev = new RadioReceiveEvent(messageSource, channel, msg, notUdsMsg, language, radioSource);
        // Einstein Engines - Language end

        var sendAttemptEv = new RadioSendAttemptEvent(channel, radioSource);
        RaiseLocalEvent(ref sendAttemptEv);
        RaiseLocalEvent(radioSource, ref sendAttemptEv);
        var canSend = !sendAttemptEv.Cancelled;

        var sourceMapId = Transform(radioSource).MapID;
        var hasActiveServer = HasActiveServer(sourceMapId, channel.ID);
        var sourceServerExempt = _exemptQuery.HasComp(radioSource);

        var radioQuery = EntityQueryEnumerator<ActiveRadioComponent, TransformComponent>();
        while (canSend && radioQuery.MoveNext(out var receiver, out var radio, out var transform))
        {
            if (!radio.ReceiveAllChannels)
            {
                if (!radio.Channels.Contains(channel.ID) || (TryComp<IntercomComponent>(receiver, out var intercom) &&
                                                             !intercom.SupportedChannels.Contains(channel.ID)))
                    continue;
            }

            if (!channel.LongRange && transform.MapID != sourceMapId && !radio.GlobalReceive)
                continue;

            // don't need telecom server for long range channels or handheld radios and intercoms
            var needServer = !channel.LongRange && !sourceServerExempt;
            if (needServer && !hasActiveServer)
                continue;

            // check if message can be sent to specific receiver
            var attemptEv = new RadioReceiveAttemptEvent(channel, radioSource, receiver);
            RaiseLocalEvent(ref attemptEv);
            RaiseLocalEvent(receiver, ref attemptEv);
            if (attemptEv.Cancelled)
                continue;

            // send the message
            RaiseLocalEvent(receiver, ref ev);
        }

        if (name != Name(messageSource))
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Radio message from {ToPrettyString(messageSource):user} as {name} on {channel.LocalizedName}: {message}");
        else
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Radio message from {ToPrettyString(messageSource):user} on {channel.LocalizedName}: {message}");

        _replay.RecordServerMessage(msg); // Einstein Engines - Language
        _messages.Remove(message);
    }

    // Einstein Engines - Language begin
    private string WrapRadioMessage(
        EntityUid source,
        RadioChannelPrototype channel,
        string name,
        string message,
        LanguagePrototype language)
    {
        // TODO: code duplication with ChatSystem.WrapMessage
        var speech = _chat.GetSpeechVerb(source, message);
        var languageColor = channel.Color;

        // Goobstation - Bolded Language Overrides begin
        var wrapId = speech.Bold ? "chat-radio-message-wrap-bold" : "chat-radio-message-wrap";
        if (speech.Bold && language.SpeechOverride.BoldFontId != null)
            wrapId = "chat-radio-message-wrap-bolded-language";
        // Goobstation end

        if (language.SpeechOverride.Color is { } colorOverride)
            languageColor = Color.InterpolateBetween(Color.White, colorOverride, colorOverride.A); // Changed first param to Color.White so it shows color correctly.

        var languageDisplay = language.IsVisibleLanguage
            ? Loc.GetString("chat-manager-language-prefix", ("language", language.ChatName))
            : "";

        return Loc.GetString(wrapId,
            ("color", channel.Color),
            ("languageColor", languageColor),
            ("fontType", language.SpeechOverride.FontId ?? speech.FontId),
            ("fontSize", language.SpeechOverride.FontSize ?? speech.FontSize),
            ("boldFontType", language.SpeechOverride.BoldFontId ?? language.SpeechOverride.FontId ?? speech.FontId), // Goob Edit - Custom Bold Fonts
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("channel", $"\\[{channel.LocalizedName}\\]"),
            ("name", name),
            ("message", message),
            ("language", languageDisplay));
    }
    // Einstein Engines - Language end

    /// <inheritdoc cref="TelecomServerComponent"/>
    private bool HasActiveServer(MapId mapId, string channelId)
    {
        var servers = EntityQuery<TelecomServerComponent, EncryptionKeyHolderComponent, ApcPowerReceiverComponent, TransformComponent>();
        foreach (var (_, keys, power, transform) in servers)
        {
            if (transform.MapID == mapId &&
                power.Powered &&
                keys.Channels.Contains(channelId))
            {
                return true;
            }
        }
        return false;
    }
}
