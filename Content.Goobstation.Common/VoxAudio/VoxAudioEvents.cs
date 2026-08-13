using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.VoxAudio;

[Serializable, NetSerializable]
public sealed class PlayVoxAudioEvent : EntityEventArgs
{
    public readonly string Message;
    public readonly TimeSpan Delay;
    public bool Cancelled = false;

    public PlayVoxAudioEvent(string message, TimeSpan delay)
    {
        Message = message;
        Delay = delay;
    }
}
