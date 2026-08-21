using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Twitch;

[Serializable, NetSerializable]
public sealed class TwitchBitsToastEvent(string message) : EntityEventArgs
{
    public string Message { get; } = message;
}
