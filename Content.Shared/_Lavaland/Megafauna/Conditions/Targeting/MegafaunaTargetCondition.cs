using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Megafauna.Conditions;

/// <summary>
/// Universal parent for all megafauna conditions that
/// check something on a <see cref="Target"/> entity.
/// </summary>
[MeansImplicitUse]
public abstract partial class MegafaunaTargetCondition : MegafaunaCondition
{
    [ViewVariables]
    public EntityUid Target;
}
