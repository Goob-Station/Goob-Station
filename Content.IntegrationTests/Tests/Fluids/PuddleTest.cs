// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2024 Jake Huxell <JakeHuxell@pm.me>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.Fluids
{
    [TestFixture]
    [TestOf(typeof(PuddleComponent))]
    public sealed class PuddleTest
    {
        [Test]
        public async Task TilePuddleTest()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;

            var testMap = await pair.CreateTestMap();

            var spillSystem = server.System<PuddleSystem>();

            await server.WaitAssertion(() =>
            {
                var solution = new Solution("Water", FixedPoint2.New(20));
                var tile = testMap.Tile;
                var gridUid = tile.GridUid;
                var (x, y) = tile.GridIndices;
                var coordinates = new EntityCoordinates(gridUid, x, y);

                Assert.That(spillSystem.TrySpillAt(coordinates, solution, out _), Is.True);
            });
            await pair.RunTicksSync(5);

            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task SpaceNoPuddleTest()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;

            var testMap = await pair.CreateTestMap();
            var grid = testMap.Grid;

            var entitySystemManager = server.ResolveDependency<IEntitySystemManager>();
            var spillSystem = server.System<PuddleSystem>();
            var mapSystem = server.System<SharedMapSystem>();

            // Remove all tiles
            await server.WaitPost(() =>
            {
                var tiles = mapSystem.GetAllTiles(grid.Owner, grid.Comp);
                foreach (var tile in tiles)
                {
                    mapSystem.SetTile(grid, tile.GridIndices, Tile.Empty);
                }
            });

            await pair.RunTicksSync(5);

            await server.WaitAssertion(() =>
            {
                var coordinates = grid.Owner.ToCoordinates();
                var solution = new Solution("Water", FixedPoint2.New(20));

                Assert.That(spillSystem.TrySpillAt(coordinates, solution, out _), Is.False);
            });

            await pair.CleanReturnAsync();
        }
    }
}