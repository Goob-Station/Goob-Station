using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.VoiceChat;

public readonly record struct VoiceContributor(ushort Speaker, byte Level);

public sealed class MsgVoiceFrame : NetMessage
{
    public const int MaxContributors = 32;

    public override MsgGroups MsgGroup => MsgGroups.Core;
    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.Unreliable;

    public NetEntity Source;
    public ushort Speaker;
    public ushort Sequence;
    public byte Flags;
    public VoiceRoute Route;
    public VoiceFormat Format;
    public bool Global;
    public float Range;
    public byte[] Payload = Array.Empty<byte>();
    public List<VoiceContributor>? Contributors;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Source = buffer.ReadNetEntity();
        Speaker = buffer.ReadUInt16();
        Sequence = buffer.ReadUInt16();
        Flags = buffer.ReadByte();
        Route = (VoiceRoute) buffer.ReadByte();
        Format = (VoiceFormat) buffer.ReadByte();
        Global = buffer.ReadBoolean();
        Range = buffer.ReadFloat();
        var length = buffer.ReadVariableInt32();
        Payload = length is > 0 and <= VoiceCodec.FrameBytes ? buffer.ReadBytes(length) : Array.Empty<byte>();

        var count = Math.Min((int) buffer.ReadByte(), MaxContributors);
        if (count == 0)
            return;

        Contributors = new List<VoiceContributor>(count);
        for (var i = 0; i < count; i++)
        {
            Contributors.Add(new VoiceContributor(buffer.ReadUInt16(), buffer.ReadByte()));
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Source);
        buffer.Write(Speaker);
        buffer.Write(Sequence);
        buffer.Write(Flags);
        buffer.Write((byte) Route);
        buffer.Write((byte) Format);
        buffer.Write(Global);
        buffer.Write(Range);
        buffer.WriteVariableInt32(Payload.Length);
        buffer.Write(Payload);

        var count = Math.Min(Contributors?.Count ?? 0, MaxContributors);
        buffer.Write((byte) count);
        for (var i = 0; i < count; i++)
        {
            buffer.Write(Contributors![i].Speaker);
            buffer.Write(Contributors[i].Level);
        }
    }
}

public sealed class MsgVoiceLinkRequest : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
    }
}

public sealed class MsgVoiceLink : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public string Url = string.Empty;
    public string Token = string.Empty;
    public int StatusPort;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Url = buffer.ReadString();
        Token = buffer.ReadString();
        StatusPort = buffer.ReadInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Url);
        buffer.Write(Token);
        buffer.Write(StatusPort);
    }
}

public sealed class MsgVoiceSettings : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public const int MaxMutedChannels = 64;
    public const int MaxMutedSpeakers = 256;

    public bool HearSelf;
    public bool Receive = true;
    public List<string> MutedChannels = new();
    public List<ushort> MutedSpeakers = new();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        HearSelf = buffer.ReadBoolean();
        Receive = buffer.ReadBoolean();

        var count = Math.Min(buffer.ReadVariableInt32(), MaxMutedChannels);
        MutedChannels = new List<string>(Math.Max(count, 0));
        for (var i = 0; i < count; i++)
        {
            MutedChannels.Add(buffer.ReadString());
        }

        var speakers = Math.Clamp(buffer.ReadVariableInt32(), 0, MaxMutedSpeakers);
        MutedSpeakers = new List<ushort>(speakers);
        for (var i = 0; i < speakers; i++)
        {
            MutedSpeakers.Add(buffer.ReadUInt16());
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(HearSelf);
        buffer.Write(Receive);

        var count = Math.Min(MutedChannels.Count, MaxMutedChannels);
        buffer.WriteVariableInt32(count);
        for (var i = 0; i < count; i++)
        {
            buffer.Write(MutedChannels[i]);
        }

        var speakers = Math.Min(MutedSpeakers.Count, MaxMutedSpeakers);
        buffer.WriteVariableInt32(speakers);
        for (var i = 0; i < speakers; i++)
        {
            buffer.Write(MutedSpeakers[i]);
        }
    }
}

public sealed class MsgVoiceStatus : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public bool Connected;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Connected = buffer.ReadBoolean();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Connected);
    }
}

public sealed class MsgVoiceSpeakerInfo : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public ushort Speaker;
    public string Name = string.Empty;
    public string Channel = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Speaker = buffer.ReadUInt16();
        Name = buffer.ReadString();
        Channel = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Speaker);
        buffer.Write(Name);
        buffer.Write(Channel);
    }
}

[Flags]
public enum VoiceSelfFlags : byte
{
    None = 0,
    Blocked = 1 << 0,
    Radio = 1 << 1,
    Broadcast = 1 << 2,
    Shout = 1 << 3,
    Whisper = 1 << 4,
    Megaphone = 1 << 5,
    God = 1 << 6,
}

public sealed class MsgVoiceSelf : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;
    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.Unreliable;

    public ushort Speaker;
    public VoiceSelfFlags Flags;
    public VoiceLevels Levels;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Speaker = buffer.ReadUInt16();
        Flags = (VoiceSelfFlags) buffer.ReadByte();
        Levels.Low = buffer.ReadByte() / 255f;
        Levels.Mid = buffer.ReadByte() / 255f;
        Levels.High = buffer.ReadByte() / 255f;
        Levels.Overall = buffer.ReadByte() / 255f;
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Speaker);
        buffer.Write((byte) Flags);
        buffer.Write(ToByte(Levels.Low));
        buffer.Write(ToByte(Levels.Mid));
        buffer.Write(ToByte(Levels.High));
        buffer.Write(ToByte(Levels.Overall));
    }

    private static byte ToByte(float level)
    {
        return (byte) Math.Clamp(MathF.Round(level * 255f), 0f, 255f);
    }
}

public sealed class MsgVoicePushToTalk : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public bool Pressed;
    public bool Radio;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Pressed = buffer.ReadBoolean();
        Radio = buffer.ReadBoolean();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Pressed);
        buffer.Write(Radio);
    }
}

public sealed class MsgVoiceMicMute : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public bool Muted;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Muted = buffer.ReadBoolean();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Muted);
    }
}

[Serializable, NetSerializable]
public sealed class VoiceGodCueEvent : EntityEventArgs;
