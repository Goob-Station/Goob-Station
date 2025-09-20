// SPDX-FileCopyrightText: 2023 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 HerCoyote23 <131214189+HerCoyote23@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Verm <32827189+Vermidia@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 geraeumig <171753363+geraeumig@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 geraeumig <alfenos@proton.me>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 CerberusWolfie <wb.johnb.willis@gmail.com>
// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 John Willis <143434770+CerberusWolfie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 lzk <124214523+lzk228@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Frozen;
using Content.Goobstation.Common.MisandryBox;
using Content.Shared.Chat; // Einstein Engines - Languages & Goobmod
using Content.Server.Popups;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Emoting;
using Content.Shared.Speech;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Chat.Systems;

// emotes using emote prototype
public partial class ChatSystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    private FrozenDictionary<string, EmotePrototype> _wordEmoteDict = FrozenDictionary<string, EmotePrototype>.Empty;

    protected override void OnPrototypeReload(PrototypesReloadedEventArgs obj)
    {
        base.OnPrototypeReload(obj);
        if (obj.WasModified<EmotePrototype>())
            CacheEmotes();
    }

    private void CacheEmotes()
    {
        var dict = new Dictionary<string, EmotePrototype>();
        var emotes = _prototypeManager.EnumeratePrototypes<EmotePrototype>();
        foreach (var emote in emotes)
        {
            foreach (var word in emote.ChatTriggers)
            {
                var lowerWord = word.ToLower();
                if (dict.TryGetValue(lowerWord, out var value))
                {
                    var errMsg = $"Duplicate of emote word {lowerWord} in emotes {emote.ID} and {value.ID}";
                    Log.Error(errMsg);
                    continue;
                }

                dict.Add(lowerWord, emote);
            }
        }

        _wordEmoteDict = dict.ToFrozenDictionary();
    }

    /// <summary>
    ///     Makes selected entity to emote using <see cref="EmotePrototype"/> and sends message to chat.
    /// </summary>
    /// <param name="source">The entity that is speaking</param>
    /// <param name="emoteId">The id of emote prototype. Should has valid <see cref="EmotePrototype.ChatMessages"/></param>
    /// <param name="hideLog">Whether or not this message should appear in the adminlog window</param>
    /// <param name="range">Conceptual range of transmission, if it shows in the chat window, if it shows to far-away ghosts or ghosts at all...</param>
    /// <param name="nameOverride">The name to use for the speaking entity. Usually this should just be modified via <see cref="TransformSpeakerNameEvent"/>. If this is set, the event will not get raised.</param>
    /// <param name="forceEmote">Bypasses whitelist/blacklist/availibility checks for if the entity can use this emote</param>
    /// <returns>True if an emote was performed. False if the emote is unvailable, cancelled, etc.</returns>
    public bool TryEmoteWithChat(
        EntityUid source,
        string emoteId,
        ChatTransmitRange range = ChatTransmitRange.Normal,
        bool hideLog = false,
        string? nameOverride = null,
        bool ignoreActionBlocker = false,
        bool forceEmote = false,
        bool voluntary = false
        )
    {
        if (!_prototypeManager.TryIndex<EmotePrototype>(emoteId, out var proto))
            return false;
        return TryEmoteWithChat(source, proto, range, hideLog: hideLog, nameOverride, ignoreActionBlocker: ignoreActionBlocker, forceEmote: forceEmote, voluntary: voluntary);
    }

    /// <summary>
    ///     Makes selected entity to emote using <see cref="EmotePrototype"/> and sends message to chat.
    /// </summary>
    /// <param name="source">The entity that is speaking</param>
    /// <param name="emote">The emote prototype. Should has valid <see cref="EmotePrototype.ChatMessages"/></param>
    /// <param name="hideLog">Whether or not this message should appear in the adminlog window</param>
    /// <param name="hideChat">Whether or not this message should appear in the chat window</param>
    /// <param name="range">Conceptual range of transmission, if it shows in the chat window, if it shows to far-away ghosts or ghosts at all...</param>
    /// <param name="nameOverride">The name to use for the speaking entity. Usually this should just be modified via <see cref="TransformSpeakerNameEvent"/>. If this is set, the event will not get raised.</param>
    /// <param name="forceEmote">Bypasses whitelist/blacklist/availibility checks for if the entity can use this emote</param>
    /// <returns>True if an emote was performed. False if the emote is unvailable, cancelled, etc.</returns>
    public bool TryEmoteWithChat(
        EntityUid source,
        EmotePrototype emote,
        ChatTransmitRange range = ChatTransmitRange.Normal,
        bool hideLog = false,
        string? nameOverride = null,
        bool ignoreActionBlocker = false,
        bool forceEmote = false,
        bool voluntary = false // Goob - emotespam
        )
    {
        if (!forceEmote && !AllowedToUseEmote(source, emote))
            return false;

        var didEmote = TryEmoteWithoutChat(source, emote, ignoreActionBlocker, voluntary: voluntary); // Goob - emotespam

        // check if proto has valid message for chat
        if (didEmote && emote.ChatMessages.Count != 0)
        {
            // not all emotes are loc'd, but for the ones that are we pass in entity
            var action = Loc.GetString(_random.Pick(emote.ChatMessages), ("entity", source));
            var language = _language.GetLanguage(source); // Einstein Engines - Language
            SendEntityEmote(source, action, range, nameOverride, language, hideLog: hideLog, checkEmote: false, ignoreActionBlocker: ignoreActionBlocker); // Einstein Engines - Language
        }

        return didEmote;
    }

    /// <summary>
    ///     Makes selected entity to emote using <see cref="EmotePrototype"/> without sending any messages to chat.
    /// </summary>
    /// <returns>True if an emote was performed. False if the emote is unvailable, cancelled, etc.</returns>
    public bool TryEmoteWithoutChat(EntityUid uid, string emoteId, bool ignoreActionBlocker = false, bool voluntary = false) // Goob - emotespam
    {
        if (!_prototypeManager.TryIndex<EmotePrototype>(emoteId, out var proto))
            return false;

        return TryEmoteWithoutChat(uid, proto, ignoreActionBlocker, voluntary); // Goob - emotespam
    }

    /// <summary>
    ///     Makes selected entity to emote using <see cref="EmotePrototype"/> without sending any messages to chat.
    /// </summary>
    /// <returns>True if an emote was performed. False if the emote is unvailable, cancelled, etc.</returns>
    public bool TryEmoteWithoutChat(EntityUid uid, EmotePrototype proto, bool ignoreActionBlocker = false, bool voluntary = false) // Goob - emotespam
    {
        if (!_actionBlocker.CanEmote(uid) && !ignoreActionBlocker)
            return false;

        return TryInvokeEmoteEvent(uid, proto, voluntary); // Goob - emotespam
    }

    /// <summary>
    ///     Tries to find and play relevant emote sound in emote sounds collection.
    /// </summary>
    /// <returns>True if emote sound was played.</returns>
    public bool TryPlayEmoteSound(EntityUid uid, EmoteSoundsPrototype? proto, EmotePrototype emote, AudioParams? audioParams = null)
    {
        return TryPlayEmoteSound(uid, proto, emote.ID, audioParams);
    }

    /// <summary>
    ///     Tries to find and play relevant emote sound in emote sounds collection.
    /// </summary>
    /// <returns>True if emote sound was played.</returns>
    public bool TryPlayEmoteSound(EntityUid uid, EmoteSoundsPrototype? proto, string emoteId, AudioParams? audioParams = null)
    {
        if (proto == null)
            return false;

        // try to get specific sound for this emote
        if (!proto.Sounds.TryGetValue(emoteId, out var sound))
        {
            // no specific sound - check fallback
            sound = proto.FallbackSound;
            if (sound == null)
                return false;
        }

        // optional override params > general params for all sounds in set > individual sound params
        var param = audioParams ?? proto.GeneralParams ?? sound.Params;

        // Goobstation/MisandryBox - Emote spam countermeasures
        var ev = new EmoteSoundPitchShiftEvent();
        RaiseLocalEvent(uid, ref ev);

        param.Pitch += ev.Pitch;
        // Goobstation/MisandryBox

        _audio.PlayPvs(sound, uid, param);
        return true;
    }
    /// <summary>
    /// Checks if a valid emote was typed, to play sounds and etc and invokes an event.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="textInput"></param>
    /// <returns>True if the chat message should be displayed (because the emote was explicitly cancelled), false if it should not be.</returns>
    private bool TryEmoteChatInput(EntityUid uid, string textInput)
    {
        var actionTrimmedLower = TrimPunctuation(textInput.ToLower());
        if (!_wordEmoteDict.TryGetValue(actionTrimmedLower, out var emote))
            return true;

        if (!AllowedToUseEmote(uid, emote))
            return true;

        return TryInvokeEmoteEvent(uid, emote, voluntary: true); // Goob - emotespam

        static string TrimPunctuation(string textInput)
        {
            var trimEnd = textInput.Length;
            while (trimEnd > 0 && char.IsPunctuation(textInput[trimEnd - 1]))
            {
                trimEnd--;
            }

            var trimStart = 0;
            while (trimStart < trimEnd && char.IsPunctuation(textInput[trimStart]))
            {
                trimStart++;
            }

            return textInput[trimStart..trimEnd];
        }
    }
    /// <summary>
    /// Checks if we can use this emote based on the emotes whitelist, blacklist, and availibility to the entity.
    /// </summary>
    /// <param name="source">The entity that is speaking</param>
    /// <param name="emote">The emote being used</param>
    /// <returns></returns>
    private bool AllowedToUseEmote(EntityUid source, EmotePrototype emote)
    {
        // If emote is in AllowedEmotes, it will bypass whitelist and blacklist
        if (TryComp<SpeechComponent>(source, out var speech) &&
            speech.AllowedEmotes.Contains(emote.ID))
        {
            return true;
        }

        // Check the whitelist and blacklist
        if (_whitelistSystem.IsWhitelistFail(emote.Whitelist, source) ||
            _whitelistSystem.IsBlacklistPass(emote.Blacklist, source))
        {
            return false;
        }

        // Check if the emote is available for all
        if (!emote.Available)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates and raises <see cref="BeforeEmoteEvent"/> and then <see cref="EmoteEvent"/> to let other systems do things like play audio.
    /// In the case that the Before event is cancelled, EmoteEvent will NOT be raised, and will optionally show a message to the player
    /// explaining why the emote didn't happen.
    /// </summary>
    /// <param name="uid">The entity which is emoting</param>
    /// <param name="proto">The emote which is being performed</param>
    /// <returns>True if the emote was performed, false otherwise.</returns>
    private bool TryInvokeEmoteEvent(EntityUid uid, EmotePrototype proto, bool voluntary = false)
    {
        var beforeEv = new BeforeEmoteEvent(uid, proto);
        RaiseLocalEvent(uid, ref beforeEv);

        if (beforeEv.Cancelled)
        {
            if (beforeEv.Blocker != null)
            {
                _popupSystem.PopupEntity(
                    Loc.GetString(
                        "chat-system-emote-cancelled-blocked",
                        ("emote", Loc.GetString(proto.Name).ToLower()),
                        ("blocker", beforeEv.Blocker.Value)
                    ),
                    uid,
                    uid
                );
            }
            else
            {
                _popupSystem.PopupEntity(
                    Loc.GetString("chat-system-emote-cancelled-generic",
                        ("emote", Loc.GetString(proto.Name).ToLower())),
                    uid,
                    uid
                );
            }

            return false;
        }

        var ev = new EmoteEvent(proto, voluntary);
        RaiseLocalEvent(uid, ref ev);

        return true;
    }
}

/// <summary>
///     Raised by chat system when entity made some emote.
///     Use it to play sound, change sprite or something else.
/// </summary>
[ByRefEvent]
public sealed class EmoteEvent : HandledEntityEventArgs
{
    public readonly EmotePrototype Emote;
    public bool Voluntary; // Goob - emotespam

    public EmoteEvent(EmotePrototype emote, bool voluntary = true) // Goob - emotespam
    {
        Emote = emote;
        Handled = false;
        Voluntary = voluntary; // Goob - emotespam
    }
}
