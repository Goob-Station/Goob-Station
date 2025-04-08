// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Spawning
{
    public static class EntitySystemExtensions
    {
        public static EntityUid? SpawnIfUnobstructed(
            this IEntityManager entityManager,
            string? prototypeName,
            EntityCoordinates coordinates,
            CollisionGroup collisionLayer,
            in Box2? box = null,
            SharedPhysicsSystem? physicsManager = null)
        {
            physicsManager ??= entityManager.System<SharedPhysicsSystem>();
            var mapCoordinates = coordinates.ToMap(entityManager, entityManager.System<SharedTransformSystem>());

            return entityManager.SpawnIfUnobstructed(prototypeName, mapCoordinates, collisionLayer, box, physicsManager);
        }

        public static EntityUid? SpawnIfUnobstructed(
            this IEntityManager entityManager,
            string? prototypeName,
            MapCoordinates coordinates,
            CollisionGroup collisionLayer,
            in Box2? box = null,
            SharedPhysicsSystem? collision = null)
        {
            var boxOrDefault = box.GetValueOrDefault(Box2.UnitCentered).Translated(coordinates.Position);
            collision ??= entityManager.System<SharedPhysicsSystem>();

            foreach (var body in collision.GetCollidingEntities(coordinates.MapId, in boxOrDefault))
            {
                if (!body.Hard)
                {
                    continue;
                }

                // TODO: wtf fix this
                if (collisionLayer == 0 || (body.CollisionMask & (int) collisionLayer) == 0)
                {
                    continue;
                }

                return null;
            }

            return entityManager.SpawnEntity(prototypeName, coordinates);
        }

        public static bool TrySpawnIfUnobstructed(
            this IEntityManager entityManager,
            string? prototypeName,
            EntityCoordinates coordinates,
            CollisionGroup collisionLayer,
            [NotNullWhen(true)] out EntityUid? entity,
            Box2? box = null,
            SharedPhysicsSystem? physicsManager = null)
        {
            entity = entityManager.SpawnIfUnobstructed(prototypeName, coordinates, collisionLayer, box, physicsManager);

            return entity != null;
        }

        public static bool TrySpawnIfUnobstructed(
            this IEntityManager entityManager,
            string? prototypeName,
            MapCoordinates coordinates,
            CollisionGroup collisionLayer,
            [NotNullWhen(true)] out EntityUid? entity,
            in Box2? box = null,
            SharedPhysicsSystem? physicsManager = null)
        {
            entity = entityManager.SpawnIfUnobstructed(prototypeName, coordinates, collisionLayer, box, physicsManager);

            return entity != null;
        }
    }
}