using System.Threading.Tasks;
using Content.Server.Chat.Systems;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.TTS;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using TTSComponent = Content.Shared.TTS.TTSComponent;

namespace Content.Server.TTS;

// ReSharper disable once InconsistentNaming
public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TTSManager _ttsManager = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly IRobustRandom _rng = default!;

    private readonly List<string> _sampleText = new()
    {
        "Hello station, I have teleported the janitor.",
        "Yes, Ms. Sarah, about the theater issue -- will Engineering be dealing with it?",
        "Since Samuel was detained should we change it to a code green?",
        "He wants to do an interview, where are you?",
        "Samuel Rodriguez broke the door to the bridge with an e-mag!",
        "I want to give credit where it's due -- the newspaper is working, and it's doing quite well. I like it.",
        "Praise and glory from NT.",
        "Will someone build a podium in the theater?",
        "Clown, I'm about to be interviewed, I'll be gone about 10 minutes.",
        "Chief, I'm about to be interviewed, I'll be gone for about 10 minutes.",
        "As far as I understand, the anomaly broke the barrier between the Singularity and the station.",
    };

    private const int MaxMessageChars = 100 * 2; // Same as SingleBubbleCharLimit * 2
    private bool _isEnabled = true;

    public override void Initialize()
    {
        _cfg.OnValueChanged(CCVars.TTSEnabled, v => _isEnabled = v, true);

        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<TTSComponent, EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        SubscribeNetworkEvent<RequestPreviewTTSEvent>(OnRequestPreviewTTS);
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

        var previewText = _rng.Pick(_sampleText);
        var soundData = await GenerateTTS(previewText, protoVoice.Model, protoVoice.Speaker);
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

        if (args.IsWhisper)
        {
            HandleWhisper(uid, args.Message, protoVoice.Model, protoVoice.Speaker);
            return;
        }

        HandleSay(uid, args.Message, protoVoice.Model, protoVoice.Speaker);
    }

    private async void HandleSay(EntityUid uid, string message, string model, string speaker)
    {
        var soundData = await GenerateTTS(message, model, speaker);
        if (soundData is null)
            return;
        RaiseNetworkEvent(new PlayTTSEvent(soundData, GetNetEntity(uid)), Filter.Pvs(uid));
    }

    private async void HandleWhisper(EntityUid uid, string message, string model, string speaker)
    {
        var fullSoundData = await GenerateTTS(message, model, speaker, true);
        if (fullSoundData is null)
            return;

        var fullTtsEvent = new PlayTTSEvent(fullSoundData, GetNetEntity(uid), true);

        // TODO: Check obstacles
        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourcePos = _xforms.GetWorldPosition(xformQuery.GetComponent(uid), xformQuery);
        var receptions = Filter.Pvs(uid).Recipients;
        foreach (var session in receptions)
        {
            if (!session.AttachedEntity.HasValue)
                continue;
            var xform = xformQuery.GetComponent(session.AttachedEntity.Value);
            var distance = (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).Length();
            if (distance > 10 * 10)
                continue;

            RaiseNetworkEvent(fullTtsEvent, session);
        }
    }

    // ReSharper disable once InconsistentNaming
    private async Task<byte[]?> GenerateTTS(string text, string model, string speaker, bool isWhisper = false)
    {
        var textSanitized = Sanitize(text);
        if (textSanitized == "")
            return null;
        if (char.IsLetter(textSanitized[^1]))
            textSanitized += ".";

        var ssmlTraits = SoundTraits.RateFast;
        if (isWhisper)
            ssmlTraits = SoundTraits.PitchVerylow;
        var textSsml = ToSsmlText(textSanitized, ssmlTraits);

        // return await _ttsManager.ConvertTextToSpeech(speaker, textSsml); //TODO: What is this ssml?
        return await _ttsManager.ConvertTextToSpeech(model, speaker, textSanitized);
    }
}

 public sealed class TransformSpeakerVoiceEvent(EntityUid sender, string voiceId) : EntityEventArgs
{
    public EntityUid Sender = sender;
    public string VoiceId = voiceId;
}
