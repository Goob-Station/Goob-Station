// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking;
using Content.Shared.Follower;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests;

[TestFixture, TestOf(typeof(FollowerSystem))]
public sealed class FollowerSystemTest
{
    /// <summary>
    ///     This test ensures that deleting a map while an entity follows another doesn't throw any exceptions.
    /// </summary>
    [Test]
    public async Task FollowerMapDeleteTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var entMan = server.ResolveDependency<IEntityManager>();
        var mapMan = server.ResolveDependency<IMapManager>();
        var sysMan = server.ResolveDependency<IEntitySystemManager>();
        var logMan = server.ResolveDependency<ILogManager>();
        var mapSys = server.System<SharedMapSystem>();
        var logger = logMan.RootSawmill;

        await server.WaitPost(() =>
        {
            var followerSystem = sysMan.GetEntitySystem<FollowerSystem>();

            // Create a map to spawn the observers on.
            mapSys.CreateMap(out var map);

            // Spawn an observer to be followed.
            var followed = entMan.SpawnEntity(GameTicker.ObserverPrototypeName, new MapCoordinates(0, 0, map));
            logger.Info($"Spawned followed observer: {entMan.ToPrettyString(followed)}");

            // Spawn an observer to follow another observer.
            var follower = entMan.SpawnEntity(GameTicker.ObserverPrototypeName, new MapCoordinates(0, 0, map));
            logger.Info($"Spawned follower observer: {entMan.ToPrettyString(follower)}");

            followerSystem.StartFollowingEntity(follower, followed);

            entMan.DeleteEntity(mapSys.GetMap(map));
        });
        await pair.CleanReturnAsync();
    }
}