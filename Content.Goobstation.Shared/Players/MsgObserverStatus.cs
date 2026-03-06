using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Players;

/// <summary>
/// Sent server to client to inform the client of their observer status.
/// Used by DenyObserverRequirement to block observers from taking certain ghost roles.
/// </summary>
public sealed class MsgObserverStatus : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    public bool JoinedAsObserver;
    public bool IsAdmin;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        JoinedAsObserver = buffer.ReadBoolean();
        IsAdmin = buffer.ReadBoolean();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(JoinedAsObserver);
        buffer.Write(IsAdmin);
    }
}
