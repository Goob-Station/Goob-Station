// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Lavaland.MobPhases;

namespace Content.Shared._Lavaland.Megafauna.Conditions;

/// <summary>
/// Condition that returns true if the boss is currently at specific phase.
/// Returns false if doesn't have <see cref="MobPhasesComponent"/> or phase doesn't equal to any of RequiredPhases.
/// </summary>
public sealed partial class PhaseMegafaunaCondition : MegafaunaCondition
{
    [DataField("phases", required: true)]
    public int[] RequiredPhases;

    public override bool EvaluateImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        if (!entMan.TryGetComponent(args.Entity, out MobPhasesComponent? phasesComp))
            return false;

        return RequiredPhases.Contains(phasesComp.CurrentPhase);
    }
}
