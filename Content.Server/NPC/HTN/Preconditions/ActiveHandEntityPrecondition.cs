// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Hands.Components;

namespace Content.Server.NPC.HTN.Preconditions;

/// <summary>
/// Returns true if an entity is held in the active hand.
/// </summary>
public sealed partial class ActiveHandEntityPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        if (!blackboard.TryGetValue(NPCBlackboard.ActiveHand, out Hand? activeHand, _entManager))
        {
            return false;
        }

        return activeHand.HeldEntity != null;
    }
}