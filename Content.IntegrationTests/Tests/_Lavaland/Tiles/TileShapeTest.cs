// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Content.Shared._Lavaland.Tile;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Lavaland.Tiles;

[TestFixture]
[TestOf(typeof(TileShapeSystem))]
public sealed class TileShapeTest
{
    private static readonly Dictionary<string, int> TestShapes = new()
    {
        { "DebugBoxFilled1",   1 },
        { "DebugBoxFilled2",   4 },
        { "DebugBoxFilled3",   9 },
        { "DebugBoxFilled4",   16 },
        { "DebugBoxFilled5",   25 },
        { "DebugBoxHollow1",   1 },
        { "DebugBoxHollow2",   4 },
        { "DebugBoxHollow3",   8 },
        { "DebugBoxHollow4",   12 },
        { "DebugBoxHollow5",   16 },
        { "DebugCrossRook1",   1 },
        { "DebugCrossBishop1", 1 },
        { "DebugCrossRook5",   17 },
        { "DebugCrossBishop5", 17 },
        { "DebugOptionsTest",  51 },
    };

    [Test]
    public async Task LaunchAndShutdownMegafauna()
    {
        await using var pair = await PoolManager.GetServerClient();

        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();
        var entSysMan = server.ResolveDependency<IEntitySystemManager>();

        TileShapeSystem system = null;

        await server.WaitPost(() =>
        {
            system = entSysMan.GetEntitySystem<TileShapeSystem>();
        });

        await server.WaitPost(() =>
        {
            foreach (var (shapeId, expectedCount) in TestShapes)
            {
                if (!protoMan.TryIndex<TileShapePrototype>(shapeId, out var shapeProto))
                    continue;

                var shape = shapeProto.Shape;
                var pos = testMap.GridCoords;
                system.SpawnTileShape(shape, pos, "TetherEntity", out var spawned);

                Assert.That(spawned.Count,
                    Is.EqualTo(expectedCount),
                    $"{shapeId} spawned {spawned.Count} entities instead of {expectedCount}!");

                foreach (var spawn in spawned)
                {
                    entMan.DeleteEntity(spawn);
                }
            }
        });

        await pair.CleanReturnAsync();
    }
}
