using Robust.Shared.Random;

namespace Content.Server._Lavaland.Hierophant;

/// <summary>
/// Some static helper methods that help to create some tile patterns with ease.
/// </summary>
public static class TileHelperMethods
{
    public static List<Vector2i> MakeBox(Vector2i center, int range, bool hollow = false, int borderRange = 1)
    {
        var refs = new List<Vector2i>();
        var bottomLeft = center + new Vector2i(-range, -range);
        for (int y = 0; y < range; y++)
        {
            for (int x = 0; x < range; x++)
            {
                refs.Add(bottomLeft + new Vector2i(x, y));
            }
        }

        if (!hollow)
            return refs;

        borderRange = Math.Clamp(borderRange, 1, range);
        var borderOrigin = bottomLeft +  new Vector2i(borderRange, borderRange);
        var borderSize = new Vector2i(range - borderRange, range - borderRange);
        var box = Box2i.FromDimensions(borderOrigin, borderSize);
        var tiles = new List<Vector2i>(refs);
        foreach (var tile in tiles)
        {
            if (box.Contains(tile))
                refs.Remove(tile);
        }

        return refs;
    }

    public static List<Vector2i> MakeBoxRandom(Vector2i center, int range, IRobustRandom random, float filledSquareChance = 0.3f)
    {
        var refs = MakeBox(center, range);
        var refsTemp = new List<Vector2i>(refs);
        foreach (var tile in refsTemp)
        {
            if (!random.Prob(filledSquareChance))
                refs.Remove(tile);
        }

        return refs;
    }

    public static List<Vector2i> MakeCross(Vector2i center, int range)
    {
        var refs = new List<Vector2i> { center };
        for (int i = 1; i < range; i++)
        {
            refs.Add(new Vector2i(center.X + i, center.Y));
            refs.Add(new Vector2i(center.X, center.Y + i));
            refs.Add(new Vector2i(center.X - i, center.Y));
            refs.Add(new Vector2i(center.X, center.Y - i));
        }

        return refs;
    }

    public static List<Vector2i> MakeCrossDiagonal(Vector2i center, int range)
    {
        var refs = new List<Vector2i> { center };
        for (var i = 1; i < range; i++)
        {
            refs.Add(new Vector2i(center.X + i, center.Y + i));
            refs.Add(new Vector2i(center.X + i, center.Y - i));
            refs.Add(new Vector2i(center.X - i, center.Y + i));
            refs.Add(new Vector2i(center.X - i, center.Y - i));
        }

        return refs;
    }
}
