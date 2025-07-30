// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Robust.Shared.Random;

namespace Content.Shared._Lavaland.Tile;

/// <summary>
/// Some static helper methods that help to create some tile patterns with ease.
/// Allows to reuse already written methods for generating shapes, so making new
/// TileShape classes becomes much easier.
/// </summary>
public static class TileHelperMethods
{
    /// <summary>
    /// Draws a simple line in a specified direction, adding Step vector Range
    /// times, starting from the center and returning the result.
    /// </summary>
    public static IEnumerable<Vector2i> MakeLine(Vector2i center, int range, Vector2i step)
    {
        var refs = new List<Vector2i> { center };

        if (step == Vector2i.Zero)
            return refs;

        var curStep = new Vector2i(center.X, center.Y);
        for (int i = 0; i < range; i++)
        {
            curStep += step;
            refs.Add(curStep);
        }

        return refs;
    }

    public static IEnumerable<Vector2i> MakeBox(Vector2i center, int range, bool hollow)
    {
        return hollow ? MakeBoxHollow(center, range) : MakeBoxFilled(center, range);
    }

    public static IEnumerable<Vector2i> MakeBoxFilled(Vector2i center, int range)
    {
        if (range <= 0)
            yield break;

        if (range == 1)
        {
            yield return center;
            yield break;
        }

        var startPoint = center - new Vector2i(range / 2, range / 2);

        for (int y = 0; y < range; y++)
        {
            for (int x = 0; x < range; x++)
            {
                yield return startPoint + new Vector2i(x, y);
            }
        }
    }

    public static IEnumerable<Vector2i> MakeBoxHollow(Vector2i center, int range)
    {
        if (range <= 0)
            yield break;

        if (range == 1)
        {
            yield return center;
            yield break;
        }

        var startPoint = center - new Vector2i(range / 2, range / 2);

        // Left side
        for (int i = 0; i < range - 1; i++)
        {
            yield return startPoint + Vector2i.Up * i;
        }
        // Top side
        for (int i = 0; i < range - 1; i++)
        {
            yield return startPoint + Vector2i.Right * i;
        }
        // Right side
        for (int i = 0; i < range - 1; i++)
        {
            yield return startPoint + Vector2i.Down * i;
        }
        // Bottom side
        for (int i = 0; i < range - 1; i++)
        {
            yield return startPoint + Vector2i.Left * i;
        }
    }

    public static IEnumerable<Vector2i> MakeCross(Vector2i center, int range)
    {
        yield return center;

        if (range <= 0)
            yield break;

        for (int i = 1; i < range; i++)
        {
            yield return new Vector2i(center.X + i, center.Y);
            yield return new Vector2i(center.X, center.Y + i);
            yield return new Vector2i(center.X - i, center.Y);
            yield return new Vector2i(center.X, center.Y - i);
        }
    }

    public static IEnumerable<Vector2i> MakeCrossDiagonal(Vector2i center, int range)
    {
        yield return center;

        if (range <= 0)
            yield break;

        for (var i = 1; i < range; i++)
        {
            yield return new Vector2i(center.X + i, center.Y + i);
            yield return new Vector2i(center.X + i, center.Y - i);
            yield return new Vector2i(center.X - i, center.Y + i);
            yield return new Vector2i(center.X - i, center.Y - i);
        }
    }

    /// <summary>
    /// Makes a box where each square is filled by a random chance.
    /// </summary>
    public static IEnumerable<Vector2i> MakeBoxChanceRandom(Vector2i center, int range, System.Random random, float filledSquareChance = 0.3f)
    {
        var refs = MakeBoxFilled(center, range).ToList();
        var refsTemp = new List<Vector2i>(refs);
        foreach (var tile in refsTemp)
        {
            if (!random.Prob(filledSquareChance))
                refs.Remove(tile);
        }

        return refs;
    }

    /// <summary>
    /// Makes a box and then removes the specified amount of tiles from it randomly.
    /// </summary>
    public static IEnumerable<Vector2i> MakeBoxCountRandom(Vector2i center, int range, System.Random random, int removeAmount)
    {
        var refs = MakeBoxFilled(center, range).ToList();
        for (int i = 0; i < removeAmount; i++)
        {
            if (refs.Count == 0)
                return refs;

            refs.Remove(random.Pick(refs));
        }

        return refs;
    }
}
