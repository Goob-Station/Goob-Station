using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob;

[Serializable, NetSerializable]
public sealed class TTSAnnouncedEvent : EntityEventArgs
{
    public byte[] Data { get; }

    public TTSAnnouncedEvent(byte[] data)
    {
        Data = data;
    }
}
