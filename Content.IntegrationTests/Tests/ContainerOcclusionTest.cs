// SPDX-FileCopyrightText: 2020 L.E.D <10257081+unusualcrow@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server.Storage.EntitySystems;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests
{
    public sealed class ContainerOcclusionTest
    {
        [TestPrototypes]
        private const string Prototypes = @"
- type: entity
  id: ContainerOcclusionA
  components:
  - type: EntityStorage
    occludesLight: true

- type: entity
  id: ContainerOcclusionB
  components:
  - type: EntityStorage
    showContents: true
    occludesLight: false

- type: entity
  id: ContainerOcclusionDummy
  components:
  - type: Sprite
  - type: PointLight
";

        [Test]
        public async Task TestA()
        {
            await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
            var server = pair.Server;
            var client = pair.Client;

            var clientEntManager = client.ResolveDependency<IEntityManager>();
            var serverEntManager = server.ResolveDependency<IEntityManager>();

            EntityUid dummy = default;
            var mapManager = server.ResolveDependency<IMapManager>();
            var map = await pair.CreateTestMap();

            await server.WaitPost(() =>
            {
                var pos = new MapCoordinates(Vector2.Zero, map.MapId);
                var entStorage = serverEntManager.EntitySysManager.GetEntitySystem<EntityStorageSystem>();
                var container = serverEntManager.SpawnEntity("ContainerOcclusionA", pos);
                dummy = serverEntManager.SpawnEntity("ContainerOcclusionDummy", pos);

                entStorage.Insert(dummy, container);
            });

            await pair.RunTicksSync(5);

            var clientEnt = clientEntManager.GetEntity(serverEntManager.GetNetEntity(dummy));

            await client.WaitAssertion(() =>
            {
                var sprite = clientEntManager.GetComponent<SpriteComponent>(clientEnt);
                var light = clientEntManager.GetComponent<PointLightComponent>(clientEnt);
                Assert.Multiple(() =>
                {
                    Assert.That(sprite.ContainerOccluded);
                    Assert.That(light.ContainerOccluded);
                });
            });

            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task TestB()
        {
            await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
            var server = pair.Server;
            var client = pair.Client;

            var clientEntManager = client.ResolveDependency<IEntityManager>();
            var serverEntManager = server.ResolveDependency<IEntityManager>();

            EntityUid dummy = default;
            var mapManager = server.ResolveDependency<IMapManager>();

            var map = await pair.CreateTestMap();

            await server.WaitPost(() =>
            {
                var pos = new MapCoordinates(Vector2.Zero, map.MapId);
                var entStorage = serverEntManager.EntitySysManager.GetEntitySystem<EntityStorageSystem>();
                var container = serverEntManager.SpawnEntity("ContainerOcclusionB", pos);
                dummy = serverEntManager.SpawnEntity("ContainerOcclusionDummy", pos);

                entStorage.Insert(dummy, container);
            });

            await pair.RunTicksSync(5);

            var clientEnt = clientEntManager.GetEntity(serverEntManager.GetNetEntity(dummy));

            await client.WaitAssertion(() =>
            {
                var sprite = clientEntManager.GetComponent<SpriteComponent>(clientEnt);
                var light = clientEntManager.GetComponent<PointLightComponent>(clientEnt);
                Assert.Multiple(() =>
                {
                    Assert.That(sprite.ContainerOccluded, Is.False);
                    Assert.That(light.ContainerOccluded, Is.False);
                });
            });

            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task TestAb()
        {
            await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
            var server = pair.Server;
            var client = pair.Client;

            var clientEntManager = client.ResolveDependency<IEntityManager>();
            var serverEntManager = server.ResolveDependency<IEntityManager>();

            EntityUid dummy = default;
            var mapManager = server.ResolveDependency<IMapManager>();

            var map = await pair.CreateTestMap();

            await server.WaitPost(() =>
            {
                var pos = new MapCoordinates(Vector2.Zero, map.MapId);
                var entStorage = serverEntManager.EntitySysManager.GetEntitySystem<EntityStorageSystem>();
                var containerA = serverEntManager.SpawnEntity("ContainerOcclusionA", pos);
                var containerB = serverEntManager.SpawnEntity("ContainerOcclusionB", pos);
                dummy = serverEntManager.SpawnEntity("ContainerOcclusionDummy", pos);

                entStorage.Insert(containerB, containerA);
                entStorage.Insert(dummy, containerB);
            });

            await pair.RunTicksSync(5);

            var clientEnt = clientEntManager.GetEntity(serverEntManager.GetNetEntity(dummy));

            await client.WaitAssertion(() =>
            {
                var sprite = clientEntManager.GetComponent<SpriteComponent>(clientEnt);
                var light = clientEntManager.GetComponent<PointLightComponent>(clientEnt);
                Assert.Multiple(() =>
                {
                    Assert.That(sprite.ContainerOccluded);
                    Assert.That(light.ContainerOccluded);
                });
            });

            await pair.CleanReturnAsync();
        }
    }
}