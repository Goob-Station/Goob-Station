using Content.Shared._Lavaland.Megafauna.Selectors;
using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Megafauna.Conditions;

/// <summary>
/// Universal parent for all megafauna conditions that
/// check something on a <see cref="Target"/> entity.
///
/// Used in selectors like <see cref="AggressivePickTargetSelector"/> to check
/// all possible variants and return just one best target out of all possibilities.
/// </summary>
[MeansImplicitUse]
public abstract partial class MegafaunaTargetCondition : MegafaunaCondition
{
    [ViewVariables]
    public EntityUid Target;
}
