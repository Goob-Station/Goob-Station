using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Barks;

[Serializable, NetSerializable]
public sealed class PlayBarkEvent(NetEntity sourceUid, string message, bool whisper) : EntityEventArgs
{
    public NetEntity SourceUid { get; } = sourceUid;
    public string Message { get; } = message;
    public bool Whisper { get; } = whisper;
}

[Serializable, NetSerializable]
public sealed class PreviewBarkEvent(string barkProtoID) : EntityEventArgs
{
    public string BarkProtoID { get; } = barkProtoID;
}
