// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Megafauna.Conditions;

/// <summary>
/// Represents a condition that is checked before making some specific MegafaunaAction.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class MegafaunaCondition
{
    /// <summary>
    /// If true, inverts the result of the condition.
    /// </summary>
    [DataField]
    public bool Invert;

    public bool Evaluate(MegafaunaCalculationBaseArgs args)
    {
        var res = EvaluateImplementation(args);

        // XOR eval to invert the result.
        return res ^ Invert;
    }

    public abstract bool EvaluateImplementation(MegafaunaCalculationBaseArgs args);
}
