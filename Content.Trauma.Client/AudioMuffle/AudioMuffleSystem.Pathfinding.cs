using System.Numerics;
using System.Linq;
using Content.Trauma.Shared.AudioMuffle;
using Robust.Shared.Utility;

namespace Content.Trauma.Client.AudioMuffle;

public sealed partial class AudioMuffleSystem
{
    private readonly HashSet<MuffleTileData> _updatedData = new();
    private readonly HashSet<MuffleTileData> _reExpand = new();
    private readonly HashSet<Vector2i> _passed = new();
    private readonly HashSet<Vector2i> _rewritePassed = new();
    private readonly PriorityQueue<MuffleTileData> _frontier = new();
    private readonly Dictionary<MuffleTileData, float> _expansionNodes = new();
    private readonly Dictionary<MuffleTileData, float> _innerExpansionNodes = new();

    public static int ManhattanDistance(Vector2i start, Vector2i end)
    {
        var distance = end - start;
        return Math.Abs(distance.X) + Math.Abs(distance.Y);
    }

    public static float ManhattanDistance(Vector2 start, Vector2 end)
    {
        var distance = end - start;
        return Math.Abs(distance.X) + Math.Abs(distance.Y);
    }

    private void RebuildAndExpand(Vector2i newPos, Vector2i oldPos)
    {
        if (newPos == oldPos)
            return;

        var difference = newPos - oldPos;
        var signX = MathF.Sign(difference.X);
        var signY = MathF.Sign(difference.Y);
        var distance = ManhattanDistance(newPos, oldPos);

        if (distance >= PathfindingRange)
        {
            Expand(newPos);
            return;
        }

        if (!TileDataDict.TryGetValue(oldPos, out var oldData) || oldData.Previous != null ||
            !TileDataDict.TryGetValue(newPos, out var newData))
        {
            Expand(newPos);
            return;
        }

        var cur = newData;
        MuffleTileData? newPrev = null;
        for (var i = 0; i < PathfindingRange; i++)
        {
            cur.TotalCost = i;

            SwapPrev(cur, newPrev, out var nextTile);

            if (nextTile == null || cur.Equals(oldData))
                break;

            newPrev = cur;
            cur = nextTile;
        }

        if (!cur.Equals(oldData))
        {
            Expand(newPos);
            return;
        }

        newData.TotalCost = 0f;
        _reExpand.Clear();
        if (!ExpandNode(newData, 0f, false, _reExpand, out _))
        {
            if (_reExpand.Contains(newData))
            {
                Expand(newPos);
                return;
            }

            _passed.Clear();
            foreach (var node in _reExpand)
            {
                if (_passed.Contains(node.Indices))
                    continue;

                RewriteAndReExpand(node, _passed);
            }
            _passed.Clear();
        }
        _reExpand.Clear();

        _frontier.Clear();
        _passed.Clear();
        foreach (var (tile, data) in TileDataDict)
        {
            var isPassed = true;
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if (x != 0 && y != 0 || x == 0 && y == 0)
                        continue;

                    var neighbor = tile + new Vector2i(x, y);

                    if (neighbor == data.Previous?.Indices)
                        continue;

                    if (TileDataDict.ContainsKey(neighbor))
                        continue;

                    _frontier.Add(data);
                    isPassed = false;
                }
            }

