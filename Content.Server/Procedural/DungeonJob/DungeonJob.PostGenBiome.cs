// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading.Tasks;
using Content.Server.Parallax;
using Content.Shared.Maps;
using Content.Shared.Procedural;
using Content.Shared.Procedural.PostGeneration;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server.Procedural.DungeonJob;

public sealed partial class DungeonJob
{
    /// <summary>
    /// <see cref="BiomeDunGen"/>
    /// </summary>
    private async Task PostGen(BiomeDunGen dunGen, DungeonData data, Dungeon dungeon, HashSet<Vector2i> reservedTiles, Random random)
    {
        if (!_prototype.TryIndex(dunGen.BiomeTemplate, out var indexedBiome))
            return;

        var biomeSystem = _entManager.System<BiomeSystem>();

        var seed = random.Next();
        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();

        var tiles = _maps.GetAllTilesEnumerator(_gridUid, _grid);
        while (tiles.MoveNext(out var tileRef))
        {
            var node = tileRef.Value.GridIndices;

            if (reservedTiles.Contains(node))
                continue;
            
            if (dunGen.TileMask is not null)
            {
                if (!dunGen.TileMask.Contains(((ContentTileDefinition) _tileDefManager[tileRef.Value.Tile.TypeId]).ID))
                    continue;
            }

            // Need to set per-tile to override data.
            if (biomeSystem.TryGetTile(node, indexedBiome.Layers, seed, _grid, out var tile))
            {
                _maps.SetTile(_gridUid, _grid, node, tile.Value);
            }

            if (biomeSystem.TryGetDecals(node, indexedBiome.Layers, seed, _grid, out var decals))
            {
                foreach (var decal in decals)
                {
                    _decals.TryAddDecal(decal.ID, new EntityCoordinates(_gridUid, decal.Position), out _);
                }
            }

            if (biomeSystem.TryGetEntity(node, indexedBiome.Layers, tile ?? tileRef.Value.Tile, seed, _grid, out var entityProto))
            {
                var ent = _entManager.SpawnEntity(entityProto, new EntityCoordinates(_gridUid, node + _grid.TileSizeHalfVector));
                var xform = xformQuery.Get(ent);

                if (!xform.Comp.Anchored)
                {
                    _transform.AnchorEntity(ent, xform);
                }

                // TODO: Engine bug with SpawnAtPosition
                DebugTools.Assert(xform.Comp.Anchored);
            }

            await SuspendDungeon();
            if (!ValidateResume())
                return;
        }
    }
}