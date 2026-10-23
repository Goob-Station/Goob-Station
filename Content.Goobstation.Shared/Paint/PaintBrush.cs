using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Paint;

[Serializable, NetSerializable]
public enum PaintBrushStyle : byte
{
    Round,
    Square,
    Spray,
    Calligraphy,
}

[Serializable, NetSerializable]
public enum PaintShape : byte
{
    Freehand,
    Line,
    Rectangle,
    Ellipse,
}

public static class PaintBrush
{
    public const int MaxStrokePoints = 64;
    public const int MaxStrokeLength = 512;
    public const int PointMargin = 16;
    public const int SprayDensity = 30;

    public static void Rasterize(
        ReadOnlySpan<Vector2i> points,
        PaintShape shape,
        int size,
        PaintBrushStyle style,
        int seed,
        Vector2i bounds,
        HashSet<int> output)
    {
        if (points.Length == 0 || size <= 0)
            return;

        switch (shape)
        {
            case PaintShape.Freehand:
            case PaintShape.Line:
                Polyline(points, size, style, seed, bounds, output);
                break;
            case PaintShape.Rectangle when points.Length >= 2:
                Polyline(RectanglePath(points[0], points[1]), size, style, seed, bounds, output);
                break;
            case PaintShape.Ellipse when points.Length >= 2:
                Polyline(EllipsePath(points[0], points[1]), size, style, seed, bounds, output);
                break;
        }
    }

    public static bool IsValidStroke(ReadOnlySpan<Vector2i> points, PaintShape shape, Vector2i bounds)
    {
        if (points.Length == 0 || points.Length > MaxStrokePoints)
            return false;

        if (shape != PaintShape.Freehand && points.Length != 2)
            return false;

        var length = 0;
        for (var i = 0; i < points.Length; i++)
        {
            var point = points[i];
            if (point.X < -PointMargin || point.Y < -PointMargin
                || point.X >= bounds.X + PointMargin || point.Y >= bounds.Y + PointMargin)
                return false;

            if (i == 0)
                continue;

            var previous = points[i - 1];
            length += Math.Max(Math.Abs(point.X - previous.X), Math.Abs(point.Y - previous.Y));
        }

        return shape != PaintShape.Freehand || length <= MaxStrokeLength;
    }

    private static Vector2i[] RectanglePath(Vector2i a, Vector2i b)
    {
        return new[]
        {
            a,
            new Vector2i(b.X, a.Y),
            b,
            new Vector2i(a.X, b.Y),
            a,
        };
    }

    private static Vector2i[] EllipsePath(Vector2i a, Vector2i b)
    {
        var centerX = (a.X + b.X) / 2f;
        var centerY = (a.Y + b.Y) / 2f;
        var radiusX = Math.Abs(b.X - a.X) / 2f;
        var radiusY = Math.Abs(b.Y - a.Y) / 2f;
        var segments = Math.Max(8, (int) MathF.Ceiling(MathF.Tau * MathF.Max(radiusX, radiusY) / 2f));

        var path = new Vector2i[segments + 1];
        for (var i = 0; i <= segments; i++)
        {
            var angle = MathF.Tau * i / segments;
            path[i] = new Vector2i(
                (int) MathF.Round(centerX + MathF.Cos(angle) * radiusX),
                (int) MathF.Round(centerY + MathF.Sin(angle) * radiusY));
        }

        return path;
    }

    private static void Polyline(ReadOnlySpan<Vector2i> points, int size, PaintBrushStyle style, int seed, Vector2i bounds, HashSet<int> output)
    {
        Stamp(points[0], size, style, seed, bounds, output);
        for (var i = 1; i < points.Length; i++)
        {
            Line(points[i - 1], points[i], size, style, seed, bounds, output);
        }
    }

    private static void Line(Vector2i from, Vector2i to, int size, PaintBrushStyle style, int seed, Vector2i bounds, HashSet<int> output)
    {
        var dx = Math.Abs(to.X - from.X);
        var dy = -Math.Abs(to.Y - from.Y);
        var stepX = from.X < to.X ? 1 : -1;
        var stepY = from.Y < to.Y ? 1 : -1;
        var error = dx + dy;
        var x = from.X;
        var y = from.Y;

        while (true)
        {
            Stamp(new Vector2i(x, y), size, style, seed, bounds, output);
            if (x == to.X && y == to.Y)
                break;

            var doubled = 2 * error;
            if (doubled >= dy)
            {
                error += dy;
                x += stepX;
            }

            if (doubled <= dx)
            {
                error += dx;
                y += stepY;
            }
        }
    }

    private static void Stamp(Vector2i center, int size, PaintBrushStyle style, int seed, Vector2i bounds, HashSet<int> output)
    {
        if (style == PaintBrushStyle.Calligraphy)
        {
            StampCalligraphy(center, size, bounds, output);
            return;
        }

        var start = -(size - 1) / 2;
        var middle = (size - 1) / 2f;
        var radius = size / 2f - 0.25f;
        var radiusSquared = radius * radius;

        for (var offsetY = 0; offsetY < size; offsetY++)
        {
            for (var offsetX = 0; offsetX < size; offsetX++)
            {
                if (style != PaintBrushStyle.Square)
                {
                    var fromMiddleX = offsetX - middle;
                    var fromMiddleY = offsetY - middle;
                    if (fromMiddleX * fromMiddleX + fromMiddleY * fromMiddleY > radiusSquared)
                        continue;
                }

                var x = center.X + start + offsetX;
                var y = center.Y + start + offsetY;
                if (style == PaintBrushStyle.Spray && size > 2 && !SprayHit(x, y, seed))
                    continue;

                Add(x, y, bounds, output);
            }
        }
    }

    private static void StampCalligraphy(Vector2i center, int size, Vector2i bounds, HashSet<int> output)
    {
        var start = -(size - 1) / 2;
        for (var i = 0; i < size; i++)
        {
            var x = center.X + start + i;
            var y = center.Y - start - i;
            Add(x, y, bounds, output);
            if (size > 1)
                Add(x + 1, y, bounds, output);
        }
    }

    private static bool SprayHit(int x, int y, int seed)
    {
        unchecked
        {
            var hash = (uint) (x * 73856093) ^ (uint) (y * 19349663) ^ (uint) (seed * 83492791);
            hash ^= hash >> 13;
            hash *= 0x5bd1e995;
            hash ^= hash >> 15;
            return hash % 100 < SprayDensity;
        }
    }

    private static void Add(int x, int y, Vector2i bounds, HashSet<int> output)
    {
        if (x < 0 || y < 0 || x >= bounds.X || y >= bounds.Y)
            return;

        output.Add(y * bounds.X + x);
    }
}
