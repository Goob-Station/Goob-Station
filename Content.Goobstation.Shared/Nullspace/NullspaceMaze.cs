namespace Content.Goobstation.Shared.Nullspace;

public enum NullspaceFixtureKind : byte
{
    On,
    Flicker,
    Dead,
}

public sealed class NullspaceMaze
{
    private const int Chunk = NullspaceParams.Chunk;
    private const int Nodes = Chunk / 2;

    public static readonly Vector2i[] Directions = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

    public readonly uint Seed;

    private readonly Dictionary<Vector2i, byte[]> _chunks = new();
    private readonly Dictionary<Vector2i, int> _variants = new();

    public int AppliedVariantVersion = -1;

    public NullspaceMaze(uint seed)
    {
        Seed = seed;
    }

    public static uint Hash(uint seed, int a, int b, int salt = 0)
    {
        unchecked
        {
            var h = seed ^ (uint) a * 0x27d4eb2du ^ (uint) b * 0x165667b1u ^ (uint) salt * 0x9e3779b1u;
            h = (h ^ (h >> 15)) * 0x85ebca6bu;
            h = (h ^ (h >> 13)) * 0xc2b2ae35u;
            return h ^ (h >> 16);
        }
    }

    public static int FloorDiv(int value, int size)
    {
        var q = value / size;
        return value % size < 0 ? q - 1 : q;
    }

    public bool IsWall(int x, int y)
    {
        var cx = FloorDiv(x, Chunk);
        var cy = FloorDiv(y, Chunk);
        return GetChunk(cx, cy)[(y - cy * Chunk) * Chunk + x - cx * Chunk] == 1;
    }

    public bool IsWall(Vector2i cell)
    {
        return IsWall(cell.X, cell.Y);
    }

    public byte[] GetChunk(int cx, int cy)
    {
        var key = new Vector2i(cx, cy);
        if (!_chunks.TryGetValue(key, out var chunk))
        {
            var variant = _variants.GetValueOrDefault(key);
            chunk = Generate(variant == 0 ? Seed : Hash(Seed, variant, 911, 17), cx, cy);
            _chunks[key] = chunk;
        }

        return chunk;
    }

    public int Variant(Vector2i chunk)
    {
        return _variants.GetValueOrDefault(chunk);
    }

    public void SetVariant(Vector2i chunk, int variant)
    {
        if (_variants.GetValueOrDefault(chunk) == variant)
            return;

        _variants[chunk] = variant;
        _chunks.Remove(chunk);
    }

    public NullspaceFixtureKind? Fixture(int x, int y)
    {
        if ((x & 1) == 0 || (y & 1) == 0 || IsWall(x, y))
            return null;

        var h = Hash(Seed, x, y, 7);
        if (h % 100 >= 20)
            return null;

        var k = (h >> 8) % 100;
        return k < 58 ? NullspaceFixtureKind.On : k < 85 ? NullspaceFixtureKind.Flicker : NullspaceFixtureKind.Dead;
    }

    public NullspaceFixtureKind? Fixture(int x, int y, float darkness)
    {
        if (Fixture(x, y) is not { } kind)
            return null;

        if (darkness <= 0f || kind == NullspaceFixtureKind.Dead)
            return kind;

        if (Hash(Seed, x, y, 13) % 1000 / 1000f < darkness * NullspaceParams.DarknessDeadLights)
            return NullspaceFixtureKind.Dead;

        if (kind == NullspaceFixtureKind.On && Hash(Seed, x, y, 14) % 1000 / 1000f < darkness * NullspaceParams.DarknessFlickerLights)
            return NullspaceFixtureKind.Flicker;

        return kind;
    }

    public float FixturePhase(int x, int y)
    {
        return Hash(Seed, x, y, 9) % 1000 / 1000f;
    }

    public bool FixtureAlongX(int x, int y)
    {
        return IsWall(x, y - 1) && IsWall(x, y + 1);
    }

    public bool LineClear(float ax, float ay, float bx, float by)
    {
        var x = (int) MathF.Floor(ax);
        var y = (int) MathF.Floor(ay);
        var tx = (int) MathF.Floor(bx);
        var ty = (int) MathF.Floor(by);
        var dx = bx - ax;
        var dy = by - ay;
        var stepX = dx > 0 ? 1 : -1;
        var stepY = dy > 0 ? 1 : -1;
        var tDeltaX = dx != 0 ? MathF.Abs(1 / dx) : float.PositiveInfinity;
        var tDeltaY = dy != 0 ? MathF.Abs(1 / dy) : float.PositiveInfinity;
        var tMaxX = dx != 0 ? (dx > 0 ? x + 1 - ax : ax - x) * tDeltaX : float.PositiveInfinity;
        var tMaxY = dy != 0 ? (dy > 0 ? y + 1 - ay : ay - y) * tDeltaY : float.PositiveInfinity;
        var n = Math.Abs(tx - x) + Math.Abs(ty - y);

        for (var i = 0; i < n; i++)
        {
            if (tMaxX < tMaxY)
            {
                tMaxX += tDeltaX;
                x += stepX;
            }
            else
            {
                tMaxY += tDeltaY;
                y += stepY;
            }

            if (IsWall(x, y))
                return false;
        }

        return true;
    }

