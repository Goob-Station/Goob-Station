// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Spawners.EntitySystems;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Prototypes;

namespace Content.Server.Spawners.Components;

[RegisterComponent, EntityCategory("Spawner"), Access(typeof(ConditionalSpawnerSystem))]
public sealed partial class EntityTableSpawnerComponent : Component
{
    /// <summary>
    /// Table that determines what gets spawned.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table = default!;

    /// <summary>
    /// Scatter of entity spawn coordinates
    /// </summary>
    [DataField]
    public float Offset = 0.2f;

    /// <summary>
    /// A variable meaning whether the spawn will
    /// be able to be used again or whether
    /// it will be destroyed after the first use
    /// </summary>
    [DataField]
    public bool DeleteSpawnerAfterSpawn = true;
}
