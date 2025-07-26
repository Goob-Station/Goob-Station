using System.Numerics;
using Content.Shared._Lavaland.Anger;

namespace Content.Shared._Lavaland.Megafauna.NumberSelectors;

/// <summary>
/// Scales the number based on current anger percentage.
/// </summary>
public sealed partial class AngerNumberSelector : MegafaunaNumberSelector
{
    [DataField]
    public Vector2 Range = new(1f, 1f);

    /// <summary>
    /// If true, will inverse the calculation so the value will
    /// become smaller with bigger aggression.
    /// </summary>
    [DataField]
    public bool Inverse;

    [DataField]
    public NumberGrowthFormula ScaleFormula = NumberGrowthFormula.Linear;

    public override float Get(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;

        if (!entMan.TryGetComponent<AngerComponent>(uid, out var angerComp))
            return Range.X; // Minimal possible value as if anger was 0

        var maxAnger = angerComp.MaxAnger;
        var anger = angerComp.CurrentAnger;
        switch (ScaleFormula)
        {
            case NumberGrowthFormula.Linear:
                var progress = anger / maxAnger;
                return Inverse
                    ? Range.Y + (Range.X - Range.Y) * (1f - progress)
                    : Range.X + (Range.Y - Range.X) * progress;
            case NumberGrowthFormula.Exponent:
                return Range.X;
            case NumberGrowthFormula.Sqrt:
                return Range.X;
            default:
                return Range.X;
        }
    }
}

public enum NumberGrowthFormula
{
    Linear,
    Exponent, // TODO
    Sqrt, // TODO
}
