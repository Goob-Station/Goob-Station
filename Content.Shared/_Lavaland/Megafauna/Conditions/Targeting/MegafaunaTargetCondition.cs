using Content.Shared._Lavaland.Megafauna.Selectors;
using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Megafauna.Conditions.Targeting;

/// <summary>
/// Universal parent for all megafauna conditions that
/// check something on a target entity.
///
/// Used in selectors like <see cref="AggressivePickTargetSelector"/> to check
/// all possible variants and return just one best target out of all possibilities.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class MegafaunaTargetCondition
{
    /// <summary>
    /// If true, inverts the result of the condition.
    /// </summary>
    [DataField]
    public bool Invert;

    public bool Evaluate(MegafaunaCalculationBaseArgs args, EntityUid target)
    {
        var res = EvaluateImplementation(args, target);

        // XOR eval to invert the result.
        return res ^ Invert;
    }

    public abstract bool EvaluateImplementation(MegafaunaCalculationBaseArgs args, EntityUid target);
}
