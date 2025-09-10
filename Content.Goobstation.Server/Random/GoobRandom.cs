using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Dataset;
using Content.Shared.Random;
using Robust.Shared.Collections;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Random;

/// <summary>
/// A wrapper for System.Random that provides additional functionality (all the funcs from robust random and its extensions) and integrates with the ApiRandomManager.
/// Numbers are theoretically "more" random as they are fetched from a randomness api, read @ https://nousrandom.net/index.html for more details.
/// Why should you use this? because why not lol.
/// </summary>
public sealed class GoobRandom : IGoobRandom
{
    [Dependency] private readonly ApiRandomManager _apiRandom = default!;

    private System.Random _random = new();

    public System.Random GetRandom() => _random;

    public void SetSeed(int seed)
    {
        _random = new(seed);
    }

    public float NextFloat()
    {
        if (_apiRandom.TryGetFloat(out var value))
            return value;

        return Next() * 4.6566128752458E-10f;
    }

    public float NextFloat(float minValue, float maxValue)
    {
        return NextFloat() * (maxValue - minValue) + minValue;
    }

    public float NextFloat(float maxValue)
    {
        return NextFloat() * maxValue;
    }

    public int Next()
    {
        if (_apiRandom.TryGetInt(out var value))
            return value;

        return _random.Next();
    }

    public int Next(int maxValue)
    {
        return Next(0, maxValue);
    }

    public int Next(int minValue, int maxValue)
    {
        if (_apiRandom.TryGetInt(out var apiValue))
        {
            var scaled = (double) apiValue / int.MaxValue;
            return (int) (minValue + scaled * (maxValue - minValue));
        }

        return _random.Next(minValue, maxValue);
    }

    public byte NextByte()
    {
        return (byte) Next(0, 256);
    }

    public byte NextByte(byte maxValue)
    {
        return (byte) Next(0, maxValue);
    }

    public byte NextByte(byte minValue, byte maxValue)
    {
        return (byte) Next(minValue, maxValue);
    }

    public double NextDouble()
    {
        if (_apiRandom.TryGetFloat(out var value))
            return value;

        return _random.NextDouble();
    }

    public double Next(double maxValue)
    {
        return NextDouble() * maxValue;
    }

    public double NextDouble(double minValue, double maxValue)
    {
        return NextDouble() * (maxValue - minValue) + minValue;
    }

    public TimeSpan Next(TimeSpan maxTime)
    {
        return Next(TimeSpan.Zero, maxTime);
    }

    public TimeSpan Next(TimeSpan minTime, TimeSpan maxTime)
    {
        DebugTools.Assert(minTime < maxTime);
        return minTime + (maxTime - minTime) * NextDouble();
    }

    public void NextBytes(byte[] buffer)
    {
        _random.NextBytes(buffer);
    }

    public Angle NextAngle()
    {
        return NextFloat() * MathF.Tau;
    }

    public Angle NextAngle(Angle maxValue)
    {
        return NextFloat() * maxValue;
    }

    public Angle NextAngle(Angle minValue, Angle maxValue)
    {
        return NextFloat() * (maxValue - minValue) + minValue;
    }

    public Vector2 NextVector2(float maxMagnitude = 1)
    {
        return NextVector2(0, maxMagnitude);
    }

    public Vector2 NextVector2(float minMagnitude, float maxMagnitude)
    {
        return NextAngle().RotateVec(new Vector2(NextFloat(minMagnitude, maxMagnitude), 0));
    }

    public Vector2 NextVector2Box(float minX, float minY, float maxX, float maxY)
    {
        return new Vector2(NextFloat(minX, maxX), NextFloat(minY, maxY));
    }

    public Vector2 NextVector2Box(float maxAbsX = 1, float maxAbsY = 1)
    {
        return NextVector2Box(-maxAbsX, -maxAbsY, maxAbsX, maxAbsY);
    }

