// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Robust.Shared.Random;
// ReSharper disable PossibleLossOfFraction

namespace Content.Shared._Lavaland.EntityShapes;

/// <summary>
/// Some static helper methods that help to create some tile patterns with ease.
/// Allows to reuse already written methods for generating shapes, so making new
/// EntityShape classes becomes much easier.
/// </summary>
public static class ShapeHelpers
{
    /// <summary>
    /// Draws a simple line in a specified direction, adding Step vector Range
    /// times, starting from the center and returning the result.
    /// </summary>
    public static IEnumerable<Vector2> MakeLine(Vector2 center, int range, Vector2 step)
    {
        yield return center;

        if (step == Vector2.Zero)
            yield break;

        var curStep = new Vector2(center.X, center.Y);
        for (int i = 0; i < range; i++)
        {
            curStep += step;
            yield return curStep;
        }
    }

    public static IEnumerable<Vector2> MakeBox(Vector2 center, int range, bool hollow, float stepSize = 1)
    {
        return hollow ? MakeBoxHollow(center, range, stepSize) : MakeBoxFilled(center, range, stepSize);
    }


    public static IEnumerable<Vector2> MakeBoxFilled(Vector2 center, int range, float stepSize = 1)
    {
        if (range <= 0)
            yield break;

        if (stepSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(stepSize), "stepSize must be greater than zero.");

        // If range == 1, just return the center
        if (range == 1)
        {
            yield return center;
            yield break;
        }

        // Use float arithmetic to get a true centered start.
        // (range - 1) / 2f centers the integer grid around the center.
        var half = (range - 1) / 2f;
        var startPoint = center - new Vector2(half, half);

        for (var y = 0f; y < range; y += stepSize)
        {
            for (var x = 0f; x < range; x += stepSize)
            {
                yield return startPoint + new Vector2(x, y);
            }
        }
    }

    public static IEnumerable<Vector2> MakeBoxHollow(Vector2 center, int range, float stepSize = 1)
    {
        if (range <= 0)
            yield break;

        var bottomLeft = center - new Vector2(range, range);
        var topLeft = center - new Vector2(range, -range);
        var topRight = center - new Vector2(-range, -range);
        var bottomRight = center - new Vector2(-range, range);

        var side = range * 2;

        // Left side
        for (var i = 0f; i < side; i += stepSize)
        {
            yield return bottomLeft + Vector2.UnitY * i;
        }
        // Top side
        for (var i = 0f; i < side; i += stepSize)
        {
            yield return topLeft + Vector2.UnitX * i;
        }
        // Right side
        for (var i = 0f; i < side; i += stepSize)
        {
            yield return topRight + -Vector2.UnitY * i;
        }
        // Bottom side
        for (var i = 0f; i < side; i += stepSize)
        {
            yield return bottomRight + -Vector2.UnitX * i;
        }
    }

    public static IEnumerable<Vector2> MakeCross(Vector2 center, int range, float stepSize = 1)
    {
        yield return center;

        if (range <= 0)
            yield break;

        for (var i = 1f; i < range; i += stepSize)
        {
            yield return center with { X = center.X + i };
            yield return center with { Y = center.Y + i };
            yield return center with { X = center.X - i };
            yield return center with { Y = center.Y - i };
        }
    }

    public static IEnumerable<Vector2> MakeCrossDiagonal(Vector2 center, int range, float stepSize = 1)
    {
        yield return center;

        if (range <= 0)
            yield break;

        for (var i = 1f; i < range; i += stepSize)
        {
            yield return new Vector2(center.X + i, center.Y + i);
            yield return new Vector2(center.X + i, center.Y - i);
            yield return new Vector2(center.X - i, center.Y + i);
            yield return new Vector2(center.X - i, center.Y - i);
        }
    }

    /// <summary>
    /// Makes a box where each square is filled by a random chance.
    /// </summary>
    public static IEnumerable<Vector2> MakeBoxChanceRandom(
        Vector2 center,
        int range,
        System.Random random,
        float filledSquareChance = 0.3f,
        float stepSize = 1)
    {
        var refs = MakeBoxFilled(center, range, stepSize).ToList();
        var refsTemp = new List<Vector2>(refs);
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
    public static IEnumerable<Vector2> MakeBoxCountRandom(
        Vector2 center,
        int range,
        System.Random random,
        int removeAmount = 0,
        float stepSize = 1)
    {
        var refs = MakeBoxFilled(center, range, stepSize).ToList();
        for (int i = 0; i < removeAmount; i++)
        {
            if (refs.Count == 0)
                return refs;

            refs.Remove(random.Pick(refs));
        }

        return refs;
    }
}
