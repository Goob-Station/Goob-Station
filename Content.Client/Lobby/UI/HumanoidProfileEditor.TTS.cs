using System.Linq;
using Content.Client.TTS;
using Content.Shared.Preferences;
using Content.Shared.TTS;
using Robust.Shared.Random;

namespace Content.Client.Lobby.UI;

public sealed partial class HumanoidProfileEditor
{
    private IRobustRandom _random = default!;
    private TTSSystem _ttsSys = default!;
    private List<TTSVoicePrototype> _voiceList = default!;
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

    private void InitializeVoice()
    {
        _random = IoCManager.Resolve<IRobustRandom>();
        _ttsSys = _entManager.System<TTSSystem>();
        _voiceList = _prototypeManager
            .EnumeratePrototypes<TTSVoicePrototype>()
            .Where(o => o.CanSelect)
            .OrderBy(o => Loc.GetString(o.Name))
            .ToList();

        VoiceButton.OnItemSelected += args =>
        {
            VoiceButton.SelectId(args.Id);
            SetVoice(_voiceList[args.Id].ID);
        };

        VoicePlayButton.OnPressed += _ => { PlayTTS(); };
    }

    private void UpdateTTSVoicesControls()
    {
        if (Profile is null)
            return;

        VoiceButton.Clear();

        var firstVoiceChoiceId = 1;
        for (var i = 0; i < _voiceList.Count; i++)
        {
            var voice = _voiceList[i];
            if (!HumanoidCharacterProfile.CanHaveVoice(voice, Profile.Sex))
                continue;

            var name = Loc.GetString(voice.Name);
            VoiceButton.AddItem(name, i);

            if (firstVoiceChoiceId == 1)
                firstVoiceChoiceId = i;

        }

        var voiceChoiceId = _voiceList.FindIndex(x => x.ID == Profile.Voice);
        if (!VoiceButton.TrySelectId(voiceChoiceId) &&
            VoiceButton.TrySelectId(firstVoiceChoiceId))
        {
            SetVoice(_voiceList[firstVoiceChoiceId].ID);
        }
    }

    private void PlayTTS()
    {
        if (Profile is null)
            return;

        _ttsSys.RequestPreviewTTS(Profile.Voice);
    }
}
