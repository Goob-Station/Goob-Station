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

    public FixedPoint2 GetEfficiencyMultiplier(FixedPoint2 temperature)
    {
        if (temperature == Min)
            return 1;

        if (temperature > Max)
            return FixedPoint2.Zero;

        var distance = FixedPoint2.Abs(temperature - Min);
        var totalRange = Max - Min;
        var scaledDistance = distance / totalRange;
        var efficiency = 1 - scaledDistance;
        return efficiency < 0 ? FixedPoint2.Zero : efficiency;
    }
}
