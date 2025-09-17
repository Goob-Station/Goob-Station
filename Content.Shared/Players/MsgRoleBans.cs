// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Players;

/// <summary>
/// Sent server -> client to inform the client of their role bans.
/// </summary>
public sealed class MsgRoleBans : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public List<string> JobBans = new();
    public List<string> AntagBans = new();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var jobCount = buffer.ReadVariableInt32();
        JobBans.EnsureCapacity(jobCount);

        for (var i = 0; i < jobCount; i++)
        {
            JobBans.Add(buffer.ReadString());
        }

        var antagCount = buffer.ReadVariableInt32();
        AntagBans.EnsureCapacity(antagCount);

        for (var i = 0; i < antagCount; i++)
        {
            AntagBans.Add(buffer.ReadString());
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(JobBans.Count);

        foreach (var ban in JobBans)
        {
            buffer.Write(ban);
        }

        buffer.WriteVariableInt32(AntagBans.Count);

        foreach (var ban in AntagBans)
        {
            buffer.Write(ban);
        }
    }
}