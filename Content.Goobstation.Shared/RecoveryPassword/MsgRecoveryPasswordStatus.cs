using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.RecoveryPassword;

public sealed class MsgRecoveryPasswordStatus : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    public bool HasPassword;
    public bool Enabled;
    public int MinLength;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        HasPassword = buffer.ReadBoolean();
        Enabled = buffer.ReadBoolean();
        buffer.ReadPadBits();
        MinLength = buffer.ReadInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(HasPassword);
        buffer.Write(Enabled);
        buffer.WritePadBits();
        buffer.Write(MinLength);
    }
}
