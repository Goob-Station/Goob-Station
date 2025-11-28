using Content.Shared._CorvaxGoob;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Client._CorvaxGoob.TTS;

public sealed partial class TTSSystem
{
    private float _announcementsVolume = 0.0f;

    private void OnAnnouncementsVolumeChanged(float volume)
    {
        _announcementsVolume = volume;
    }

    private void PlayTTS(byte[] data)
    {
        var filePath = new ResPath($"{_fileIdx++}.ogg");
        _contentRoot.AddOrUpdateFile(filePath, data!);

        var audioResource = new AudioResource();
        audioResource.Load(IoCManager.Instance!, Prefix / filePath);

        var audioParams = AudioParams.Default
            .WithVolume((AdjustVolume(false) + SharedAudioSystem.GainToVolume(_announcementsVolume)) / 2);

        var soundSpecifier = new SoundPathSpecifier(Prefix / filePath);

        var noiseParams = AudioParams.Default
            .WithLoop(true);

        var mainSound = _audio.PlayGlobal(soundSpecifier, Filter.Local(), true, audioParams);

        if (!mainSound.HasValue)
            return;

        try // I really don't sure about this so I just put it into try-catch
        {
            var effect = _audio.CreateEffect();
            var aux = _audio.CreateAuxiliary();

            if (!_prototypeManager.TryIndex<AudioPresetPrototype>("TTSCommunication", out var prototype))
                return;

            AddComp<AudioPresetComponent>(aux.Entity);
            AddComp<AudioEffectComponent>(mainSound.Value.Entity);

            _audio.SetEffectPreset(effect.Entity, effect.Component, prototype);

            _audio.SetEffect(aux.Entity, aux.Component, effect.Entity);
            _audio.SetAuxiliary(mainSound.Value.Entity, mainSound.Value.Component, aux.Entity);
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Failed to apply audio effect: {ex}");
        }

        _contentRoot.RemoveFile(filePath);
    }

    private void OnAnnounced(TTSAnnouncedEvent args)
    {
        _sawmill.Verbose($"Play TTS audio {args.Data.Length} bytes from station announcement");

        PlayTTS(args.Data);
    }
}

