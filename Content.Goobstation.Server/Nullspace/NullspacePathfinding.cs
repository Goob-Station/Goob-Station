using Content.Goobstation.Shared.Nullspace;

namespace Content.Goobstation.Server.Nullspace;

public static class NullspacePathfinding
{
    public static List<Vector2i>? FindPath(NullspaceMaze maze, Vector2i start, Vector2i goal, int maxExpand)
    {
        if (maze.IsWall(goal) || maze.IsWall(start))
            return null;

        var open = new PriorityQueue<Vector2i, int>();
        var cost = new Dictionary<Vector2i, int> { [start] = 0 };
        var came = new Dictionary<Vector2i, Vector2i>();
        open.Enqueue(start, Heuristic(start, goal));

        var expanded = 0;
        while (open.TryDequeue(out var current, out var priority) && expanded < maxExpand)
        {
            var currentCost = cost[current];
            if (priority > currentCost + Heuristic(current, goal))
                continue;

            if (current == goal)
            {
                var path = new List<Vector2i> { current };
                while (came.TryGetValue(current, out var previous))
                {
                    path.Add(previous);
                    current = previous;
                }

                path.Reverse();
                return path;
            }

            expanded++;
            foreach (var dir in NullspaceMaze.Directions)
            {
                var next = current + dir;
                if (maze.IsWall(next))
                    continue;

                var nextCost = currentCost + 1;
                if (cost.TryGetValue(next, out var known) && known <= nextCost)
                    continue;

                cost[next] = nextCost;
                came[next] = current;
                open.Enqueue(next, nextCost + Heuristic(next, goal));
            }
        }

        return null;
    }

    public static List<Vector2i> CellsAtDistance(NullspaceMaze maze, Vector2i start, int min, int max)
    {
        var seen = new HashSet<Vector2i> { start };
        var frontier = new List<Vector2i> { start };
        var next = new List<Vector2i>();
        var result = new List<Vector2i>();

        for (var distance = 1; distance <= max && frontier.Count > 0; distance++)
        {
            next.Clear();
            foreach (var cell in frontier)
            {
                foreach (var dir in NullspaceMaze.Directions)
                {
                    var neighbour = cell + dir;
                    if (maze.IsWall(neighbour) || !seen.Add(neighbour))
                        continue;

                    next.Add(neighbour);
                    if (distance >= min)
                        result.Add(neighbour);
                }
            }

            (frontier, next) = (next, frontier);
        }

        return result;
    }

    private static int Heuristic(Vector2i a, Vector2i b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
