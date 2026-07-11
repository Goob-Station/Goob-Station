using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._MACRO.StrangeMoods.Eui;

[Serializable, NetSerializable]
public sealed class SharedMoodsEuiState(HashSet<SharedMood> allSharedMoods, string? moodId) : EuiStateBase
{
    public HashSet<SharedMood> AllSharedMoods { get; } = allSharedMoods;
    public string? MoodId { get; } = moodId;
}

[Serializable, NetSerializable]
public sealed class SharedMoodsSaveMessage(string target, List<StrangeMood> moods) : EuiMessageBase
{
    public string Target { get; } = target;
    public List<StrangeMood> Moods { get; } = moods;
}

[Serializable, NetSerializable]
public sealed class SharedMoodsRequestMessage(string moodId) : EuiMessageBase
{
    public string MoodId { get; } = moodId;
}

[Serializable, NetSerializable]
public sealed class SharedMoodsSendMessage(SharedMood? mood) : EuiMessageBase
{
    public SharedMood? Mood { get; } = mood;
}