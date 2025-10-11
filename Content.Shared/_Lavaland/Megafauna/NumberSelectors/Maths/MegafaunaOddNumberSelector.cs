namespace Content.Shared._Lavaland.Megafauna.NumberSelectors.Maths;

/// <summary>
/// Returns a corresponding to Value datafield odd number.
/// Basically just multiplies input by 2 and then extracts 1.
/// </summary>
public sealed partial class MegafaunaOddNumberSelector : MegafaunaNumberSelector
{
    public override float Get(MegafaunaCalculationBaseArgs args)
    {
        if (Math.Abs(Value) < 0.1f)
            return 0f;

        return Value * 2 - 1;
    }
}