            if (isPassed)
                _passed.Add(tile);
        }

        // This determines the side of already expanded area that we are expanding into.
        var vecX = new Vector2i(Math.Sign(signX - 1), Math.Sign(1 - signX)) * signX;
        var vecY = new Vector2i(Math.Sign(signY - 1), Math.Sign(1 - signY)) * signY;
        Expand(_frontier, newPos, _passed, vecX, vecY, false, PathfindingRange * distance);
        _frontier.Clear();
        _passed.Clear();
    }

    private void Expand(Vector2i start, bool updateAudio = false)
    {
        var newNode = new MuffleTileData(start);

        // Clear and rebuild from the newNode
        TileDataDict.Clear();
        TileDataDict[start] = newNode;

        var vec = Vector2i.One;
        _frontier.Clear();
        _frontier.Add(newNode);
        Expand(_frontier,
            start,
            new HashSet<Vector2i> { start },
            vec,
            vec,
            updateAudio);
        if (updateAudio)
            ResetAudioOnPos(start);
        _frontier.Clear();
    }

    private void Expand(PriorityQueue<MuffleTileData> frontier,
        Vector2i origin,
        HashSet<Vector2i> passed,
        Vector2i minMaxXDir,
        Vector2i minMaxYDir,
        bool updateAudio = false,
        int amount = PathfindingRange)
    {
        if (PlayerGrid is not { } grid || grid.Comp.Deleted)
            return;

        var minAbsX = Math.Abs(minMaxXDir.X);
        var maxAbsX = Math.Abs(minMaxXDir.Y);
        var minAbsY = Math.Abs(minMaxYDir.X);
        var maxAbsY = Math.Abs(minMaxYDir.Y);
        var sum = minAbsX + minAbsY + maxAbsX + maxAbsY;
        if (sum == 0)
            return;
        var max = MathF.Pow(amount, sum);
        var count = 0;

        _passed.Clear();
        while (frontier.Count > 0 && count < max)
        {
            var node = frontier.Take();
            count++;

            var cost = node.TotalCost;

            for (var x = -minAbsX; x <= maxAbsX; x++)
            {
                for (var y = -minAbsY; y <= maxAbsY; y++)
                {
                    if (x != 0 && y != 0 || x == 0 && y == 0)
                        continue;

                    var neighbor = node.Indices + new Vector2i(x, y);

                    if (passed.Contains(neighbor))
                        continue;

                    if (neighbor == node.Previous?.Indices)
                        continue;

                    if (TileDataDict.ContainsKey(neighbor))
                        continue;

                    if (ManhattanDistance(origin, neighbor) > amount)
                        continue;

                    var moveCost = 1f;

                    var score = cost + moveCost + GetTotalTileCost(neighbor);

                    if (TileDataDict.TryGetValue(neighbor, out var next))
                    {
                        var diff = score - next.TotalCost;
                        if (diff >= 0)
                            continue;

                        next.Previous = node;
                        node.Next.Add(next);

                        UpdateTotalCostOfNextTileData(next, diff, false, PathfindingRange - count);

                        frontier.Add(next);
                    }
                    else
                    {
                        if (!_map.CollidesWithGrid(grid.Owner, grid.Comp, neighbor))
                            continue;

                        var newNode = new MuffleTileData(neighbor)
                        {
                            TotalCost = score,
                            Previous = node,
                        };

                        node.Next.Add(newNode);
                        TileDataDict[neighbor] = newNode;
                        frontier.Add(newNode);
                    }

                    _passed.Add(neighbor);
                }
            }
        }

        if (updateAudio)
            ReCalculateAllAudio(null, AudioProcessBehavior.None, AudioProcessBehavior.Reset);
        _passed.Clear();
    }

    private void RewriteAndReExpand(MuffleTileData first,
        HashSet<Vector2i> invalidated,
        bool updateAudio = false,
        int amount = PathfindingRange)
    {
        if (first.Previous == null)
        {
            Expand(first.Indices, updateAudio);
            return;
        }

        _frontier.Clear();
        _rewritePassed.Clear();
        _frontier.Add(first);
        _rewritePassed.Add(first.Indices);

        InvalidateNext(first, invalidated);
        invalidated.Remove(first.Indices);
        first.Next.Clear();

        foreach (var node in TileDataDict.Values)
        {
            if (ShouldAddToQueue(node, invalidated))
                _frontier.Add(node);
        }

        var count = 0;
        while (_frontier.Count > 0 && count < Math.Pow(amount, 4))
        {
            var node = _frontier.Take();
            count++;

            if (invalidated.Contains(node.Indices))
                continue;

            var cost = node.TotalCost;

            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if (x != 0 && y != 0 || x == 0 && y == 0)
                        continue;

                    var neighbor = node.Indices + new Vector2i(x, y);

                    if (neighbor == node.Previous?.Indices)
                        continue;

                    if (_rewritePassed.Contains(neighbor))
                        continue;

                    if (!invalidated.Contains(neighbor))
                        continue;

                    if (ManhattanDistance(node.Indices, neighbor) > amount)
                        continue;

                    invalidated.Remove(neighbor);

                    var moveCost = 1f;

                    var score = cost + moveCost + GetTotalTileCost(neighbor);

                    var newNode = new MuffleTileData(neighbor)
                    {
                        TotalCost = score,
                        Previous = node,
                    };

                    node.Next.Add(newNode);
                    TileDataDict[neighbor] = newNode;
                    _frontier.Add(newNode);
                    _rewritePassed.Add(neighbor);
                }
            }
        }

        if (updateAudio)
            ReCalculateAllAudio(null, AudioProcessBehavior.None, AudioProcessBehavior.Reset);
        _rewritePassed.Clear();
        _frontier.Clear();
    }

    private bool ShouldAddToQueue(MuffleTileData node, HashSet<Vector2i> invalidated)
    {
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x != 0 && y != 0 || x == 0 && y == 0)
                    continue;

                var neighbor = node.Indices + new Vector2i(x, y);

                if (invalidated.Contains(neighbor))
                    return true;
            }
        }

        return false;
    }

    private void InvalidateNext(MuffleTileData node, HashSet<Vector2i> invalidatedIndices, bool invalidateSelf = false)
    {
        if (invalidatedIndices.Contains(node.Indices))
            return;

        if (invalidateSelf)
            TileDataDict.Remove(node.Indices);

        invalidatedIndices.Add(node.Indices);

        foreach (var next in node.Next)
        {
            InvalidateNext(next, invalidatedIndices, true);
        }
    }

    private bool ExpandNode(MuffleTileData node,
        float delta,
        bool resetAudio,
        HashSet<MuffleTileData> nodesToReExpand,
        out HashSet<MuffleTileData> nextNodesToExpand,
        bool firstIteration = true,
        int iteration = PathfindingRange)
    {
        nextNodesToExpand = new();

        if (iteration <= 0)
            return true;

        if (nodesToReExpand.Contains(node))
            return true;

        if (firstIteration)
            _updatedData.Clear();
        else if (_updatedData.Contains(node))
            return true;

        _updatedData.Add(node);

        node.TotalCost += delta;
        _expansionNodes.Clear();
        GetExpansionNodes(node, _expansionNodes);
        var result = true;
        foreach (var (next, score) in _expansionNodes)
        {
            if (node.Previous == next)
                continue;

            nextNodesToExpand.Add(next);

            var diff = score - next.TotalCost;

            if (next.Previous != node && diff <= 0 || node.Previous == null)
            {
                SwapPrev(next, node, out _);
                next.TotalCost = score;

                nodesToReExpand.Add(node);
                result = false;
            }
            else if (next.Previous == node && diff > 0 && !ConnectToNewPreviousNode(next))
            {
                nodesToReExpand.Add(node);
                result = false;
            }
        }

        _expansionNodes.Clear();

        if (!result)
            return false;

        var toUpdate = (node.Previous?.TotalCost ?? -1f) + 1f;
        if (iteration <= 1)
        {
            if (firstIteration)
                UpdateTotalCostOfNextTileData(node, toUpdate, resetAudio, iteration, true);
            return true;
        }

        _frontier.Clear();
        foreach (var next in nextNodesToExpand)
        {
            if (!_updatedData.Contains(next))
                _frontier.Add(next);
        }

        var count = 0;
        while (_frontier.Count > 0 && count < Math.Pow(PathfindingRange, 4))
        {
            var next = _frontier.Take();
            count++;

            if (!ExpandNode(next, 0f, resetAudio, nodesToReExpand, out var nodes, false, 1))
                result = false;

            foreach (var toAdd in nodes)
            {
                if (_updatedData.Contains(toAdd))
                    continue;

                _frontier.Add(toAdd);
            }
        }

        _frontier.Clear();

        UpdateTotalCostOfNextTileData(node, toUpdate, resetAudio, iteration, true);

        return result;
    }

    private bool ConnectToNewPreviousNode(MuffleTileData node)
    {
        _innerExpansionNodes.Clear();
        GetExpansionNodes(node, _innerExpansionNodes);
        var list = _innerExpansionNodes.Keys.ToList();
        if (list.Count == 0)
            return true;

        list.Sort();
        var result = list[^1];
        _innerExpansionNodes.Clear();
        if (node.Previous == result)
            return true;

        var prev = result.Previous;

        if (prev == null || prev != node)
        {
            SwapPrev(node, result, out _);
            node.TotalCost = result.TotalCost + 1f + GetTotalTileCost(node.Indices);
        }
        else
            return false; // It's easier to rebuild this shit than trying to figure this out...

        return true;
    }

    private void GetExpansionNodes(MuffleTileData node, Dictionary<MuffleTileData, float> expansionNodes)
    {
        var cost = node.TotalCost;
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x != 0 && y != 0 || x == 0 && y == 0)
                    continue;

                var neighbor = node.Indices + new Vector2i(x, y);

                if (!TileDataDict.TryGetValue(neighbor, out var next))
                    continue;

                if (node.Previous == next)
                    continue;

                var moveCost = 1f;

                var score = moveCost + GetTotalTileCost(neighbor) + cost;

                expansionNodes[next] = score;
            }
        }
    }

    private void SwapPrev(MuffleTileData data, MuffleTileData? newPrevTile, out MuffleTileData? nextPrevious)
    {
        nextPrevious = data.Previous;

        if (nextPrevious != null && nextPrevious == newPrevTile)
            return;

        data.Previous = newPrevTile;

        if (nextPrevious != null)
            nextPrevious.Next.Remove(data);

        if (newPrevTile == null)
            return;

        data.Next.Remove(newPrevTile);
        newPrevTile.Next.Add(data);
    }

    private void UpdateTotalCostOfNextTileData(MuffleTileData data,
        float delta,
        bool resetAudio,
        int iteration = PathfindingRange,
        bool reCalculate = false,
        bool firstIteration = true)
    {
        if (firstIteration)
            _updatedData.Clear();
        else if (_updatedData.Contains(data))
            return;

        var cost = reCalculate ? GetTotalTileCost(data.Indices) : data.TotalCost;
        var newCost = MathF.Max(0f, cost + delta);

        data.TotalCost = newCost;

        if (resetAudio)
            ResetAudioOnPos(data.Indices);

        _updatedData.Add(data);

        if (iteration <= 0)
            return;

        var nextDelta = reCalculate ? newCost + 1f : delta;
        foreach (var next in data.Next)
        {
            UpdateTotalCostOfNextTileData(next, nextDelta, resetAudio, iteration - 1, reCalculate, false);
        }
    }

    private void AddOrRemoveBlocker(Entity<SoundBlockerComponent> blocker,
        Vector2i indices,
        bool add,
        bool modifyCost,
        bool resetAudio = false)
    {
        if (add)
        {
            blocker.Comp.Indices = indices;
            ReverseBlockerIndicesDict.GetOrNew(indices).Add(blocker);
        }
        else if (blocker.Comp.Indices == indices && ReverseBlockerIndicesDict.TryGetValue(indices, out var blockers))
        {
            blocker.Comp.Indices = null;
            blockers.Remove(blocker);
            if (blockers.Count == 0)
                ReverseBlockerIndicesDict.Remove(indices);
        }
        else
            return;

        if (!modifyCost)
            return;

        if (!TileDataDict.TryGetValue(indices, out var data))
            return;

        var cost = GetBlockerCost(blocker.Comp);
        var sign = add ? 1 : -1;

        ModifyBlockerAmount(data, sign * cost, resetAudio);
    }

    private void ModifyBlockerAmount(MuffleTileData data, float delta, bool resetAudio = false)
    {
        if (delta < 0 && delta < -data.TotalCost)
            delta = -data.TotalCost;

        _reExpand.Clear();
        if (ExpandNode(data, delta, resetAudio, _reExpand, out _, true, 1))
            return;

        _passed.Clear();
        foreach (var node in _reExpand)
        {
            if (_passed.Contains(node.Indices))
                continue;

            RewriteAndReExpand(node, _passed);
        }
        _reExpand.Clear();
        _passed.Clear();
    }

    public sealed class MuffleTileData(Vector2i indices) : IEquatable<MuffleTileData>, IComparable<MuffleTileData>
    {
        public readonly Vector2i Indices = indices;

        public float TotalCost;

        public MuffleTileData? Previous;

        public HashSet<MuffleTileData> Next = new(4);

        public bool Equals(MuffleTileData? other)
        {
            return other != null && Indices.Equals(other.Indices);
        }

        public int CompareTo(MuffleTileData? other)
        {
            return other == null ? 1 : other.TotalCost.CompareTo(TotalCost);
        }

        public override int GetHashCode()
        {
            return Indices.GetHashCode();
        }
    }
}
