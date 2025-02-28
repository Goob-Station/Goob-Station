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
            return FixedPoint2.New(1); // Prevent division by zero if range is invalid

        // Clamp temperature within range
        temperature = FixedPoint2.Clamp(temperature, Min, Max);

        var distance = invert ? FixedPoint2.Abs(temperature - Max) : FixedPoint2.Abs(temperature - Min);
        var totalRange = Max - Min;

        // Handle potential division by zero
        var scaledDistance = totalRange == FixedPoint2.Zero ? FixedPoint2.Zero : distance / totalRange;

        // Apply scale and allow values > 1
        var efficiency = (FixedPoint2.New(1) - scaledDistance) * scale;

        return efficiency;
    }
}
