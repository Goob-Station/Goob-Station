// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Procedural;
using Content.Shared._Lavaland.Procedural.Prototypes;
using Content.Shared.Decals;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Procedural.Systems;

public sealed partial class LavalandSystem
{
    private void SetupRuins(LavalandRuinPoolPrototype pool, Entity<LavalandMapComponent> lavaland, Entity<LavalandPreloaderComponent> preloader)
    {
        var random = new Random(lavaland.Comp.Seed);

        var boundary = GetOutpostBoundary(lavaland);
        if (boundary == null)
            return;

        var coords = GetCoordinates(pool.RuinDistance, pool.MaxDistance);
        random.Shuffle(coords);
        var usedSpace = new List<Box2> { boundary.Value };

        // Load grid ruins
        SetupHugeRuins(pool.GridRuins, lavaland, preloader, random, ref coords, ref usedSpace);

        // Create a new list that excludes all already used spaces that intersect with big ruins.
        // Sweet optimization (another lag machine).
        var newCoords = coords.ToHashSet();
        foreach (var usedBox in usedSpace)
        {
            var list = coords.Where(coord => !usedBox.Contains(coord)).ToHashSet();
            newCoords = newCoords.Concat(list).ToHashSet();
        }

        coords = newCoords.ToList();

        // Load dungeon ruins
        // TODO: make it actual dungeons instead of spawning markers
        SetupDungeonRuins(pool.DungeonRuins, lavaland, random, ref coords, ref usedSpace);
    }

    private void SetupHugeRuins(
        Dictionary<ProtoId<LavalandGridRuinPrototype>, ushort> ruins,
        Entity<LavalandMapComponent> lavaland,
        Entity<LavalandPreloaderComponent> preloader,
        Random random,
        ref List<Vector2i> coords,
        ref List<Box2> usedSpace)
    {
        // Get and sort all ruins, because we can't sort dictionaries
        var list = GetGridRuinProtos(ruins);
        list.Sort((x, y) => x.Priority.CompareTo(y.Priority));

        // Place them down randomly
        foreach (var ruin in list)
        {
            var attempts = 0;
            while (!LoadGridRuin(ruin, lavaland, preloader, random, ref usedSpace, ref coords))
            {
                attempts++;
                if (attempts > ruin.SpawnAttemps)
                    break;
            }
        }
    }

    private void SetupDungeonRuins(
        Dictionary<ProtoId<LavalandDungeonRuinPrototype>, ushort> ruins,
        Entity<LavalandMapComponent> lavaland,
        Random random,
        ref List<Vector2i> coords,
        ref List<Box2> usedSpace)
    {
        // Get and sort all ruins, because we can't sort dictionaries
        var list = GetDungeonRuinProtos(ruins);
        list.Sort((x, y) => x.Priority.CompareTo(y.Priority));

        // Place them down randomly
        foreach (var ruin in list)
        {
            var attempts = 0;
            while (!LoadDungeonRuin(ruin, lavaland, random, ref usedSpace, ref coords))
            {
                attempts++;
                if (attempts > ruin.SpawnAttemps)
                    break;
            }
        }
    }

    private Box2? GetOutpostBoundary(Entity<LavalandMapComponent> lavaland, FixturesComponent? manager = null, TransformComponent? xform = null)
    {
        var uid = lavaland.Comp.Outpost;

        if (!Resolve(uid, ref manager, ref xform) || xform.MapUid != lavaland)
            return null;

        var aabbs = new Box2();

        var transform = _physics.GetRelativePhysicsTransform((uid, xform), xform.MapUid.Value);
        foreach (var fixture in manager.Fixtures.Values)
        {
            if (!fixture.Hard)
                return null;

            var aabb = fixture.Shape.ComputeAABB(transform, 0);
            aabbs = aabbs.Union(aabb);
        }

        aabbs = aabbs.Enlarged(8f);
        return aabbs;
    }

