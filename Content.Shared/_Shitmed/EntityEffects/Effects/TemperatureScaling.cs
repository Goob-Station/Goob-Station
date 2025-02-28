using Robust.Shared.Serialization;
using Robust.Shared.GameObjects;
using Content.Shared.FixedPoint;

namespace Content.Shared._Shitmed.EntityEffects.Effects;

/// <summary>
/// Scales the efficiency of an effect based on the temperature of the entity.
/// <param name="Min">The minimum temperature to scale the effect.</param>
/// <param name="Max">The maximum temperature to scale the effect.</param>
/// <param name="Scale">The scale to use for the efficiency.</param>
/// </summary>
[DataRecord, Serializable]
public record struct TemperatureScaling(FixedPoint2 Min, FixedPoint2 Max, FixedPoint2 Scale)
{
    public static implicit operator (FixedPoint2, FixedPoint2, FixedPoint2)(TemperatureScaling p) => (p.Min, p.Max, p.Scale);
    public static implicit operator TemperatureScaling((FixedPoint2, FixedPoint2, FixedPoint2) p) => new(p.Item1, p.Item2, p.Item3);

    public FixedPoint2 GetEfficiencyMultiplier(FixedPoint2 temperature, FixedPoint2 scale, bool invert = false)
    {
        if (Min == Max)
            return FixedPoint2.New(1); // If the min is equal to the max, return one or full efficiency since the range is meaningless.

        // Clamp the temperature within a given range.
        temperature = FixedPoint2.Clamp(temperature, Min, Max);

        // Calculate the distance from the minimum.
        var distance = FixedPoint2.Abs(temperature - Min);
        // Calculate the full possible temperature range between min and max.
        var totalRange = Max - Min;

        // The scaled distance should be between 0 and 1. If the total range is zero, avoid dividing by zero by setting the scaled distance to zero.
        // Otherwise, we calculate scaled distance equals distance divided by total range.
        var scaledDistance = totalRange == FixedPoint2.Zero ? FixedPoint2.Zero : distance / totalRange;

        // Something something ternary
        return invert
            ? FixedPoint2.New(1) + (scaledDistance * scale)
            : (FixedPoint2.New(1) - scaledDistance) * scale;
    }
}
