using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.TTS;

[Serializable, NetSerializable]
// ReSharper disable once InconsistentNaming
public sealed class PlayTTSEvent : EntityEventArgs
{
    public byte[] Data { get; }
    public NetEntity? SourceUid { get; }
    public bool IsWhisper { get; }
    public float? Pitch { get; }

    public PlayTTSEvent(byte[] data, NetEntity? sourceUid = null, bool isWhisper = false, float? pitch = null)
    {
        Data = data;
        SourceUid = sourceUid;
        IsWhisper = isWhisper;
        Pitch = pitch;
    }
}
