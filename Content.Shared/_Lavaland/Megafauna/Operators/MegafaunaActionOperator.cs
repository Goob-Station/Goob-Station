using Content.Shared._Lavaland.Megafauna.Actions;
using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Megafauna.Operators;

/// <summary>
/// Used for attacks that change with time, via <see cref="SequenceMegafaunaAction"/>.
/// Calculates some new value based on current Counter integer.
/// </summary>
/// <remarks>
/// Im losing my sanity at this point tbh.
/// I just want this to work.
/// TODO rework this thing
/// </remarks>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class MegafaunaActionOperator
{
    public abstract object GetValue(int counter);
}
