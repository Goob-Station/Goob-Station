// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

#nullable enable
using Content.Shared.Physics;
using Content.Shared.Spawning;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics.Systems;

namespace Content.IntegrationTests.Tests.Utility
{
    [TestFixture]
    [TestOf(typeof(EntitySystemExtensions))]
    public sealed class EntitySystemExtensionsTest
    {
        private const string BlockerDummyId = "BlockerDummy";

        [TestPrototypes]
        private const string Prototypes = $@"
- type: entity
  id: {BlockerDummyId}
  name: {BlockerDummyId}
  components:
  - type: Physics
  - type: Fixtures
    fixtures:
      fix1:
        shape:
          !type:PhysShapeAabb
            bounds: ""-0.49,-0.49,0.49,0.49""
        mask:
        - Impassable
";

        [Test]
        public async Task Test()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;

            var testMap = await pair.CreateTestMap();
            var mapCoordinates = testMap.MapCoords;
            var entityCoordinates = testMap.GridCoords;

            var sEntityManager = server.ResolveDependency<IEntityManager>();
            var broady = server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<SharedBroadphaseSystem>();

            await server.WaitAssertion(() =>
            {

                // Nothing blocking it, only entity is the grid
                Assert.Multiple(() =>
                {
                    Assert.That(sEntityManager.SpawnIfUnobstructed(null, entityCoordinates, CollisionGroup.Impassable), Is.Not.Null);
                    Assert.That(sEntityManager.TrySpawnIfUnobstructed(null, entityCoordinates, CollisionGroup.Impassable, out var entity));
                    Assert.That(entity, Is.Not.Null);
                });

                // Nothing blocking it, only entity is the grid
                Assert.Multiple(() =>
                {
                    Assert.That(sEntityManager.SpawnIfUnobstructed(null, mapCoordinates, CollisionGroup.Impassable), Is.Not.Null);
                    Assert.That(sEntityManager.TrySpawnIfUnobstructed(null, mapCoordinates, CollisionGroup.Impassable, out var entity));
                    Assert.That(entity, Is.Not.Null);
                });

                // Spawn a blocker with an Impassable mask
                sEntityManager.SpawnEntity(BlockerDummyId, entityCoordinates);
                broady.Update(0.016f);

                // Cannot spawn something with an Impassable layer
                Assert.Multiple(() =>
                {
                    Assert.That(sEntityManager.SpawnIfUnobstructed(null, entityCoordinates, CollisionGroup.Impassable), Is.Null);
                    Assert.That(sEntityManager.TrySpawnIfUnobstructed(null, entityCoordinates, CollisionGroup.Impassable, out var entity), Is.False);
                    Assert.That(entity, Is.Null);
                });

                Assert.Multiple(() =>
                {
                    Assert.That(sEntityManager.SpawnIfUnobstructed(null, mapCoordinates, CollisionGroup.Impassable), Is.Null);
                    Assert.That(sEntityManager.TrySpawnIfUnobstructed(null, mapCoordinates, CollisionGroup.Impassable, out var entity), Is.False);
                    Assert.That(entity, Is.Null);
                });

                // Other layers are fine
                Assert.Multiple(() =>
                {
                    Assert.That(sEntityManager.SpawnIfUnobstructed(null, entityCoordinates, CollisionGroup.MidImpassable), Is.Not.Null);
                    Assert.That(sEntityManager.TrySpawnIfUnobstructed(null, entityCoordinates, CollisionGroup.MidImpassable, out var entity));
                    Assert.That(entity, Is.Not.Null);
                });

                Assert.Multiple(() =>
                {
                    Assert.That(sEntityManager.SpawnIfUnobstructed(null, mapCoordinates, CollisionGroup.MidImpassable), Is.Not.Null);
                    Assert.That(sEntityManager.TrySpawnIfUnobstructed(null, mapCoordinates, CollisionGroup.MidImpassable, out var entity));
                    Assert.That(entity, Is.Not.Null);
                });
            });
            await pair.CleanReturnAsync();
        }
    }
}