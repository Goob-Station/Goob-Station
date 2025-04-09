// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.NPC.HTN.Preconditions;

public sealed partial class HasOrdersPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField("orders", required: true)] public Enum Orders = default!;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        return Equals(blackboard.GetValueOrDefault<Enum>(NPCBlackboard.CurrentOrders, _entManager), Orders);
    }
}