// SPDX-FileCopyrightText: 2024 778b <33431126+778b@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Spawners.EntitySystems;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Spawners.Components;

/// <summary>
/// A spawn point that spawns a player into a target container rather than simply spawning them at a position.
/// Occurs before regular spawn points but after arrivals.
/// </summary>
[RegisterComponent]
[Access(typeof(ContainerSpawnPointSystem))]
public sealed partial class ContainerSpawnPointComponent : Component, ISpawnPoint
{
    /// <summary>
    /// The ID of the container that this entity will spawn players into
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string ContainerId = string.Empty;

    /// <summary>
    /// An optional job specifier
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<JobPrototype>? Job;

    /// <summary>
    /// The type of spawn point
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SpawnPointType SpawnType { get; set; } = SpawnPointType.Unset;
}