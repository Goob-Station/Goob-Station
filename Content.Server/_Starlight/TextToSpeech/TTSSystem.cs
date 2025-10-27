using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Content.Server._Starlight.Radio.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Starlight.TextToSpeech;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Radio.Components;
using Content.Shared.Starlight;
using Content.Shared.Starlight.CCVar;
using Content.Shared.Starlight.TextToSpeech;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Starlight.TTS;

public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly RadioChimeSystem _chime = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ITTSManager _ttsManager = default!;
    [Dependency] private readonly IRobustRandom _rng = default!;

    private readonly List<string> _sampleText =
    [
        "Can someone bring me a pair of insulating gloves, please?",
        "Security, the clown has stolen the captain's ID!",
        "The singularity has reached the arrivals area!",
        "The robust salvagers have once again halted the nuclear operatives."
    ];

    private const int DefaultAnnounceVoice = 4;
    private const int MaxChars = 200;
    private const float WhisperVoiceVolumeModifier = 0.6f;
    private const int WhisperVoiceRange = 3;

    private readonly ISawmill _sawmill = Logger.GetSawmill("tts-system");
    private readonly List<ICommonSession> _ignoredRecipients = [];

    private bool _isEnabled;

    public override void Initialize()
    {
        _cfg.OnValueChanged(StarlightCCVars.TTSEnabled, v => _isEnabled = v, true);

        SubscribeNetworkEvent<PreviewTTSRequestEvent>(OnRequestPreviewTTS);
        SubscribeNetworkEvent<ClientOptionTTSEvent>(OnClientOptionTTS);
        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<TextToSpeechComponent, EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<RadioSpokeEvent>(OnRadioReceiveEvent);
        SubscribeLocalEvent<CollectiveMindSpokeEvent>(OnCollectiveMindReceiveEvent);
        SubscribeLocalEvent<AnnouncementSpokeEvent>(OnAnnouncementSpoke);
        _sawmill.Debug("Yeah it works");

        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        var uid = ev.Entity;
        // Ensure the body has the TTS component
        var comp = EnsureComp<TextToSpeechComponent>(uid);

        // If mind has voice metadata, copy it in (optional but useful)
        if (TryComp<MindContainerComponent>(uid, out var mindContainer) && mindContainer.HasMind &&
            TryComp<MindComponent>(mindContainer.Mind, out var mind))
        {
            // Mind.Voice & Mind.SiliconVoice are strings in Starlight; adjust naming if needed
            if (!string.IsNullOrEmpty(mind.Voice))
                comp.VoicePrototypeId = comp.VoicePrototypeId ?? mind.Voice;
            if (!string.IsNullOrEmpty(mind.SiliconVoice))
                comp.VoicePrototypeId = comp.VoicePrototypeId ?? mind.SiliconVoice;
        }

        Logger.Info($"[TTS] Attached/Ensured TTS on {ToPrettyString(uid)}; voice={comp.VoicePrototypeId}");
    }

    private async void OnRequestPreviewTTS(PreviewTTSRequestEvent ev, EntitySessionEventArgs args)
    {
        if (!_isEnabled ||
            !_prototypeManager.TryIndex<VoicePrototype>(ev.VoiceId, out var protoVoice))
            return;

        var previewText = _rng.Pick(_sampleText);
        var soundData = await GenerateTTS(previewText, protoVoice.Voice);
        if (soundData is null)
            return;

        RaiseNetworkEvent(new PlayTTSEvent { Data = soundData }, Robust.Shared.Player.Filter.SinglePlayer(args.SenderSession), false);
    }

    private async void OnClientOptionTTS(ClientOptionTTSEvent ev, EntitySessionEventArgs args)
    {
        if (ev.Enabled)
            _ignoredRecipients.Remove(args.SenderSession);
        else
            _ignoredRecipients.Add(args.SenderSession);
    }

    private void OnRadioReceiveEvent(RadioSpokeEvent args)
    {
        _sawmill.Debug("Radio event received");
        if (!_isEnabled
            || args.Message.Length > MaxChars)
            return;

        _chime.TryGetSenderHeadsetChime(args.Source, out var chime);

        if (!TryComp(args.Source, out TextToSpeechComponent? senderComponent)
            || senderComponent.VoicePrototypeId is not string voiceId)
        {
            HandleRadio(args.Receivers, args.Message, 4, chime);
        }
        else
        {
            var voice = _prototypeManager.TryIndex(voiceId, out VoicePrototype? proto) ? proto.Voice : 4;
            HandleRadio(args.Receivers, args.Message, voice, chime);
        }
    }

    private void OnCollectiveMindReceiveEvent(CollectiveMindSpokeEvent args)
    {
        if (!_isEnabled
            || args.Message.Length > MaxChars)
            return;

        if (!TryComp(args.Source, out TextToSpeechComponent? senderComponent)
            || senderComponent.VoicePrototypeId is not string voiceId)
        {
            HandleCollectiveMind(args.Receivers, args.Message, 4);
        }
        else
        {
            var voice = _prototypeManager.TryIndex(voiceId, out VoicePrototype? proto) ? proto.Voice : 4;
            HandleCollectiveMind(args.Receivers, args.Message, voice);
        }
    }

    private async void OnAnnouncementSpoke(AnnouncementSpokeEvent args)
    {
        _sawmill.Warning("Announcement Success");
        if (!_isEnabled
            || args.Message.Length > MaxChars * 2)
            return;

        var voice = _prototypeManager.TryIndex(args.AnnounceVoice ?? "", out VoicePrototype? proto)
            ? proto.Voice
            : DefaultAnnounceVoice;

        var soundData = await GenerateTTS(args.Message, voice, isAnnounce: true);
        soundData ??= [];
        RaiseNetworkEvent(new AnnounceTtsEvent
        {
            Data = soundData,
            AnnouncementSound = args.AnnouncementSound
        }, args.Source.RemovePlayers(_ignoredRecipients), false);
    }

    private async void OnEntitySpoke(EntityUid uid, TextToSpeechComponent component, EntitySpokeEvent args)
    {
        _sawmill.Debug("Fired Entity Speak"); // For whatever reason, this won't fire off. Investigate this
        if (!_isEnabled || args.Message.Length > MaxChars) return;
        var voice = DefaultAnnounceVoice;
        if (!_prototypeManager.TryIndex(component.VoicePrototypeId ?? "", out VoicePrototype? proto))
        {
            var voices = _prototypeManager.TryGetInstances<VoicePrototype>(out var v) ? v.AsEnumerable() : [];
            if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoidAppearanceComponent)
                && humanoidAppearanceComponent?.Sex is Sex sex)
            {
                var voicePrototypes = voices.Where(x => !x.Value.Silicon
                    && (x.Value.Sex == Sex.Unsexed || sex == Sex.Unsexed || x.Value.Sex == sex)).ToArray();
                if (voicePrototypes.Length != 0)
                {
                    var index = Random.Shared.Next(voicePrototypes.Length);
                    if (TryComp<MindContainerComponent>(uid, out var mindContainer) && mindContainer.HasMind
                    && TryComp<MindComponent>(mindContainer.Mind, out var mind))
                    {
                        for(int i = 0; i < voicePrototypes.Length; i++)
                        {
                            if(voicePrototypes[i].Value.Name == mind.Voice)
                            {
                                index = i;
                                break;
                            }
                        }
                    }
                    var prototype = voicePrototypes[index];
                    voice = prototype.Value.Voice;
                    component.VoicePrototypeId = prototype.Value.ID;
                }
            }
            else
            {
                var voicePrototypes = voices.Where(x => x.Value.Silicon).ToArray();
                if (voicePrototypes.Length != 0)
                {
                    var index = Random.Shared.Next(voicePrototypes.Length);
                    if (TryComp<MindContainerComponent>(uid, out var mindContainer) && mindContainer.HasMind
                    && TryComp<MindComponent>(mindContainer.Mind, out var mind))
                    {
                        for(int i = 0; i < voicePrototypes.Length; i++)
                        {
                            if(voicePrototypes[i].Value.Name == mind.SiliconVoice)
                            {
                                index = i;
                                break;
                            }
                        }
                    }
                    var prototype = voicePrototypes[index];
                    voice = prototype.Value.Voice;
                    component.VoicePrototypeId = prototype.Value.ID;
                }
            }
        }
        else
            voice = proto.Voice;

        if (args.IsWhisper == true)
        {
            HandleWhisper(uid, args.Message, voice);
            return;
        }
        HandleSay(uid, args.Message, voice);
    }
    private void OnTransformSpeech(TransformSpeechEvent args)
    {
        if (!_isEnabled) return;
        args.Message = args.Message.Replace("+", "");
    }
    private async void HandleSay(EntityUid uid, string message, int voice)
    {
        var recipients = Robust.Shared.Player.Filter.Pvs(uid, 1F).RemovePlayers(_ignoredRecipients);

        var soundData = await GenerateTTS(message, voice);

        if (soundData is null)
            return;

        var netEntity = GetNetEntity(uid);

        if (TryComp<EyeComponent>(uid, out var eye) && eye is not null)
        {
            recipients.RemovePlayerByAttachedEntity(uid);
            RaiseNetworkEvent(new PlayTTSEvent
            {
                Data = soundData,
                SourceUid = GetNetEntity(eye.Target)
            }, Filter.Empty().FromEntities(uid), false);
        }

        RaiseNetworkEvent(new PlayTTSEvent
        {
            Data = soundData,
            SourceUid = netEntity
        }, recipients, false);
    }

    private async void HandleWhisper(EntityUid uid, string message, int voice)
    {
        var soundData = await GenerateTTS(message, voice);
        if (soundData is null)
            return;

        var transformQuery = GetEntityQuery<TransformComponent>();
        var sourcePos = _xforms.GetWorldPosition(transformQuery.GetComponent(uid), transformQuery);
        var receptions = Filter.Pvs(uid).Recipients;
        foreach (var session in receptions)
        {
            if (!session.AttachedEntity.HasValue
                || _ignoredRecipients.Contains(session))
                continue;

            var transform = transformQuery.GetComponent(session.AttachedEntity.Value);
            var distance = (sourcePos - _xforms.GetWorldPosition(transform, transformQuery)).LengthSquared();

            if (distance > WhisperVoiceRange)
                continue;

            if (session.AttachedEntity == uid && TryComp<EyeComponent>(uid, out var eye) && eye is not null)
            {
                RaiseNetworkEvent(new PlayTTSEvent
                {
                    Data = soundData,
                    SourceUid = GetNetEntity(eye.Target)
                }, Filter.Empty().FromEntities(uid), false);
            }
            else
            {
                RaiseNetworkEvent(new PlayTTSEvent
                {
                    Data = soundData,
                    SourceUid = GetNetEntity(uid),
                    VolumeModifier = WhisperVoiceVolumeModifier * (1f - distance / WhisperVoiceRange)
                }, session);
            }
        }
    }

    private async void HandleRadio(EntityUid[] uIds, string message, int voice, SoundSpecifier? chime)
    {
        var soundData = await GenerateTTS(message, voice, isRadio: true);
        if (soundData is null)
            return;

        RaiseNetworkEvent(new PlayTTSEvent { IsRadio = true, Chime = chime, Data = soundData }, Filter.Entities(uIds).RemovePlayers(_ignoredRecipients), false);
    }

    private async void HandleCollectiveMind(EntityUid[] uIds, string message, int voice)
    {
        var soundData = await GenerateTTS(message, voice, isRadio: true);
        if (soundData is null)
            return;

        RaiseNetworkEvent(new PlayTTSEvent { IsRadio = true, Data = soundData }, Filter.Entities(uIds).RemovePlayers(_ignoredRecipients), false);
    }

    private async Task<byte[]?> GenerateTTS(string text, int voice, bool isRadio = false, bool isAnnounce = false)
    {
        try
        {
            text = DecimalConverter().Replace(text, " point ");
            text = Number2Word().Replace(text, ReplaceNumber2Word);
            text = SymbolFilter().Replace(text, ReplaceAbbreviations);
            text = CharFilter().Replace(text.Trim(), "");

            if (text == "") return null;
            if (char.IsLetter(text[^1]))
                text += ".";

            // return isRadio
            //     ? await _ttsManager.ConvertTextToSpeechRadio(voice, text)
            //     : isAnnounce
            //         ? await _ttsManager.ConvertTextToSpeechAnnounce(voice, text)
            //         : await _ttsManager.ConvertTextToSpeechStandard(voice, text);

            if (!_prototypeManager.TryGetInstances<VoicePrototype>(out var voices))
            {
                _sawmill.Error("No voice prototypes found!");
                return null;
            }

            // Find matching protoype by "voice:" index from voice.yml
            var proto = voices.Values.FirstOrDefault(x => x.Voice == voice);
            if (proto == null)
            {
                _sawmill.Error($"No matching voice prototype for voice #{voice}");
                return null;
            }

            var elevenId = proto.ID;
            _sawmill.Debug($"Resolved ElevenLabs ID: {elevenId}");

            // Pass the ID instead of number
            if (isRadio)
                return await _ttsManager.ConvertTextToSpeechRadio(elevenId, text);

            if (isAnnounce)
                return await _ttsManager.ConvertTextToSpeechAnnounce(elevenId, text);

            return await _ttsManager.ConvertTextToSpeechStandard(elevenId, text);
        }
        catch (Exception e)
        {
            _sawmill.Error($"TTS System error: {e.Message}");
        }

        return null;
    }
    private string ReplaceNumber2Word(Match word)
        => !long.TryParse(word.Value, out var number) ? word.Value : NumberConverter.NumberToText(number);
    private string ReplaceAbbreviations(Match word)
        => _wordReplacement.TryGetValue(word.Value.ToLower(), out var replace) ? replace : word.Value;

    private static readonly IReadOnlyDictionary<string, string> _wordReplacement =
        new Dictionary<string, string>()
        {
            {"id", "Ai Di"},
            {"pda", "PiDiA"},
            {"sci", "sai"},

            //owo
            {"(•`ω´•)", "meow"},
            {";;w;;", "meow"},
            {"owo", "meow"},
            {"UwU", "meow"},
            {">w<", "meow"},
            {"^w^", "meow"},

            //russian
            {"Д", "A"},
            {"в", "b"},
            {"И", "N"},
            {"и", "n"},
            {"К", "K"},
            {"к", "k"},
            {"м", "m"},
            {"н", "h"},
            {"т", "t"},
            {"Я", "R"},
            {"я", "r"},
            {"У", "Y"},
            {"Ш", "W"},
            {"ш", "w"},
        };

    [GeneratedRegex(@"[^a-zA-Z0-9,\-+?!. ]")]
    private static partial Regex CharFilter();

    [GeneratedRegex(@"(?<=\d)[.,](?=\d)")]
    private static partial Regex DecimalConverter();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Number2Word();

    [GeneratedRegex(@"(?<![a-zA-Zа-яёА-ЯЁ0-9])([a-zA-Zа-яёА-ЯЁ]+|(\(•`ω´•\)|;;w;;|owo|UwU|>w<|\^w\^))(?![a-zA-Zа-яёА-ЯЁ0-9])", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-US")]
    private static partial Regex SymbolFilter();
}
