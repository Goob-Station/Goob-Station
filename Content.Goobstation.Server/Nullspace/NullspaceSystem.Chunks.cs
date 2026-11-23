using System.Numerics;
using Content.Goobstation.Shared.Nullspace;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Nullspace;

public sealed partial class NullspaceSystem
{
    [Dependency] private ITileDefinitionManager _tileDefs = default!;

    private static readonly EntProtoId WallPrototype = "NullspaceWall";
    private static readonly ProtoId<ContentTileDefinition> FloorTile = "Plating";

    private const int Chunk = NullspaceParams.Chunk;
    private const int ChunkBudget = 150;

    private readonly HashSet<Vector2i> _anchors = new();
    private readonly List<Vector2i> _unload = new();
    private readonly List<(Vector2i, Tile)> _tiles = new();

    private void StreamChunks(Entity<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent> ent, NullspaceMaze maze, bool immediate, Vector2i? extraCell = null)
    {
        var instance = ent.Comp1;
        _anchors.Clear();
        foreach (var body in instance.Players)
        {
            if (!TerminatingOrDeleted(body))
                _anchors.Add(ChunkOf(CellOf(_transform.GetWorldPosition(body))));
        }

        if (instance.Clown is { } clown && !TerminatingOrDeleted(clown))
            _anchors.Add(ChunkOf(CellOf(_transform.GetWorldPosition(clown))));

        if (extraCell is { } extra)
            _anchors.Add(ChunkOf(extra));

        if (!immediate && instance.Anchors.SetEquals(_anchors))
            return;

        instance.Anchors.Clear();
        instance.Anchors.UnionWith(_anchors);

        foreach (var anchor in _anchors)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    var chunk = anchor + new Vector2i(dx, dy);
                    if (!instance.Chunks.ContainsKey(chunk))
                        LoadChunk(ent, maze, chunk, immediate);
                }
            }
        }

        _unload.Clear();
        foreach (var chunk in instance.Chunks.Keys)
        {
            var keep = false;
            foreach (var anchor in _anchors)
            {
                if (Math.Max(Math.Abs(chunk.X - anchor.X), Math.Abs(chunk.Y - anchor.Y)) <= 2)
                {
                    keep = true;
                    break;
                }
            }

            if (!keep)
                _unload.Add(chunk);
        }

        foreach (var chunk in _unload)
        {
            if (!instance.Chunks.Remove(chunk, out var data))
                continue;

            instance.PendingUnloads.Enqueue(new NullspaceUnload(chunk, data.Entities));
            BumpVariant(ent.Comp2, maze, chunk);
        }

        if (_unload.Count > 0)
            Dirty(ent.Owner, ent.Comp2);
    }

    private void LoadChunk(Entity<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent> ent, NullspaceMaze maze, Vector2i chunk, bool immediate)
    {
        var floor = new Tile(_tileDefs[FloorTile].TileId);
        var origin = chunk * Chunk;

        _tiles.Clear();
        for (var y = 0; y < Chunk; y++)
        {
            for (var x = 0; x < Chunk; x++)
            {
                _tiles.Add((origin + new Vector2i(x, y), floor));
            }
        }

        _map.SetTiles(ent.Owner, ent.Comp3, _tiles);

        var data = new NullspaceChunk(++ent.Comp1.ChunkGeneration);
        ent.Comp1.Chunks[chunk] = data;

        foreach (var (cell, _) in _tiles)
        {
            if (!maze.IsWall(cell))
                continue;

            if (immediate)
                data.Entities.Add(Spawn(WallPrototype, new EntityCoordinates(ent.Owner, cell + new Vector2(0.5f, 0.5f))));
            else
                ent.Comp1.PendingSpawns.Enqueue(new NullspacePendingSpawn(chunk, data.Generation, cell));
        }
    }

    private void ProcessChunkQueues(Entity<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent> ent)
    {
        var instance = ent.Comp1;
        var budget = ChunkBudget;

        while (budget > 0 && instance.PendingSpawns.TryDequeue(out var pending))
        {
            if (!instance.Chunks.TryGetValue(pending.Chunk, out var data) || data.Generation != pending.Generation)
                continue;

            data.Entities.Add(Spawn(WallPrototype, new EntityCoordinates(ent.Owner, pending.Cell + new Vector2(0.5f, 0.5f))));
            budget--;
        }

        while (budget > 0 && instance.PendingUnloads.TryPeek(out var unload))
        {
            while (budget > 0 && unload.Entities.Count > 0)
            {
                var last = unload.Entities.Count - 1;
                QueueDel(unload.Entities[last]);
                unload.Entities.RemoveAt(last);
                budget--;
            }

            if (unload.Entities.Count > 0)
                break;

            instance.PendingUnloads.Dequeue();
            if (instance.Chunks.ContainsKey(unload.Chunk))
                continue;

            var origin = unload.Chunk * Chunk;
            _tiles.Clear();
            for (var y = 0; y < Chunk; y++)
            {
                for (var x = 0; x < Chunk; x++)
                {
                    _tiles.Add((origin + new Vector2i(x, y), Tile.Empty));
                }
            }

            _map.SetTiles(ent.Owner, ent.Comp3, _tiles);
        }
    }

    private static Vector2i ChunkOf(Vector2i cell)
    {
        return new Vector2i(NullspaceMaze.FloorDiv(cell.X, Chunk), NullspaceMaze.FloorDiv(cell.Y, Chunk));
    }
}
