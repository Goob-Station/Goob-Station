using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Twitch;

[Serializable, NetSerializable]
public sealed class TwitchChatCritterOpenEvent(NetEntity camera, TimeSpan expiresAt) : EntityEventArgs
{
    public NetEntity Camera { get; } = camera;
    public TimeSpan ExpiresAt { get; } = expiresAt;
}

[Serializable, NetSerializable]
public sealed class TwitchChatCritterCommandEvent(string viewer, string command) : EntityEventArgs
{
    public string Viewer { get; } = viewer;
    public string Command { get; } = command;
}

[Serializable, NetSerializable]
public sealed class TwitchChatCritterClosedEvent : EntityEventArgs;
