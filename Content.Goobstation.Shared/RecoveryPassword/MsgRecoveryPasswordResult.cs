using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.RecoveryPassword;

public sealed class MsgRecoveryPasswordResult : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    public RecoveryPasswordSetResult Result;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Result = (RecoveryPasswordSetResult) buffer.ReadByte();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write((byte) Result);
    }
}
