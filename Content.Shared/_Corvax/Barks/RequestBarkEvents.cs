using Robust.Shared.Serialization;

namespace Content.Shared._Corvax.Speech.Synthesis;

[Serializable, NetSerializable]
public sealed class PlayBarkEvent(NetEntity sourceUid, string message, bool whisper) : EntityEventArgs
{
    public NetEntity SourceUid { get; } = sourceUid;
    public string Message { get; } = message;
    public bool Whisper { get; } = whisper;
}