    private bool LoadGridRuin(
        LavalandGridRuinPrototype ruin,
        Entity<LavalandMapComponent> lavaland,
        Entity<LavalandPreloaderComponent> preloader,
        Random random,
        ref List<Box2> usedSpace,
        ref List<Vector2i> coords)
    {
        if (coords.Count == 0)
            return false;

        var coord = random.Pick(coords);
        var mapXform = Transform(preloader);

        // Check if we already calculated that boundary before, and if we didn't then calculate it now
        if (!_mapLoader.TryLoadGrid(mapXform.MapID, ruin.Path, out var spawnedBoundedGrid))
        {
            Log.Error($"Failed to load ruin {ruin.ID} onto dummy map, on stage of loading! AAAAA!!");
            return false;
        }

        // It's not useless!
        var spawned = spawnedBoundedGrid.Value;
        var ruinBox = spawnedBoundedGrid.Value.Comp.LocalAABB.Translated(coord);

        // Teleport it into place on preloader map
        _transform.SetCoordinates(spawned, new EntityCoordinates(preloader, coord));

        // If any used boundary intersects with current boundary, return
        if (usedSpace.Any(used => used.Intersects(ruinBox)))
        {
            Log.Debug("Ruin can't be placed on it's coordinates, skipping spawn");
            return false;
        }

        if (ruin.PatchToPlanet)
            GoidaMerge(spawned, (lavaland.Owner, _gridQuery.Comp(lavaland.Owner)), coord, out ruinBox);
        else
        {
            var spawnedXForm = _xformQuery.GetComponent(spawned);
            _metaData.SetEntityName(spawned, Loc.GetString(ruin.Name));
            _transform.SetParent(spawned, spawnedXForm, lavaland);
            _transform.SetCoordinates(spawned,
                new EntityCoordinates(lavaland, spawnedXForm.Coordinates.Position.Rounded()));

            var componentsToGrant = EnsureComp<LavalandGridGrantComponent>(spawned);
            foreach (var (key, comp) in ruin.ComponentsToGrant)
                componentsToGrant.ComponentsToGrant[key] = comp;
        }

        usedSpace.Add(ruinBox);
        coords.Remove(coord);
        return true;
    }

    private bool LoadDungeonRuin(
        LavalandDungeonRuinPrototype ruin,
        Entity<LavalandMapComponent> lavaland,
        Random random,
        ref List<Box2> usedSpace,
        ref List<Vector2i> coords)
    {
        if (coords.Count == 0)
            return false;

        var coord = random.Pick(coords);
        var box = Box2.CentredAroundZero(ruin.Boundary);
        var ruinBox = box.Translated(coord);

        // If any used boundary intersects with current boundary, return
        if (usedSpace.Any(used => used.Intersects(ruinBox)))
        {
            Log.Debug("Ruin can't be placed on it's coordinates, skipping spawn");
            return false;
        }

        // Spawn the marker
        Spawn(ruin.SpawnedMarker, new EntityCoordinates(lavaland, coord));

        usedSpace.Add(ruinBox);
        coords.Remove(coord);
        return true;
    }

    private List<Vector2i> GetCoordinates(int distance, int maxDistance)
    {
        var coords = new List<Vector2i>();
        var moveVector = new Vector2i(maxDistance, maxDistance);

        while (moveVector.Y >= -maxDistance)
        {
            // i love writing shitcode
            // Moving like a snake through the entire map placing all dots onto its places.

            while (moveVector.X > -maxDistance)
            {
                coords.Add(moveVector);
                moveVector += new Vector2i(-distance, 0);
            }

            coords.Add(moveVector);
            moveVector += new Vector2i(0, -distance);

            while (moveVector.X < maxDistance)
            {
                coords.Add(moveVector);
                moveVector += new Vector2i(distance, 0);
            }

            coords.Add(moveVector);
            moveVector += new Vector2i(0, -distance);
        }

        return coords;
    }

    private List<LavalandGridRuinPrototype> GetGridRuinProtos(Dictionary<ProtoId<LavalandGridRuinPrototype>, ushort> protos)
    {
        var list = new List<LavalandGridRuinPrototype>();

        foreach (var (protoId, count) in protos)
        {
            var proto = _proto.Index(protoId);
            for (var i = 0; i < count; i++)
            {
                list.Add(proto);
            }
        }

        return list;
    }

    private List<LavalandDungeonRuinPrototype> GetDungeonRuinProtos(Dictionary<ProtoId<LavalandDungeonRuinPrototype>, ushort> protos)
    {
        var list = new List<LavalandDungeonRuinPrototype>();
        foreach (var (protoId, count) in protos)
        {
            var proto = _proto.Index(protoId);
            for (var i = 0; i < count; i++)
            {
                list.Add(proto);
            }
        }

        return list;
    }

    private readonly List<(Vector2i, Tile)> _tiles = new();