    public Vector2i? NearestOpen(int x, int y, int radius = 6)
    {
        if (!IsWall(x, y))
            return new Vector2i(x, y);

        for (var r = 1; r <= radius; r++)
        {
            for (var dy = -r; dy <= r; dy++)
            {
                for (var dx = -r; dx <= r; dx++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r)
                        continue;

                    if (!IsWall(x + dx, y + dy))
                        return new Vector2i(x + dx, y + dy);
                }
            }
        }

        return null;
    }

    private static int At(int x, int y)
    {
        return y * Chunk + x;
    }

    private static byte[] Generate(uint seed, int cx, int cy)
    {
        var grid = new byte[Chunk * Chunk];
        Array.Fill(grid, (byte) 1);

        var r = new Mulberry(Hash(seed, cx, cy, 1));
        var visited = new bool[Nodes * Nodes];
        var start = new Vector2i((int) (r.Next() * Nodes), (int) (r.Next() * Nodes));
        visited[start.Y * Nodes + start.X] = true;
        grid[At(start.X * 2 + 1, start.Y * 2 + 1)] = 0;

        var stack = new List<Vector2i> { start };
        var options = new List<Vector2i>(4);
        while (stack.Count > 0)
        {
            var node = stack[^1];
            options.Clear();
            foreach (var dir in Directions)
            {
                var next = node + dir;
                if (next.X >= 0 && next.Y >= 0 && next.X < Nodes && next.Y < Nodes && !visited[next.Y * Nodes + next.X])
                    options.Add(dir);
            }

            if (options.Count == 0)
            {
                stack.RemoveAt(stack.Count - 1);
                continue;
            }

            var step = options[(int) (r.Next() * options.Count)];
            var target = node + step;
            grid[At(node.X * 2 + 1 + step.X, node.Y * 2 + 1 + step.Y)] = 0;
            grid[At(target.X * 2 + 1, target.Y * 2 + 1)] = 0;
            visited[target.Y * Nodes + target.X] = true;
            stack.Add(target);
        }

        for (var salt = 2; salt <= 3; salt++)
        {
            var west = salt == 2;
            var openings = new Mulberry(Hash(seed, cx, cy, salt));
            var count = 2 + (int) (openings.Next() * 2);
            for (var i = 0; i < count; i++)
            {
                var k = (int) (openings.Next() * Nodes) * 2 + 1;
                grid[west ? At(0, k) : At(k, 0)] = 0;
            }
        }

        var braid = new Mulberry(Hash(seed, cx, cy, 4));
        var closed = new List<int>(4);
        for (var ny = 0; ny < Nodes; ny++)
        {
            for (var nx = 0; nx < Nodes; nx++)
            {
                var x = nx * 2 + 1;
                var y = ny * 2 + 1;
                closed.Clear();
                var open = 0;
                foreach (var dir in Directions)
                {
                    var wx = x + dir.X;
                    var wy = y + dir.Y;
                    if (wx >= Chunk || wy >= Chunk)
                        continue;

                    if (grid[At(wx, wy)] == 0)
                        open++;
                    else
                        closed.Add(At(wx, wy));
                }

                if (open == 1 && closed.Count > 0 && braid.Next() < 0.4)
                    grid[closed[(int) (braid.Next() * closed.Count)]] = 0;
            }
        }

        var rooms = new Mulberry(Hash(seed, cx, cy, 5));
        if (rooms.Next() < 0.35)
        {
            var wn = 2 + (int) (rooms.Next() * 3);
            var hn = 2 + (int) (rooms.Next() * 3);
            var nx0 = (int) (rooms.Next() * (Nodes - wn));
            var ny0 = (int) (rooms.Next() * (Nodes - hn));
            var pillars = rooms.Next() < 0.65;
            for (var y = ny0 * 2 + 1; y <= (ny0 + hn - 1) * 2 + 1; y++)
            {
                for (var x = nx0 * 2 + 1; x <= (nx0 + wn - 1) * 2 + 1; x++)
                {
                    if (pillars && x % 2 == 0 && y % 2 == 0)
                        continue;

                    grid[At(x, y)] = 0;
                }
            }
        }

        return grid;
    }

    private struct Mulberry
    {
        private uint _state;

        public Mulberry(uint seed)
        {
            _state = seed;
        }

        public double Next()
        {
            unchecked
            {
                _state += 0x6d2b79f5u;
                var t = (_state ^ (_state >> 15)) * (1u | _state);
                t = (t + (t ^ (t >> 7)) * (61u | t)) ^ t;
                return (t ^ (t >> 14)) / 4294967296.0;
            }
        }
    }
}
