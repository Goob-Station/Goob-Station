namespace Content.Shared._Lavaland.Megafauna.NumberSelectors;

/// <summary>
/// Gives a constant value.
/// </summary>
public sealed partial class MegafaunaConstantNumberSelector : MegafaunaNumberSelector
{
    [DataField]
    public float Value = 1f;

    public MegafaunaConstantNumberSelector(float value)
    {
        Value = value;
    }

    public override float Get(MegafaunaCalculationBaseArgs args)
    {
        return Value;
    }
}
