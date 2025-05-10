using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Casino.Shared.Games.Slots;

public sealed class PlaySlotsRequest : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.String;

    public int Bet { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Bet = buffer.ReadInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Bet);
    }
}

public sealed class SlotsResultMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.String;

    public bool Won { get; set; }
    public int Payout { get; set; }
    public SlotSymbol[] Symbols { get; set; } = new SlotSymbol[3];
    public string Message { get; set; } = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Won = buffer.ReadBoolean();
        Payout = buffer.ReadInt32();

        for (var i = 0; i < 3; i++)
        {
            Symbols[i] = (SlotSymbol)buffer.ReadByte();
        }

        Message = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Won);
        buffer.Write(Payout);

        for (var i = 0; i < 3; i++)
        {
            buffer.Write((byte)Symbols[i]);
        }

        buffer.Write(Message);
    }
}
