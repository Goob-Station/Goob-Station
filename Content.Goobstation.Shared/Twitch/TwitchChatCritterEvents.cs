using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Twitch;

[Serializable, NetSerializable]
public sealed class TwitchChatCritterOpenEvent(NetEntity camera) : EntityEventArgs
{
    public NetEntity Camera { get; } = camera;
}

[Serializable, NetSerializable]
public sealed class TwitchChatCritterCloseEvent(NetEntity camera) : EntityEventArgs
{
    public NetEntity Camera { get; } = camera;
}

[Serializable, NetSerializable]
public sealed class TwitchChatCritterCommandEvent(string viewer, string command) : EntityEventArgs
{
    public string Viewer { get; } = viewer;
    public string Command { get; } = command;
}

[Serializable, NetSerializable]
public sealed class TwitchChatCritterClosedEvent : EntityEventArgs;
