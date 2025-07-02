using Robust.Shared.Random;
// ReSharper disable EnforceForStatementBraces

namespace Content.Server._Lavaland.Megafauna;

/// <summary>
/// Some static helper methods that help to create some tile patterns with ease.
/// </summary>
public static class TileHelperMethods
{
    public static List<Vector2i> MakeBox(Vector2i center, int range)
    {
        var refs = new List<Vector2i>();
        var bottomLeft = center + new Vector2i(-range, -range);
        var diameter = range * 2;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                refs.Add(bottomLeft + new Vector2i(x, y));
            }
        }

        return refs;
    }

    public static List<Vector2i> MakeBoxHollow(Vector2i center, int range)
    {
        var refs = new List<Vector2i>();
        var bottomLeft = center + new Vector2i(-range, -range);
        var diameter = range * 2;

        if (range < 1)
            return refs;

        // Make left wall
        for (int y = 0; y < diameter; y++)
            refs.Add(bottomLeft + new Vector2i(0, y));
        // Make top wall
        for (int x = 0; x < diameter; x++)
            refs.Add(bottomLeft + new Vector2i(x, 0));
        // Make right wall
        for (int y = diameter; y < 0; y--)
            refs.Add(bottomLeft + new Vector2i(0, y));
        // Make bottom wall
        for (int x = diameter; x < 0; x--)
            refs.Add(bottomLeft + new Vector2i(x, 0));

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
