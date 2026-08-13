using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Resources;
using Content.Goobstation.Common.VoxAudio;
using Robust.Client.Audio;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio;
using Robust.Shared.ContentPack;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.VoxAudio;

[Access(typeof(VoxAudioSystem))]
public sealed class PlayingVoxClip
{
    public List<string> Wordchain;
    public int WordIndex = 0;
    public TimeSpan NextWordPlayTime;
    public TimeSpan StartTime;

    public PlayingVoxClip(List<string> wordchain, TimeSpan startTime, TimeSpan playDelay)
    {
        Wordchain = wordchain;
        NextWordPlayTime = startTime + playDelay;
        StartTime = startTime;
    }
}

[Access(typeof(VoxAudioSystem))]
public sealed class VoxAudioSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const int WordLimit = 30;
    private TimeSpan _maxElapsedTime = TimeSpan.FromSeconds(25);
    private readonly ResPath _voxResPath = new ResPath("/Audio/_Goobstation/Announcements/vox_fem");

    private readonly List<PlayingVoxClip> _voxClipQueue = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PlayVoxAudioEvent>(HandlePlayVox);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _voxClipQueue.ForEach(clip =>
        {
            if (clip.NextWordPlayTime > _timing.CurTime)
                return;

            var word = clip.Wordchain.ElementAt(clip.WordIndex);
            var path = new ResPath($"{_voxResPath}/{word}.ogg");

            if (_resourceCache.TryGetResource(path, out AudioResource? resource))
            {
                _audio.PlayGlobal(resource.AudioStream, null, AudioParams.Default.WithVolume(2f));
                clip.NextWordPlayTime = _timing.CurTime + resource.AudioStream.Length;
            }

            clip.WordIndex++;
        });

        _voxClipQueue.RemoveAll(clip => clip.WordIndex == clip.Wordchain.Count());
        _voxClipQueue.RemoveAll(clip => _timing.CurTime - clip.StartTime > _maxElapsedTime);
    }

    private IEnumerable<string> GetWords()
    {
        List<string> names = [];
        var files = _resourceCache.ContentFindFiles(_voxResPath).Where(x => x.Extension == "ogg");
        foreach (var file in files)
            names.Add(file.FilenameWithoutExtension);
        return names;
    }

    private void HandlePlayVox(PlayVoxAudioEvent ev)
    {
        if (ev.Cancelled)
            return;

        var wordbank = GetWords();
        var wordchain = ev.Message.Trim()
            .ToLower()
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(x =>
            {
                // let "don't," resolve to "dont", let "," resolve to ","
                if (wordbank.Contains(x))
                    return x;
                var clean = new string(x.Where(c => !char.IsPunctuation(c)).ToArray());
                return wordbank.Contains(clean) ? clean : "";
            })
            .Take(WordLimit)
            .ToList();

        if (wordchain.Count() == 0)
            return;

        var clip = new PlayingVoxClip(wordchain, _timing.CurTime, ev.Delay);

        _voxClipQueue.Add(clip);
    }
}