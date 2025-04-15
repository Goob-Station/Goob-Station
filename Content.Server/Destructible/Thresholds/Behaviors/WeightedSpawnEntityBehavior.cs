// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server.Spawners.Components;
using Content.Server.Spawners.EntitySystems;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;

namespace Content.Server.Destructible.Thresholds.Behaviors;

/// <summary>
/// Behavior that can be assigned to a trigger that that takes a <see cref="WeightedRandomEntityPrototype"/>
/// and spawns a number of the same entity between a given min and max
/// at a random offset from the final position of the entity.
/// </summary>
[Serializable]
[DataDefinition]
public sealed partial class WeightedSpawnEntityBehavior : IThresholdBehavior
{
    /// <summary>
    /// A table of entities with assigned weights to randomly pick from
    /// </summary>
    [DataField(required: true)]
    public ProtoId<WeightedRandomEntityPrototype> WeightedEntityTable;

    /// <summary>
    /// How far away to spawn the entity from the parent position
    /// </summary>
    [DataField]
    public float SpawnOffset = 1;

    /// <summary>
    /// The mininum number of entities to spawn randomly
    /// </summary>
    [DataField]
    public int MinSpawn = 1;

    /// <summary>
    /// The max number of entities to spawn randomly
    /// </summary>
    [DataField]
    public int MaxSpawn = 1;

    /// <summary>
    /// Time in seconds to wait before spawning entities
    /// </summary>
    [DataField]
    public float SpawnAfter;

    public void Execute(EntityUid uid, DestructibleSystem system, EntityUid? cause = null)
    {
        // Get the position at which to start initially spawning entities
        var transform = system.EntityManager.System<TransformSystem>();
        var position = transform.GetMapCoordinates(uid);
        // Helper function used to randomly get an offset to apply to the original position
        Vector2 GetRandomVector() => new (system.Random.NextFloat(-SpawnOffset, SpawnOffset), system.Random.NextFloat(-SpawnOffset, SpawnOffset));
        // Randomly pick the entity to spawn and randomly pick how many to spawn
        var entity = system.PrototypeManager.Index(WeightedEntityTable).Pick(system.Random);
        var amountToSpawn = system.Random.NextFloat(MinSpawn, MaxSpawn);

        // Different behaviors for delayed spawning and immediate spawning
        if (SpawnAfter != 0)
        {
            // if it fails to get the spawner, this won't ever work so just return
            if (!system.PrototypeManager.TryIndex("TemporaryEntityForTimedDespawnSpawners", out var tempSpawnerProto))
                return;

            // spawn the spawner, assign it a lifetime, and assign the entity that it will spawn when despawned
            for (var i = 0; i < amountToSpawn; i++)
            {
                var spawner = system.EntityManager.SpawnEntity(tempSpawnerProto.ID, position.Offset(GetRandomVector()));
                system.EntityManager.EnsureComponent<TimedDespawnComponent>(spawner, out var timedDespawnComponent);
                timedDespawnComponent.Lifetime = SpawnAfter;
                system.EntityManager.EnsureComponent<SpawnOnDespawnComponent>(spawner, out var spawnOnDespawnComponent);
                system.EntityManager.System<SpawnOnDespawnSystem>().SetPrototype((spawner, spawnOnDespawnComponent), entity);
            }
        }
        else
        {
            // directly spawn the desired entities
            for (var i = 0; i < amountToSpawn; i++)
            {
                system.EntityManager.SpawnEntity(entity, position.Offset(GetRandomVector()));
            }
        }
    }
}