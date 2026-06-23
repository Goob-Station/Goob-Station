using System.Numerics;

namespace Content.Goobstation.Shared.HeatTint;

public abstract class SharedHeatTintSystem : EntitySystem
{
    public static Color GetHeatColor(List<HeatTintPoint> gradient, float temperature)
    {
        if (gradient.Count == 0)
            return Color.White;

        if (gradient.Count == 1)
            return gradient[0].Color;

        if (temperature <= gradient[0].Temperature)
            return gradient[0].Color;

        if (temperature >= gradient[^1].Temperature)
            return gradient[^1].Color;

        for (var i = 0; i < gradient.Count - 1; i++)
        {
            var lower = gradient[i];
            var upper = gradient[i + 1];

            if (temperature < lower.Temperature || temperature > upper.Temperature)
                continue;

            var range = upper.Temperature - lower.Temperature;
            var t = range > 0 ? (temperature - lower.Temperature) / range : 0f;

            var lab1 = Color.ToLab(lower.Color);
            var lab2 = Color.ToLab(upper.Color);
            return Color.FromLab(Vector4.Lerp(lab1, lab2, t));
        }

        return gradient[^1].Color;
    }
}
