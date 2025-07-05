// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MPL-2.0

using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MisandryBox.Spider;

public sealed class SpiderMsg : NetMessage
{
    public override MsgGroups MsgGroup { get; } = MsgGroups.Command;

    public bool Permanent = false;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Permanent = buffer.ReadBoolean();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Permanent);
    }
}

public sealed class SpiderConsentMsg : NetMessage
{
    public override MsgGroups MsgGroup { get; } = MsgGroups.Command;
    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
    }
}

public sealed class SpiderClearMsg : NetMessage
{
    public override MsgGroups MsgGroup { get; } = MsgGroups.Command;
    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
    }
}
