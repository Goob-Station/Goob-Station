// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Random;

// ReSharper disable EnforceForStatementBraces
namespace Content.Shared._Lavaland.Tile;

/// <summary>
/// Some static helper methods that help to create some tile patterns with ease.
/// </summary>
public static class TileHelperMethods
{
    public static List<Vector2i> MakeBox(Vector2i center, int range, bool hollow)
    {
        return hollow ? MakeBoxHollow(center, range) : MakeBox(center, range);
    }

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
        var boxTiles = new List<Vector2i>();
        var length = range * 2;

        // Calculate the starting position (top-left corner) of the box
        var startX = (int) Math.Round(center.X - (length - 1) / 2.0);
        var startY = (int) Math.Round(center.Y - (length - 1) / 2.0);

        // Top side
        for (var x = 0; x < length; x++)
            boxTiles.Add(new Vector2i(startX + x, startY));
        // Right side
        for (var y = 1; y < length - 1; y++)
            boxTiles.Add(new Vector2i(startX + length - 1, startY + y));
        // Bottom side
        for (var x = length - 1; x >= 0; x--)
            boxTiles.Add(new Vector2i(startX + x, startY + length - 1));
        // Left side
        for (var y = length - 2; y > 0; y--)
            boxTiles.Add(new Vector2i(startX, startY + y));

        return boxTiles;
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
