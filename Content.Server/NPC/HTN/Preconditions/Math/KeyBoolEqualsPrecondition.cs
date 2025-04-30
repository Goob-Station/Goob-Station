// SPDX-FileCopyrightText: 2024 Tornado Tech <54727692+Tornado-Technology@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.NPC.HTN.Preconditions.Math;

/// <summary>
/// Checks if there is a bool value for the specified <see cref="KeyBoolEqualsPrecondition.Key"/>
/// in the <see cref="NPCBlackboard"/> and the specified value is equal to the <see cref="KeyBoolEqualsPrecondition.Value"/>.
/// </summary>
public sealed partial class KeyBoolEqualsPrecondition : HTNPrecondition
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    [DataField(required: true), ViewVariables]
    public string Key = string.Empty;

    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public bool Value;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        if (!blackboard.TryGetValue<bool>(Key, out var value, _entManager))
            return false;

        return Value == value;
    }
}