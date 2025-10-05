using System.Threading.Tasks;
using Content.Server._EinsteinEngines.Language;
using Content.Server.Chat.Systems;
using Content.Server.Communications;
using Content.Shared._CorvaxGoob.CCCVars;
using Content.Shared._CorvaxGoob.TTS;
using Content.Shared._EinsteinEngines.Language;
using Content.Shared.GameTicking;
using Content.Shared.Players.RateLimiting;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxGoob.TTS;

// ReSharper disable once InconsistentNaming
public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TTSManager _ttsManager = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly IRobustRandom _rng = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly LanguageSystem _lang = default!;

    private readonly List<string> _sampleText =
        new()
        {
            "Съешь же ещё этих мягких французских булок, да выпей чаю.",
            "Клоун, прекрати разбрасывать банановые кожурки офицерам под ноги!",
            "Капитан, вы уверены что хотите назначить клоуна на должность главы персонала?",
            "Эс Бэ! Тут человек в сером костюме, с тулбоксом и в маске! Помогите!!",
            "Учёные, тут странная аномалия в баре! Она уже съела мима!",
            "Я надеюсь что инженеры внимательно следят за сингулярностью...",
            "Вы слышали эти странные крики в техах? Мне кажется туда ходить небезопасно.",
            "Вы не видели Гамлета? Мне кажется он забегал к вам на кухню.",
            "Здесь есть доктор? Человек умирает от отравленного пончика! Нужна помощь!",
            "Вам нужно согласие и печать квартирмейстера, если вы хотите сделать заказ на партию дробовиков.",
            "Возле эвакуационного шаттла разгерметизация! Инженеры, нам срочно нужна ваша помощь!",
            "Бармен, налей мне самого крепкого вина, которое есть в твоих запасах!"
        };

    private const int MaxMessageChars = 100 * 2; // same as SingleBubbleCharLimit * 2
    private bool _isEnabled = false;

    public override void Initialize()
    {
        _cfg.OnValueChanged(CCCVars.TTSEnabled, v => _isEnabled = v, true);

        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<TTSComponent, EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        SubscribeNetworkEvent<RequestPreviewTTSEvent>(OnRequestPreviewTTS);
        SubscribeLocalEvent<CommunicationConsoleAnnouncementEvent>(OnConsoleAnnouncement);

        RegisterRateLimits();
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _ttsManager.ResetCache();
    }

    private async void OnRequestPreviewTTS(RequestPreviewTTSEvent ev, EntitySessionEventArgs args)
    {
        if (!_isEnabled ||
            !_prototypeManager.TryIndex<TTSVoicePrototype>(ev.VoiceId, out var protoVoice))
            return;

        if (HandleRateLimit(args.SenderSession) != RateLimitStatus.Allowed)
            return;

        var previewText = _rng.Pick(_sampleText);
        var soundData = await GenerateTTS(previewText, protoVoice.Speaker);
        if (soundData is null)
            return;

        RaiseNetworkEvent(new PlayTTSEvent(soundData), Filter.SinglePlayer(args.SenderSession));
    }

    private async void OnEntitySpoke(EntityUid uid, TTSComponent component, EntitySpokeEvent args)
    {
        var voiceId = component.VoicePrototypeId;
        if (!_isEnabled ||
            args.Message.Length > MaxMessageChars ||
            voiceId == null)
            return;

        var voiceEv = new TransformSpeakerVoiceEvent(uid, voiceId);
        RaiseLocalEvent(uid, voiceEv);
        voiceId = voiceEv.VoiceId;

        if (!_prototypeManager.TryIndex<TTSVoicePrototype>(voiceId, out var protoVoice))
            return;

        if (args.Message is null)
            return;

        var obfuscatedMessage = _lang.ObfuscateSpeech(args.Message, args.Language);

        if (!args.Language.SpeechOverride.RequireSpeech)
            return;

        if (args.IsWhisper)
            HandleWhisper(uid, args.Message, obfuscatedMessage, args.Language, protoVoice.Speaker);
        else
            HandleSay(uid, args.Message, obfuscatedMessage, args.Language, protoVoice.Speaker);
    }

    private async void HandleSay(EntityUid uid, string message, string obfMessage, LanguagePrototype language, string speaker)
    {
        var originalSoundData = await GenerateTTS(message, speaker);
        var obfuscatedSoundData = await GenerateTTS(obfMessage, speaker);

        foreach (var pvsSession in Filter.Pvs(uid).Recipients)
        {
            if (!pvsSession.AttachedEntity.HasValue)
                continue;

            if (obfuscatedSoundData is not null)
                if (HasComp<LanguageKnowledgeComponent>(pvsSession.AttachedEntity.Value))
                    if (!_lang.CanUnderstand(pvsSession.AttachedEntity.Value, language))
                    {
                        RaiseNetworkEvent(new PlayTTSEvent(obfuscatedSoundData, GetNetEntity(uid)), pvsSession.AttachedEntity.Value);
                        continue;
                    }

            if (originalSoundData is not null)
                RaiseNetworkEvent(new PlayTTSEvent(originalSoundData, GetNetEntity(uid)), pvsSession.AttachedEntity.Value);
        }
    }

    private async void HandleWhisper(EntityUid uid, string message, string obfMessage, LanguagePrototype language, string speaker)
    {
        var fullSoundData = await GenerateTTS(message, speaker, true);
        var obfSoundData = await GenerateTTS(obfMessage, speaker, true);

        if (obfSoundData is null && fullSoundData is null)
            return;

        // TODO: Check obstacles
        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourcePos = _xforms.GetWorldPosition(xformQuery.GetComponent(uid), xformQuery);
        var receptions = Filter.Pvs(uid).Recipients;
        foreach (var session in receptions)
        {
            if (!session.AttachedEntity.HasValue) continue;
            var xform = xformQuery.GetComponent(session.AttachedEntity.Value);
            var distance = (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).Length();
            if (distance > ChatSystem.WhisperClearRange)
                continue;

            var canUnderstand = _lang.CanUnderstand(session.AttachedEntity.Value, language);

            if (obfSoundData is not null)
                if (HasComp<LanguageKnowledgeComponent>(session.AttachedEntity.Value))
                    if (!_lang.CanUnderstand(session.AttachedEntity.Value, language))
                    {
                        RaiseNetworkEvent(new PlayTTSEvent(obfSoundData, GetNetEntity(uid), true), session);
                        continue;
                    }

            if (fullSoundData is not null)
                RaiseNetworkEvent(new PlayTTSEvent(fullSoundData, GetNetEntity(uid), true), session);
        }
    }

    // ReSharper disable once InconsistentNaming
    private async Task<byte[]?> GenerateTTS(string text, string speaker, bool isWhisper = false)
    {
        var textSanitized = Sanitize(text);
        if (textSanitized == "") return null;
        if (char.IsLetter(textSanitized[^1]))
            textSanitized += ".";

        var ssmlTraits = SoundTraits.RateFast;
        if (isWhisper)
            ssmlTraits = SoundTraits.PitchVerylow;
        var textSsml = ToSsmlText(textSanitized, ssmlTraits);

        return await _ttsManager.ConvertTextToSpeech(speaker, textSsml);
    }

    public void SendTTSAdminAnnouncement(string text, string voice, string announcementPath = ChatSystem.CentComAnnouncementSound)
    {
        if (_isPlaying)
            return;

        if (!_isEnabled ||
            text.Length > MaxMessageChars ||
            voice == "None")
            return;

        if (!_prototypeManager.TryIndex<TTSVoicePrototype>(voice, out var protoVoice))
            return;

        SendTTS(Filter.Broadcast(), text, protoVoice.Speaker, new SoundPathSpecifier(announcementPath));
    }
}
