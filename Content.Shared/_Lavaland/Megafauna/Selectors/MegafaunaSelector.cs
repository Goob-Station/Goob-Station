// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Megafauna.Conditions;
using Content.Shared._Lavaland.Megafauna.NumberSelectors;
using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

/// <summary>
/// Seals a method to be invoked by some megafauna AI.
/// </summary>
/// <remarks>
/// If you want to make this action reusable, just make sure that at all steps
/// it doesn't require any specific components, and specify everything required
/// for the attack in DataFields.
/// </remarks>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class MegafaunaSelector
{
    /// <summary>
    /// A weight used to pick between actions.
    /// </summary>
    [DataField]
    public float Weight = 1;

    [DataField]
    public int Priority;

    /// <summary>
    /// A list of conditions that must evaluate to 'true' for the selector to apply.
    /// </summary>
    [DataField]
    public List<MegafaunaCondition> Conditions = new();

    /// <summary>
    /// If true, all the conditions must be successful in order for the selector to process.
    /// Otherwise, only one of them must be.
    /// </summary>
    [DataField]
    public bool RequireAllConditions = true;

    /// <summary>
    /// Used for calculating the delay for actions.
    /// </summary>
    [DataField("delay")]
    public MegafaunaNumberSelector DelaySelector = new MegafaunaConstantNumberSelector(0f);

    /// <summary>
    /// Default delay time after failing random or conditions check.
    /// </summary>
    [DataField]
    public float FailDelay;

    public bool CheckConditions(MegafaunaCalculationBaseArgs args)
    {
        if (Conditions.Count == 0)
            return true;

        var success = false;
        foreach (var condition in Conditions)
        {
            var res = condition.Evaluate(args);

            if (RequireAllConditions && !res)
                return false; // intentional break out of loop and function

            success |= res;
        }

        if (RequireAllConditions)
            return true;

        return success;
    }

    public float Invoke(MegafaunaCalculationBaseArgs args)
    {
        if (!CheckConditions(args))
            return FailDelay;

        return InvokeImplementation(args);
    }

    protected abstract float InvokeImplementation(MegafaunaCalculationBaseArgs args);
}
