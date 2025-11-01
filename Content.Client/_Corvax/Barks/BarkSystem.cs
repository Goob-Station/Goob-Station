using Content.Shared._Corvax.Speech.Synthesis.Components;
using Content.Shared._Corvax.Speech.Synthesis;
using Content.Shared.Corvax.CorvaxVars;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Client._Corvax.Speech.Synthesis.System;

public sealed class BarkSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudio = default!;

    private readonly Dictionary<NetEntity, EntityUid> _playingSounds = new();
    private static readonly char[] Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PlayBarkEvent>(OnPlayBark);
    }

    public void RequestPreviewBark(string barkId)
    {
        if (!_prototypeManager.TryIndex<BarkPrototype>(barkId, out var proto))
            return;

        var messageLength = _random.Next(5, 20);
        var message = new char[messageLength];
        for (var i = 0; i < messageLength; i++)
        {
            message[i] = _random.Pick(Characters);
        }
        PlayBark(null, new string(message), false, proto);
    }

    private void OnPlayBark(PlayBarkEvent ev)
    {
        var sourceEntity = _entityManager.GetEntity(ev.SourceUid);
        if (!TryComp<SpeechSynthesisComponent>(sourceEntity, out var comp)
            || comp.VoicePrototypeId is null
            || !_prototypeManager.TryIndex<BarkPrototype>(comp.VoicePrototypeId, out var proto))
            return;

        PlayBark(sourceEntity, ev.Message, ev.Whisper, proto);
    }

    private void PlayBark(EntityUid? source, string message, bool whisper, BarkPrototype proto)
    {
        if (proto.SoundCollection is null)
            return;

        if (message.Length > 50)
            message = message[..50];

        var isPreview = source == null;
        var volume = GetVolume(whisper, proto);
        if (volume <= -10f)
            return;

        var upperCount = 0;
        foreach (var c in message)
            if (char.IsUpper(c))
                upperCount++;

        if (upperCount > message.Length / 2
            || message.EndsWith("!!"))
            volume += 5;

        var messageLength = message.Length;
        var totalDuration = Math.Max(0.1f, messageLength * 0.05f);
        var soundInterval = 0.08f / proto.Frequency;
        var soundCount = (int) Math.Max(1, totalDuration / soundInterval);

        for (var i = 0; i < soundCount; i++)
        {
            var character = message[i % messageLength];

            if (character == ' ' || character == '-')
                continue;

            Timer.Spawn(TimeSpan.FromSeconds(i * soundInterval), () =>
            {
                if (!isPreview && TerminatingOrDeleted(source!.Value))
                    return;

                var sound = _sharedAudio.ResolveSound(proto.SoundCollection);
                var audioParams = proto.SoundCollection.Params;

                if (proto.Predictable)
                {
                    var hashCode = character.GetHashCode();

                    if (sound is ResolvedCollectionSpecifier collection && collection.Collection != null)
                    {
                        var soundCollection = _prototypeManager.Index<SoundCollectionPrototype>(collection.Collection);
                        var index = hashCode % soundCollection.PickFiles.Count;
                        sound = new ResolvedCollectionSpecifier(collection.Collection, index);
                    }

                    var minPitchInt = (int) (proto.MinPitch * 100);
                    var maxPitchInt = (int) (proto.MaxPitch * 100);
                    var pitchRangeInt = maxPitchInt - minPitchInt;
                    if (pitchRangeInt != 0)
                    {
                        var predictablePitchInt = hashCode % pitchRangeInt + minPitchInt;
                        var predictablePitch = predictablePitchInt / 100f;
                        audioParams = audioParams.WithPitchScale(predictablePitch);
                    }
                    else
                    {
                        audioParams = audioParams.WithPitchScale(proto.MinPitch);
                    }
                }
                else
                {
                    audioParams = audioParams.WithPitchScale(_random.NextFloat(proto.MinPitch, proto.MaxPitch));
                }

                audioParams = audioParams.WithVolume(volume);

                var filter = Filter.Local();
                var soundEntity = isPreview
                    ? _sharedAudio.PlayGlobal(sound, filter, false, audioParams)
                    : _sharedAudio.PlayEntity(sound, filter, source!.Value, false, audioParams);

                if (!isPreview && proto.Stop)
                {
                    if (_playingSounds.TryGetValue(GetNetEntity(source!.Value), out var playing))
                        _sharedAudio.Stop(playing);
                }

                if (!isPreview && soundEntity is not null)
                    _playingSounds[GetNetEntity(source!.Value)] = soundEntity.Value.Entity;
            });
        }
    }

    private float GetVolume(bool whisper, BarkPrototype proto)
    {
        var volume = _cfg.GetCVar(CorvaxVars.BarksVolume);
        var finalVolume = SharedAudioSystem.GainToVolume(volume);

        finalVolume += SharedAudioSystem.GainToVolume(proto.Volume);

        finalVolume += whisper ? -10f : 0f;
        return finalVolume;
    }

}
