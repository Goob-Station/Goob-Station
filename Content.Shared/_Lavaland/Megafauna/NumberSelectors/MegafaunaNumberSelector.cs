using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Megafauna.NumberSelectors;

/// <summary>
/// Used for implementing custom value selection for <see cref="MegafaunaSelector"/>.
/// Yeah, I didn't want to mess with Wizcode, so it's just NumberSelector but using float instead of integer.
/// </summary>
[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class MegafaunaNumberSelector
{
    /// <summary>
    /// Input that is used in some number selectors to modify it and return a result.
    /// </summary>
    [DataField]
    public float Value;

    [DataField]
    public MidpointRounding Rounding = MidpointRounding.ToEven;

    public int GetRounded(MegafaunaCalculationBaseArgs args) // Hello, Im Rounded
    {
        return (int) Math.Round(Get(args), Rounding);
    }

    public abstract float Get(MegafaunaCalculationBaseArgs args);
}
