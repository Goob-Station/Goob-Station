using Content.Goobstation.Common.Barks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Goobstation.Common.CCVar;

namespace Content.Goobstation.Client.Barks;

public sealed class BarkSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudio = default!;

    private readonly Dictionary<NetEntity, EntityUid> _playingSounds = new();
    private static readonly char[] Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    private readonly List<ActiveBark> _activeBarks = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PlayBarkEvent>(OnPlayBark);
        SubscribeLocalEvent<PreviewBarkEvent>(OnPreviewBark);
    }

    public void OnPreviewBark(PreviewBarkEvent ev)
    {
        if (!_prototypeManager.TryIndex<BarkPrototype>(ev.BarkProtoID, out var proto))
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
        var sourceEntity = GetEntity(ev.SourceUid);
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

        var volume = GetVolume(whisper, proto);
        if (volume <= -20f)
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

        var activeBark = new ActiveBark
        {
            Source = source,
            IsPreview = source == null,
            Message = message,
            Prototype = proto,
            Volume = volume,
            TotalSounds = soundCount,
            SoundInterval = soundInterval,
            NextSound = _timing.CurTime
        };

        _activeBarks.Add(activeBark);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        for (var i = _activeBarks.Count - 1; i >= 0; i--)
        {
            var bark = _activeBarks[i];

            if (bark.NextSound > _timing.CurTime)
                continue;

            if (!bark.IsPreview && TerminatingOrDeleted(bark.Source!.Value))
            {
                _activeBarks.RemoveAt(i);
                continue;
            }

            var character = bark.Message[bark.CurrentSound % bark.Message.Length];
            if (character != ' ' && character != '-')
                PlaySound(bark, character);

            bark.CurrentSound++;
            bark.NextSound += TimeSpan.FromSeconds(bark.SoundInterval);

            if (bark.CurrentSound >= bark.TotalSounds)
                _activeBarks.RemoveAt(i);
        }
    }

    private void PlaySound(ActiveBark bark, char character)
    {
        var proto = bark.Prototype;
        var sound = _sharedAudio.ResolveSound(proto.SoundCollection!);
        var audioParams = proto.SoundCollection!.Params;

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

        audioParams = audioParams.WithVolume(bark.Volume);

        var filter = Filter.Local();
        var soundEntity = bark.IsPreview
            ? _sharedAudio.PlayGlobal(sound, filter, false, audioParams)
            : _sharedAudio.PlayEntity(sound, filter, bark.Source!.Value, false, audioParams);

        if (!bark.IsPreview && proto.Stop)
        {
            if (_playingSounds.TryGetValue(GetNetEntity(bark.Source!.Value), out var playing))
                _sharedAudio.Stop(playing);
        }

        if (!bark.IsPreview && soundEntity is not null)
            _playingSounds[GetNetEntity(bark.Source!.Value)] = soundEntity.Value.Entity;
    }

    private float GetVolume(bool whisper, BarkPrototype proto)
    {
        var volume = proto.Volume;

        if (whisper)
            volume = 0.05f + (volume - 0.05f) * 0.25f;

        var barksVolume = _cfg.GetCVar(GoobCVars.BarksVolume);
        volume *= barksVolume / 3f;

        return SharedAudioSystem.GainToVolume(volume);
    }

    private sealed class ActiveBark
    {
        public EntityUid? Source;
        public bool IsPreview;
        public string Message = string.Empty;
        public BarkPrototype Prototype = default!;
        public float Volume;

        public int TotalSounds;
        public int CurrentSound;
        public float SoundInterval;
        public TimeSpan NextSound;
    }
}
