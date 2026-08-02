// SPDX-License-Identifier: MIT

using Content.Server.NPC.Systems;

namespace Content.Server.NPC.Components;

/// <summary>
/// Entities with this component will retaliate against those who physically attack them.
/// It has an optional "memory" specification wherein it will only attack those entities for a specified length of time.
/// </summary>
[RegisterComponent /*Access(typeof(NPCRetaliationSystem))*/ ] // Goobstation - Removed the Access. It is used in NPCUtilitySystem
public sealed partial class NPCRetaliationComponent : Component
{
    /// <summary>
    /// How long after being attacked will an NPC continue to be aggressive to the attacker for.
    /// </summary>
    [DataField]
    public TimeSpan? AttackMemoryLength;

    /// <summary>
    /// A dictionary that stores an entity and the time at which they will no longer be considered hostile.
    /// </summary>
    /// todo: this needs to support timeoffsetserializer at some point
    [DataField]
    public Dictionary<EntityUid, TimeSpan> AttackMemories = new();

    /// <summary>
    /// Goobstation - Whether or not the update should be running
    /// </summary>
    [DataField]
    public bool Activated = false;
}
