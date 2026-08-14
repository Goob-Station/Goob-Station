using System.Linq;
using Content.Goobstation.Common.VoxAudio;
using Content.Goobstation.Shared.VoxAudio;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.VoxAudio;

public sealed class PlayingVoxClip
{
    public int WordIndex = 0;
    public List<VoxPlaybackWord> Wordchain;
    public TimeSpan NextWordPlayTime;
    public TimeSpan StartTime;
    public TimeSpan MaxRuntime;
    public EntityUid? TargetEnt;

    public PlayingVoxClip(List<VoxPlaybackWord> wordchain, TimeSpan startTime, TimeSpan playDelay,
        TimeSpan maxRuntime, EntityUid? targetUid)
    {
        Wordchain = wordchain;
        NextWordPlayTime = startTime + playDelay;
        StartTime = startTime;
        MaxRuntime = maxRuntime;
        TargetEnt = targetUid;
    }
}

public sealed class VoxAudioSystem : SharedVoxAudioSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly List<PlayingVoxClip> _voxClipQueue = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<VoxPlayMessage>(OnVoxPlayMessage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _voxClipQueue.ForEach(clip =>
        {
            if (clip.NextWordPlayTime > _timing.CurTime)
                return;

            Log.Error("Should play");

            var word = clip.Wordchain.ElementAt(clip.WordIndex);

            Log.Error($"{word.Path}");

            if (_resourceCache.TryGetResource(word.Path, out AudioResource? resource))
            {
                var @params = AudioParams.Default.WithVolume(2f);
                var stream = resource.AudioStream;

                if (clip.TargetEnt is not null)
                    _audio.PlayEntity(stream, clip.TargetEnt.Value, null, @params);
                else
                    _audio.PlayGlobal(stream, null, @params);

                clip.NextWordPlayTime = _timing.CurTime + resource.AudioStream.Length;
            }

            clip.WordIndex++;
        });

        _voxClipQueue.RemoveAll(clip => clip.WordIndex == clip.Wordchain.Count);
        _voxClipQueue.RemoveAll(clip => _timing.CurTime - clip.StartTime > clip.MaxRuntime);
    }

    public override void Play(string message, List<ProtoId<VoxVoicePrototype>> voiceProtoSet, float? delay = 0f,
        float? maxRuntime = null, EntityUid? uid = null, Filter? filter = null)
    {
        var voiceset = voiceProtoSet.Select(id => _proto.Index(id)).ToList();
        var wordchain = GetPlaybackWordChain(voiceset, message);
        Log.Error($"CHAIN LENGTH: {wordchain.Count()}");
        if (wordchain.Count == 0)
            return;
        if (filter != null && !filter.Recipients.Contains(_playerManager.LocalSession))
            return;

        var clip = new PlayingVoxClip(
            wordchain: wordchain,
            startTime: _timing.CurTime,
            playDelay: delay != null ? TimeSpan.FromSeconds(delay.Value) : TimeSpan.Zero,
            maxRuntime: maxRuntime != null ? TimeSpan.FromSeconds(maxRuntime.Value) : TimeSpan.MaxValue,
            targetUid: uid
        );
        _voxClipQueue.Add(clip);
    }

    private void OnVoxPlayMessage(VoxPlayMessage ev)
    {
        Log.Error("receive play");
        Play(ev.Message, ev.VoiceSet, ev.Delay, ev.MaxRuntime, GetEntity(ev.TargetNuid));
    }
}