    public void Shuffle<T>(IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n -= 1;
            var k = Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public void Shuffle<T>(Span<T> list)
    {
        var n = list.Length;
        while (n > 1)
        {
            n -= 1;
            var k = Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public void Shuffle<T>(ValueList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n -= 1;
            var k = Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public double NextGaussian(double μ = 0, double σ = 1)
    {
        return _random.NextGaussian(μ, σ);
    }

    public T Pick<T>(IReadOnlyList<T> list)
    {
        var index = Next(list.Count);
        return list[index];
    }

    public ref T Pick<T>(ValueList<T> list)
    {
        var index = Next(list.Count);
        return ref list[index];
    }

    public T Pick<T>(IReadOnlyCollection<T> collection)
    {
        var index = Next(collection.Count);
        var i = 0;
        foreach (var t in collection)
        {
            if (i++ == index)
            {
                return t;
            }
        }

        throw new UnreachableException("This should be unreachable!");
    }

    public T PickAndTake<T>(IList<T> list)
    {
        var index = Next(list.Count);
        var element = list[index];
        list.RemoveAt(index);
        return element;
    }

    public bool Prob(float chance)
    {
        DebugTools.Assert(chance <= 1 && chance >= 0, $"Chance must be in the range 0-1. It was {chance}.");
        return NextDouble() < chance;
    }

    public T[] GetItems<T>(IList<T> source, int count, bool allowDuplicates = true)
    {
        if (source.Count == 0 || count <= 0)
            return Array.Empty<T>();

        if (allowDuplicates == false && count >= source.Count)
        {
            var arr = source.ToArray();
            Shuffle(arr);
            return arr;
        }

        var sourceCount = source.Count;
        var result = new T[count];

        if (allowDuplicates)
        {
            for (var i = 0; i < count; i++)
            {
                result[i] = source[Next(sourceCount)];
            }

            return result;
        }

        var indices = sourceCount <= 1024 ? stackalloc int[sourceCount] : new int[sourceCount];
        for (var i = 0; i < sourceCount; i++)
        {
            indices[i] = i;
        }

        for (var i = 0; i < count; i++)
        {
            var j = Next(sourceCount - i);
            result[i] = source[indices[j]];
            indices[j] = indices[sourceCount - i - 1];
        }

        return result;
    }

    public T Pick<T>(IReadOnlyDictionary<T, float> collection)
    {
        var totalWeight = 0f;
        foreach (var weight in collection.Values)
        {
            totalWeight += weight;
        }

        var randomValue = NextFloat() * totalWeight;
        foreach (var (item, weight) in collection)
        {
            randomValue -= weight;
            if (randomValue <= 0)
            {
                return item;
            }
        }

        return collection.Keys.First();
    }

    public void Shuffle<T>(T[] array)
    {
        for (var i = array.Length - 1; i > 0; i--)
        {
            var j = Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    public string Pick(DatasetPrototype prototype)
    {
        return Pick(prototype.Values);
    }

    public string Pick(LocalizedDatasetPrototype prototype)
    {
        var index = Next(prototype.Values.Count);
        return Loc.GetString(prototype.Values[index]);
    }

    public T PickAndTake<T>(Dictionary<T, float> weights) where T : notnull
    {
        var pick = Pick(weights);
        weights.Remove(pick);
        return pick;
    }

    public bool TryPickAndTake<T>(Dictionary<T, float> weights, [NotNullWhen(true)] out T? pick) where T : notnull
    {
        if (weights.Count == 0)
        {
            pick = default;
            return false;
        }
        pick = PickAndTake(weights);
        return true;
    }

    public (string reagent, FixedPoint2 quantity) Pick(WeightedRandomFillSolutionPrototype prototype)
    {
        var randomFill = PickRandomFill(prototype);
        var sum = randomFill.Reagents.Count;
        var accumulated = 0f;
        var rand = NextFloat() * sum;

        foreach (var reagent in randomFill.Reagents)
        {
            accumulated += 1f;

            if (accumulated >= rand)
            {
                return (reagent, randomFill.Quantity);
            }
        }

        throw new InvalidOperationException($"Invalid weighted pick for {prototype.ID}!");
    }

    public RandomFillSolution PickRandomFill(WeightedRandomFillSolutionPrototype prototype)
    {
        var fills = prototype.Fills;
        Dictionary<RandomFillSolution, float> picks = new();

        foreach (var fill in fills)
        {
            picks[fill] = fill.Weight;
        }

        var sum = picks.Values.Sum();
        var accumulated = 0f;
        var rand = NextFloat() * sum;

        foreach (var (randSolution, weight) in picks)
        {
            accumulated += weight;

            if (accumulated >= rand)
            {
                return randSolution;
            }
        }

        throw new InvalidOperationException($"Invalid weighted pick for {prototype.ID}!");
    }

    public string Pick(IWeightedRandomPrototype prototype)
    {
        var picks = prototype.Weights;
        var sum = picks.Values.Sum();
        var accumulated = 0f;

        var rand = NextFloat() * sum;

        foreach (var (key, weight) in picks)
        {
            accumulated += weight;

            if (accumulated >= rand)
            {
                return key;
            }
        }

        throw new InvalidOperationException($"Invalid weighted pick for {prototype.ID}!");
    }
}
