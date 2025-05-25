// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration
{
    public sealed class MsgUpdateAdminStatus : NetMessage
    {
        public override MsgGroups MsgGroup => MsgGroups.Command;

        public AdminData? Admin;
        public string[] AvailableCommands = Array.Empty<string>();

        public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
        {
            var count = buffer.ReadVariableInt32();

            AvailableCommands = new string[count];

            for (var i = 0; i < count; i++)
            {
                AvailableCommands[i] = buffer.ReadString();
            }

            if (buffer.ReadBoolean())
            {
                var active = buffer.ReadBoolean();
                buffer.ReadPadBits();
                var flags = (AdminFlags) buffer.ReadUInt32();
                var title = buffer.ReadString();

                Admin = new AdminData
                {
                    Active = active,
                    Title = title,
                    Flags = flags,
                };
            }

        }

        public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
        {
            buffer.WriteVariableInt32(AvailableCommands.Length);

            foreach (var cmd in AvailableCommands)
            {
                buffer.Write(cmd);
            }

            buffer.Write(Admin != null);

            if (Admin == null) return;

            buffer.Write(Admin.Active);
            buffer.WritePadBits();
            buffer.Write((uint) Admin.Flags);
            buffer.Write(Admin.Title);
        }

        public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}