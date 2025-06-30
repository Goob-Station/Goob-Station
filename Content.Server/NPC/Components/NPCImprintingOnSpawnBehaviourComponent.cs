// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Server.NPC.Components;
/// <summary>
/// A component that makes the entity friendly to nearby creatures it sees on init.
/// </summary>
[RegisterComponent]
public sealed partial class NPCImprintingOnSpawnBehaviourComponent : Component
{
    /// <summary>
    /// filter who can be a friend to this creature
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// when a creature appears, it will memorize all creatures in the radius to remember them as friends
    /// </summary>
    [DataField]
    public float SpawnFriendsSearchRadius = 3f;

    /// <summary>
    /// if there is a FollowCompound in HTN, the target of the following will be selected from random nearby targets when it appears
    /// </summary>
    [DataField]
    public bool Follow = true;

    /// <summary>
    /// is used to determine who became a friend from this component
    /// </summary>
    [DataField]
    public List<EntityUid> Friends = new();
}