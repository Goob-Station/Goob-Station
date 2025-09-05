using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Dataset;
using Content.Shared.Random;
using Robust.Shared.Collections;
using Robust.Shared.Maths;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Random;

public interface IGoobRandom
{
    // From RobustRandom
    System.Random GetRandom();
    void SetSeed(int seed);
    float NextFloat();
    float NextFloat(float minValue, float maxValue);
    float NextFloat(float maxValue);
    int Next();
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    byte NextByte();
    byte NextByte(byte maxValue);
    byte NextByte(byte minValue, byte maxValue);
    double NextDouble();
    double Next(double maxValue);
    double NextDouble(double minValue, double maxValue);
    TimeSpan Next(TimeSpan maxTime);
    TimeSpan Next(TimeSpan minTime, TimeSpan maxTime);
    void NextBytes(byte[] buffer);
    Angle NextAngle();
    Angle NextAngle(Angle maxValue);
    Angle NextAngle(Angle minValue, Angle maxValue);
    Vector2 NextVector2(float maxMagnitude = 1);
    Vector2 NextVector2(float minMagnitude, float maxMagnitude);
    Vector2 NextVector2Box(float minX, float minY, float maxX, float maxY);
    Vector2 NextVector2Box(float maxAbsX = 1, float maxAbsY = 1);
    void Shuffle<T>(IList<T> list);
    void Shuffle<T>(Span<T> list);
    void Shuffle<T>(ValueList<T> list);

    // From RandomExtensions
    double NextGaussian(double μ = 0, double σ = 1);
    T Pick<T>(IReadOnlyList<T> list);
    ref T Pick<T>(ValueList<T> list);
    T Pick<T>(IReadOnlyCollection<T> collection);
    T PickAndTake<T>(IList<T> list);
    bool Prob(float chance);
    T[] GetItems<T>(IList<T> source, int count, bool allowDuplicates = true);
    T Pick<T>(IReadOnlyDictionary<T, float> collection);
    string Pick(DatasetPrototype prototype);
    string Pick(LocalizedDatasetPrototype prototype);
    T PickAndTake<T>(Dictionary<T, float> weights) where T: notnull;
    bool TryPickAndTake<T>(Dictionary<T, float> weights, [NotNullWhen(true)] out T? pick) where T : notnull;
    (string reagent, FixedPoint2 quantity) Pick(WeightedRandomFillSolutionPrototype prototype);
    RandomFillSolution PickRandomFill(WeightedRandomFillSolutionPrototype prototype);
    string Pick(IWeightedRandomPrototype prototype);
}
