using System.Linq;
using Content.Goobstation.Common.VoxAudio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.ContentPack;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.VoxAudio;

/// <summary>
/// Word with an absolute path, exists separately from voxword for basepath behavior
/// Idk a better way to do this,, it might be fine
/// </summary>
public sealed partial class VoxPlaybackWord
{
    [DataField]
    public string Word { get; set; } = default!;

    [DataField]
    public ResPath Path { get; set; } = default!;
}

public abstract partial class SharedVoxAudioSystem : EntitySystem
{
    /// <summary>
    /// Returns an unsorted list of all the valid words provided by the given voice set.
    /// </summary>
    /// <param name="voiceSet">List of all voices used for validation and playback.</param>
    /// <returns></returns>
    public List<string> GetValidWords(List<VoxVoicePrototype> voiceSet)
        => voiceSet
            .SelectMany(voice => voice.Words)
            .Select(word => word.Word)
            .Distinct()
            .ToList();

    /// <summary>
    /// whether equals, or equals with puncutation removed
    /// todo support removing plurality
    /// </summary>
    /// <param name="sWord"></param>
    /// <param name="vWord"></param>
    /// <returns></returns>
    public bool VoxWordCheck(string sWord, VoxWord vWord)
        => vWord.Word.Equals(sWord, StringComparison.CurrentCultureIgnoreCase)
            || vWord.Word.Equals(new string(sWord.Where(c => !char.IsPunctuation(c)).ToArray()));

    /// <summary>
    /// returns a sequential list of all valid words for playback, using the provided voice set.
    /// not case sensitive
    /// "Ten. feet! TWEnty,," => ["ten", "feet", "twenty"].
    /// order is relevant, so if there are duplicate word entries only the first found set's word is used.
    /// </summary>
    /// <param name="voiceSet"></param>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public List<VoxPlaybackWord> GetPlaybackWordChain(List<VoxVoicePrototype> voiceSet, string sentence)
        => sentence
            .Trim()
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(wordStr =>
            {
                foreach (var voice in voiceSet)
                {
                    var word = voice.Words.FirstOrDefault(w => VoxWordCheck(wordStr, w));
                    if (word != null)
                        // we're doing this to allow explicitly defined paths
                        return new VoxPlaybackWord()
                        {
                            Word = word.Word,
                            Path = new ResPath($"{word.Path ?? $"{voice.BasePath}/{word.Word}.ogg"}")
                        };
                }
                return null;
            })
            .OfType<VoxPlaybackWord>()
            .ToList();

    /// <summary>
    /// Resolves VOX voice shit from a message and plays it. if entity provided will play on that entity
    /// using AudioSystem.PlayEntity(), otherwise will use AudioSystem.PlayGlobal().
    /// respects a provided Filter
    /// </summary>
    /// <param name="message"></param>
    /// <param name="voiceProtoSet"></param>
    /// <param name="delay"></param>
    /// <param name="uid"></param>
    /// <param name="filter"></param>
    public abstract void Play(string message, List<ProtoId<VoxVoicePrototype>> voiceProtoSet, float? delay = 0f,
        float? maxRuntime = null, EntityUid? uid = null, Filter? filter = null);
}