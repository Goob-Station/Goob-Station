// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;

namespace Content.Shared._Funkystation.MalfAI.Factory;

/// <summary>
/// Shared tile-validity logic for MalfAI factory placement.
/// Uses anchored entities + FixturesComponent instead of the physics broadphase,
/// so it works identically on both client and server.
/// </summary>
public sealed class SharedMalfAiFactorySystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;

    private const int ImpassableLayer = (int) CollisionGroup.Impassable;

    /// <summary>
    /// Returns true if the tile at <paramref name="coordinates"/> has a floor and no impassable fixture.
    /// Also outputs the snapped tile-center coordinates for use as a spawn position.
    /// </summary>
    public bool IsTileFree(EntityCoordinates coordinates, out EntityCoordinates tileCenter)
    {
        tileCenter = coordinates;

        var gridUid = _transform.GetGrid(coordinates);
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var mapGrid))
            return false;

        var tileIndices = _mapSystem.TileIndicesFor(gridUid.Value, mapGrid, coordinates);
        var tile = _mapSystem.GetTileRef(gridUid.Value, mapGrid, tileIndices);
        if (tile.Tile.IsEmpty)
            return false;

        var anchorEnum = _mapSystem.GetAnchoredEntitiesEnumerator(gridUid.Value, mapGrid, tileIndices);
        while (anchorEnum.MoveNext(out var anchEnt))
        {
            if (!TryComp<FixturesComponent>(anchEnt.Value, out var fixtures))
                continue;
            foreach (var fixture in fixtures.Fixtures.Values)
            {
                if (fixture.Hard && (fixture.CollisionLayer & ImpassableLayer) != 0)
                    return false;
            }
        }

        tileCenter = _mapSystem.GridTileToLocal(gridUid.Value, mapGrid, tileIndices);
        return true;
    }
}
