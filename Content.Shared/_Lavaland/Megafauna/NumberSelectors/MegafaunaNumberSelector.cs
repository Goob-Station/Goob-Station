using Content.Shared._Lavaland.Megafauna.Actions;
using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Megafauna.NumberSelectors;

/// <summary>
/// Used for implementing custom value selection for <see cref="MegafaunaActionSelector"/>.
/// Yeah, I didn't want to mess with Wizcode, so it's just NumberSelector but using float instead of integer.
/// </summary>
[ImplicitDataDefinitionForInheritors, UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract partial class MegafaunaNumberSelector
{
    public int GetRounded(MegafaunaCalculationBaseArgs args) // Hello, Im Rounded
    {
        return (int) Math.Round(Get(args));
    }

    public abstract float Get(MegafaunaCalculationBaseArgs args);
}
