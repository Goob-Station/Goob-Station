using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Nullspace;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Nullspace;

public sealed partial class NullspaceSystem
{
    private static readonly TimeSpan SilenceCheckInterval = TimeSpan.FromSeconds(0.25);
    private static readonly TimeSpan SilenceRepeat = TimeSpan.FromSeconds(1.5);
    private const float SilenceSeconds = 3f;
    private const float SilenceLead = 2.5f;
    private const float SilenceRange = 16f;

    private const float ShiftSightRange = 14f;

    private void UpdateHorror(Entity<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent> ent, NullspaceMaze maze)
    {
        var now = _timing.CurTime;
        if (now >= ent.Comp1.SilenceCheckAt)
        {
            ent.Comp1.SilenceCheckAt = now + SilenceCheckInterval;
            UpdateSilence(ent.Comp1, maze, now);
        }

        if (ent.Comp1.NextShiftAt == TimeSpan.Zero)
            ent.Comp1.NextShiftAt = now + TimeSpan.FromSeconds(_random.NextFloat(20f, 30f));
        else if (now >= ent.Comp1.NextShiftAt)
        {
            ent.Comp1.NextShiftAt = now + TimeSpan.FromSeconds(_random.NextFloat(12f, 25f));
            ShiftChunk(ent, maze);
        }
    }

    private void UpdateSilence(NullspaceInstanceComponent instance, NullspaceMaze maze, TimeSpan now)
    {
        if (instance.Clown is not { } clown
            || TerminatingOrDeleted(clown)
            || !TryComp<NullspaceClownComponent>(clown, out var state)
            || state.State is not (NullspaceClownState.Hunt or NullspaceClownState.Search))
        {
            return;
        }

        var clownTiles = _transform.GetWorldPosition(clown);
        var clownCell = CellOf(clownTiles);
        var reach = Math.Max(1, (int) MathF.Ceiling(ClownSpeed(state.State, 0f) / Cell * SilenceLead));
        List<Vector2i>? ahead = null;

        foreach (var body in instance.Players)
        {
            if (!IsHuntable(body)
                || !TryComp<NullspaceTrackerComponent>(body, out var tracker)
                || now < tracker.SilenceAt)
            {
                continue;
            }

            var playerTiles = _transform.GetWorldPosition(body);
            if ((playerTiles - clownTiles).Length() > SilenceRange
                || maze.LineClear(clownTiles.X, clownTiles.Y, playerTiles.X, playerTiles.Y))
            {
                continue;
            }

            ahead ??= NullspacePathfinding.CellsAtDistance(maze, clownCell, 1, reach);
            foreach (var cell in ahead)
            {
                if (!maze.LineClear(cell.X + 0.5f, cell.Y + 0.5f, playerTiles.X, playerTiles.Y))
                    continue;

                SendSilence(body, tracker);
                break;
            }
        }
    }

    private void SendSilence(EntityUid body, NullspaceTrackerComponent tracker)
    {
        tracker.SilenceAt = _timing.CurTime + SilenceRepeat;
        if (_player.TryGetSessionById(tracker.User, out var session))
            RaiseNetworkEvent(new NullspaceSilenceEvent(SilenceSeconds), session);
    }

    private void ShiftChunk(Entity<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent> ent, NullspaceMaze maze)
    {
        var instance = ent.Comp1;
        var occupied = new HashSet<Vector2i>(instance.Anchors);
        var candidates = instance.Chunks.Keys.Where(c => !occupied.Contains(c)).ToList();
        _random.Shuffle(candidates);

        foreach (var chunk in candidates)
        {
            if (ChunkSeen(instance, maze, chunk))
                continue;

            BumpVariant(ent.Comp2, maze, chunk);
            if (instance.Chunks.Remove(chunk, out var data))
                instance.PendingUnloads.Enqueue(new NullspaceUnload(chunk, data.Entities));

            LoadChunk(ent, maze, chunk, false);
            Dirty(ent.Owner, ent.Comp2);
            return;
        }
    }

    private bool ChunkSeen(NullspaceInstanceComponent instance, NullspaceMaze maze, Vector2i chunk)
    {
        var origin = chunk * Chunk;
        foreach (var body in instance.Players)
        {
            if (TerminatingOrDeleted(body))
                continue;

            var tiles = _transform.GetWorldPosition(body);
            for (var y = -1; y <= Chunk; y++)
            {
                for (var x = -1; x <= Chunk; x++)
                {
                    var cell = origin + new Vector2i(x, y);
                    var center = cell + new Vector2(0.5f, 0.5f);
                    if ((center - tiles).Length() > ShiftSightRange || maze.IsWall(cell))
                        continue;

                    if (maze.LineClear(tiles.X, tiles.Y, center.X, center.Y))
                        return true;
                }
            }
        }

        return false;
    }

    private void BumpVariant(NullspaceMapComponent map, NullspaceMaze maze, Vector2i chunk)
    {
        var variant = map.ChunkVariants.GetValueOrDefault(chunk) + 1;
        map.ChunkVariants[chunk] = variant;
        map.VariantVersion++;
        maze.SetVariant(chunk, variant);
        maze.AppliedVariantVersion = map.VariantVersion;
    }
}
