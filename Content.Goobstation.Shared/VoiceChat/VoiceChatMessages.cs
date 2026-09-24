using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.VoiceChat;

public sealed class MsgVoiceFrame : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;
    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.Unreliable;

    public NetEntity Source;
    public ushort Speaker;
    public ushort Sequence;
    public byte Flags;
    public VoiceRoute Route;
    public bool Global;
    public float Range;
    public byte[] Payload = Array.Empty<byte>();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Source = buffer.ReadNetEntity();
        Speaker = buffer.ReadUInt16();
        Sequence = buffer.ReadUInt16();
        Flags = buffer.ReadByte();
        Route = (VoiceRoute) buffer.ReadByte();
        Global = buffer.ReadBoolean();
        Range = buffer.ReadFloat();
        var length = buffer.ReadVariableInt32();
        Payload = length is > 0 and <= VoiceCodec.FrameBytes ? buffer.ReadBytes(length) : Array.Empty<byte>();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Source);
        buffer.Write(Speaker);
        buffer.Write(Sequence);
        buffer.Write(Flags);
        buffer.Write((byte) Route);
        buffer.Write(Global);
        buffer.Write(Range);
        buffer.WriteVariableInt32(Payload.Length);
        buffer.Write(Payload);
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

    public bool HearSelf;
    public bool Receive = true;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        HearSelf = buffer.ReadBoolean();
        Receive = buffer.ReadBoolean();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(HearSelf);
        buffer.Write(Receive);
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
