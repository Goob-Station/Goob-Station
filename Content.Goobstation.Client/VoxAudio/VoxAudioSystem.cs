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

    public PlayingVoxClip(List<string> wordchain, TimeSpan startTime)
    {
        Wordchain = wordchain;
        NextWordPlayTime = startTime;
    }
}

[Access(typeof(VoxAudioSystem))]
public sealed class VoxAudioSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IResourceManager _resourceManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public const int WordLimit = 30;
    private readonly ResPath _voxResPath = new ResPath("/Audio/_Goobstation/Announcements/vox_fem");
    private readonly TimeSpan _wordInterval = TimeSpan.FromSeconds(1);

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
            var path = new SoundPathSpecifier($"{_voxResPath}/{word}.ogg");

            _audio.PlayGlobal(path, Filter.Broadcast(), true, AudioParams.Default.WithVolume(2f));
            clip.NextWordPlayTime = _timing.CurTime + _wordInterval;
            clip.WordIndex++;
        });

        _voxClipQueue.RemoveAll(clip => clip.WordIndex == clip.Wordchain.Count());
    }

    private IEnumerable<string> GetWords()
    {
        List<string> names = [];
        var files = _resourceManager.ContentFindFiles(_voxResPath).Where(x => x.Extension == "ogg");
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

        var clip = new PlayingVoxClip(wordchain, _timing.CurTime + _wordInterval + ev.Delay);

        _voxClipQueue.Add(clip);
    }
}