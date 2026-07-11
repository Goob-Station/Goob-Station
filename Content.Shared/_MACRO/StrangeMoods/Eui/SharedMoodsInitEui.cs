using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._MACRO.StrangeMoods.Eui;

[Serializable, NetSerializable]
public sealed class SharedMoodsInitStartMessage : EuiMessageBase;

[Serializable, NetSerializable]
public sealed class SharedMoodsInitErrorMessage : EuiMessageBase;

[Serializable, NetSerializable]
public sealed class SharedMoodsInitAcceptMessage(string name) : EuiMessageBase
{
    public string Name { get; } = name;
}

[Serializable, NetSerializable]
public sealed class SharedMoodsInitValidMessage(HashSet<SharedMood> allSharedMoods, SharedMood mood) : EuiMessageBase
{
    public HashSet<SharedMood> AllSharedMoods { get; } = allSharedMoods;
    public SharedMood Mood { get; } = mood;
}