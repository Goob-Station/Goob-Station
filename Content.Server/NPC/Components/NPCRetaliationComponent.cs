// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.NPC.Systems;

namespace Content.Server.NPC.Components;

/// <summary>
/// Entities with this component will retaliate against those who physically attack them.
/// It has an optional "memory" specification wherein it will only attack those entities for a specified length of time.
/// </summary>
[RegisterComponent, Access(typeof(NPCRetaliationSystem))]
public sealed partial class NPCRetaliationComponent : Component
{
    /// <summary>
    /// How long after being attacked will an NPC continue to be aggressive to the attacker for.
    /// </summary>
    [DataField("attackMemoryLength"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? AttackMemoryLength;

    /// <summary>
    /// A dictionary that stores an entity and the time at which they will no longer be considered hostile.
    /// </summary>
    /// todo: this needs to support timeoffsetserializer at some point
    [DataField("attackMemories")]
    public Dictionary<EntityUid, TimeSpan> AttackMemories = new();
}