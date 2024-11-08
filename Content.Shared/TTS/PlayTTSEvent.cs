using Robust.Shared.Serialization;

namespace Content.Shared.TTS;

[Serializable, NetSerializable]
// ReSharper disable once InconsistentNaming
public sealed class PlayTTSEvent(byte[] data, NetEntity? sourceUid = null, bool isWhisper = false) : EntityEventArgs
{
    public byte[] Data { get; } = data;
    public NetEntity? SourceUid { get; } = sourceUid;
    public bool IsWhisper { get; } = isWhisper;
}
