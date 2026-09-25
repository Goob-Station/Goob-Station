using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.VoiceChat;

[Flags]
public enum VoiceLogFlags : byte
{
    None = 0,
    Blocked = 1 << 0,
    Lobby = 1 << 1,
    Radio = 1 << 2,
    Broadcast = 1 << 3,
    Shout = 1 << 4,
    Whisper = 1 << 5,
    Megaphone = 1 << 6,
}

[Serializable, NetSerializable]
public sealed record VoiceLogRound(int RoundId, long StartMs, long EndMs, bool Current);

[Serializable, NetSerializable]
public sealed record VoiceLogSpeaker(Guid UserId, string Username, float TalkSeconds);

[Serializable, NetSerializable]
public sealed record VoiceLogSegment(long StartMs, VoiceLogFlags Flags, string Name, string Channel, byte[] Levels);

[Serializable, NetSerializable]
public sealed class VoiceLogsEuiState : EuiStateBase
{
    public List<VoiceLogRound> Rounds = new();
    public int RoundId;
    public List<VoiceLogSpeaker> Speakers = new();
    public bool Loading;
}

[Serializable, NetSerializable]
public sealed class VoiceLogsSelectRoundMessage(int roundId) : EuiMessageBase
{
    public readonly int RoundId = roundId;
}

[Serializable, NetSerializable]
public sealed class VoiceLogsRefreshMessage : EuiMessageBase;

[Serializable, NetSerializable]
public sealed class VoiceLogsRequestTrackMessage(int roundId, Guid userId) : EuiMessageBase
{
    public readonly int RoundId = roundId;
    public readonly Guid UserId = userId;
}

[Serializable, NetSerializable]
public sealed class VoiceLogsTrackMessage(int roundId, Guid userId, string username, List<VoiceLogSegment> segments) : EuiMessageBase
{
    public readonly int RoundId = roundId;
    public readonly Guid UserId = userId;
    public readonly string Username = username;
    public readonly List<VoiceLogSegment> Segments = segments;
}

[Serializable, NetSerializable]
public sealed class VoiceLogsRequestAudioMessage(int roundId, Guid userId, int firstSegment, int lastSegment) : EuiMessageBase
{
    public readonly int RoundId = roundId;
    public readonly Guid UserId = userId;
    public readonly int FirstSegment = firstSegment;
    public readonly int LastSegment = lastSegment;
}

[Serializable, NetSerializable]
public sealed class VoiceLogsAudioMessage(int roundId, Guid userId, int firstSegment, List<byte[]> payloads) : EuiMessageBase
{
    public readonly int RoundId = roundId;
    public readonly Guid UserId = userId;
    public readonly int FirstSegment = firstSegment;
    public readonly List<byte[]> Payloads = payloads;
}
