using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.RecoveryPassword;
public sealed class MsgSetRecoveryPassword : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    public string Password = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Password = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Password);
    }
}
