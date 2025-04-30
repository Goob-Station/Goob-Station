// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Eui
{
    /// <summary>
    ///     Sent server -> client to signal that the client should open an EUI.
    /// </summary>
    public sealed class MsgEuiCtl : NetMessage
    {
        public override MsgGroups MsgGroup => MsgGroups.Command;
        public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableOrdered;

        public CtlType Type;
        public string OpenType = string.Empty;
        public uint Id;

        public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
        {
            Id = buffer.ReadUInt32();
            Type = (CtlType) buffer.ReadByte();
            switch (Type)
            {
                case CtlType.Open:
                    OpenType = buffer.ReadString();
                    break;
            }
        }

        public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
        {
            buffer.Write(Id);
            buffer.Write((byte) Type);
            switch (Type)
            {
                case CtlType.Open:
                    buffer.Write(OpenType);
                    break;
            }
        }

        public enum CtlType : byte
        {
            Open,
            Close
        }
    }
}