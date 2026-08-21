using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Twitch;

[Serializable, NetSerializable]
public sealed class TwitchChatCritterOpenEvent(NetEntity critter) : EntityEventArgs
{
    public NetEntity Critter { get; } = critter;
}

[Serializable, NetSerializable]
public sealed class TwitchChatCritterCloseEvent(NetEntity critter) : EntityEventArgs
{
    public NetEntity Critter { get; } = critter;
}

[Serializable, NetSerializable]
public sealed class TwitchChatCritterCommandEvent(string viewer, string command) : EntityEventArgs
{
    public string Viewer { get; } = viewer;
    public string Command { get; } = command;
}

[Serializable, NetSerializable]
public sealed class TwitchChatCritterClosedEvent : EntityEventArgs;
