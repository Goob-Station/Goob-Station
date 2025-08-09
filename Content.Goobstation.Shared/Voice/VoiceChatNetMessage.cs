// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.VoiceChat;

public sealed class MsgVoiceChat : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    public byte[]? PcmData;
    public NetEntity? SourceEntity;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var hasData = buffer.ReadBoolean();
        if (hasData)
        {
            var length = buffer.ReadInt32();
            PcmData = new byte[length];
            buffer.ReadBytes(PcmData, 0, length);
        }
        else
        {
            PcmData = null;
        }

        var hasEntity = buffer.ReadBoolean();
        if (hasEntity)
        {
            SourceEntity = buffer.ReadNetEntity();
        }
        else
        {
            SourceEntity = null;
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(PcmData != null);
        if (PcmData != null)
        {
            buffer.Write(PcmData.Length);
            buffer.Write(PcmData);
        }

        buffer.Write(SourceEntity.HasValue);
        if (SourceEntity.HasValue)
        {
            buffer.Write(SourceEntity.Value);
        }
    }

    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.UnreliableSequenced;
}
