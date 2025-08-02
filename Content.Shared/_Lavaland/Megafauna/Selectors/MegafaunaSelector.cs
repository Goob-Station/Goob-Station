// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Megafauna.Conditions;
using Content.Shared._Lavaland.Megafauna.NumberSelectors;
using JetBrains.Annotations;
using Robust.Shared.Random;

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

    /// <summary>
    /// A simple chance that the selector will run.
    /// </summary>
    [DataField]
    public double Prob = 1;

    [DataField]
    public int Priority;

    /// <summary>
    /// Represents this attack's order from other attacks that are called via
    /// <see cref="SequenceMegafauna"/>. Use this variable to make time-progressive actions.
    /// </summary>
    [DataField]
    public int? Counter;

    /// <summary>
    /// If true, allows some actions to use the Counter variable in order to create sequence attacks
    /// instead of just repeating the same attack multiple times.
    /// Using this you can create more complex actions that are called with <see cref="SequenceMegafauna"/>.
    /// </summary>
    [DataField]
    public bool? IsSequence;

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
    public MegafaunaNumberSelector DelaySelector = new MegafaunaConstantNumberSelector(1f);

    /// <summary>
    /// Default delay time after failing random or conditions check.
    /// </summary>
    [DataField]
    public float FailDelay = 0.5f;

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
        if (!CheckConditions(args)
            || !args.Random.Prob(Prob))
            return FailDelay;

        return InvokeImplementation(args);
    }

    public void CopyFrom(MegafaunaSelector parent)
    {
        if (parent.Counter != null)
            Counter = parent.Counter;
        if (parent.IsSequence != null)
            IsSequence = parent.IsSequence;
    }

    protected abstract float InvokeImplementation(MegafaunaCalculationBaseArgs args);
}