    /// <summary>
    /// Tricky and sneaky method that patches some grid to a lavaland planet
    /// by teleporting all entities and copying all tiles & decals.
    /// Of course, it deletes the grid right after.
    /// </summary>
    private void GoidaMerge(
        Entity<MapGridComponent> grid,
        Entity<MapGridComponent> lavaland,
        Vector2i offset,
        out Box2 usedSpace,
        HashSet<Vector2i>? reservedTiles = null)
    {
        usedSpace = grid.Comp.LocalAABB.Translated(offset);
        var center = usedSpace.Center;
        var roomTransform = Matrix3Helpers.CreateTranslation(center.X, center.Y);

        // Copy all tiles
        _tiles.Clear();

        var tiles = _map.GetAllTiles(grid.Owner, grid.Comp).ToList();
        foreach (var tileRef in tiles)
        {
            _tiles.Add((tileRef.GridIndices + offset, tileRef.Tile));
        }

        _map.SetTiles(lavaland.Owner, lavaland.Comp, _tiles);

        // Teleport all entities
        var ents = new HashSet<Entity<TransformComponent>>();
        _lookup.GetChildEntities(grid, ents);
        foreach (var (teleportEnt, xform) in ents)
        {
            var anchored = xform.Anchored;
            var newPos = new EntityCoordinates(lavaland.Owner, xform.LocalPosition + offset);
            _transform.SetParent(teleportEnt, lavaland);
            _transform.SetCoordinates(teleportEnt, newPos);

            if (anchored)
                _transform.AnchorEntity(teleportEnt);
        }

        // Spawn decals
        if (TryComp<DecalGridComponent>(grid.Owner, out var loadedDecals))
        {
            EnsureComp<DecalGridComponent>(lavaland);
            foreach (var (_, decal) in _decals.GetDecalsIntersecting(grid.Owner, usedSpace, loadedDecals))
            {
                // Offset by 0.5 because decals are offset from bot-left corner
                // So we convert it to center of tile then convert it back again after transform.
                // Do these shenanigans because 32x32 decals assume as they are centered on bottom-left of tiles.
                var position = Vector2.Transform(decal.Coordinates + grid.Comp.TileSizeHalfVector - center,
                    roomTransform);
                position -= grid.Comp.TileSizeHalfVector;

                if (reservedTiles?.Contains(position.Floored()) == true)
                    continue;

                // Umm uhh I love decals so uhhhh idk what to do about this
                var angle = decal.Angle.Reduced();

                // Adjust because 32x32 so we can't rotate cleanly
                // Yeah idk about the uhh vectors here but it looked visually okay but they may still be off by 1.
                // Also EyeManager.PixelsPerMeter should really be in shared.
                if (angle.Equals(Math.PI))
                {
                    position += new Vector2(-1f / 32f, 1f / 32f);
                }
                else if (angle.Equals(-Math.PI / 2f))
                {
                    position += new Vector2(-1f / 32f, 0f);
                }
                else if (angle.Equals(Math.PI / 2f))
                {
                    position += new Vector2(0f, 1f / 32f);
                }
                else if (angle.Equals(Math.PI * 1.5f))
                {
                    // I hate this but decals are bottom-left rather than center position and doing the
                    // matrix ops is a PITA hence this workaround for now; I also don't want to add a stupid
                    // field for 1 specific op on decals
                    if (decal.Id != "DiagonalCheckerAOverlay" &&
                        decal.Id != "DiagonalCheckerBOverlay")
                    {
                        position += new Vector2(-1f / 32f, 0f);
                    }
                }

                var tilePos = position.Floored();

                // Fallback because uhhhhhhhh yeah, a corner tile might look valid on the original
                // but place 1 nanometre off grid and fail the add.
                if (!_map.TryGetTileRef(grid, grid, tilePos, out var tileRef) || tileRef.Tile.IsEmpty)
                {
                    _map.SetTile(grid,
                        grid,
                        tilePos,
                        _tile.GetVariantTile((ContentTileDefinition) _tiledef[DungeonSystem.FallbackTileId],
                            _random.GetRandom()));
                }

                _decals.TryAddDecal(
                    decal.Id,
                    new EntityCoordinates(grid, position),
                    out _,
                    decal.Color,
                    angle,
                    decal.ZIndex,
                    decal.Cleanable);
            }
        }
    }
}
