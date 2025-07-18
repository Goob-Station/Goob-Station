using System.Numerics;
using Robust.Shared.Random;

namespace Content.Shared._Lavaland.Megafauna.NumberSelectors;

/// <summary>
/// Gives a value between the two numbers specified, inclusive.
/// </summary>
public sealed partial class MegafaunaRangeNumberSelector : MegafaunaNumberSelector
{
    [DataField]
    public Vector2 Range = new(1f, 1f);

    public MegafaunaRangeNumberSelector(Vector2 range)
    {
        Range = range;
    }

    public override float Get(MegafaunaCalculationBaseArgs args)
    {
        // rand.Next() is inclusive on the first number and exclusive on the second number,
        // so we add 1 to the second number.
        return args.Random.NextFloat(Range.X, Range.Y + 1);
    }
}
