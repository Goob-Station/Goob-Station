// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._Lavaland.Megafauna.Components;

namespace Content.Server._Lavaland.Megafauna.Conditions;

/// <summary>
/// Condition that returns true if the boss is currently at specific phase.
/// Returns false if doesn't have <see cref="PhasesMegafaunaAiComponent"/> or phase doesn't equal to any of RequiredPhases.
/// </summary>
public abstract partial class PhasesMegafaunaCondition : MegafaunaCondition
{
    [DataField(required: true)]
    public int[] RequiredPhases;

    public override bool Check(MegafaunaThinkBaseArgs args)
    {
        var entMan = args.EntityManager;
        if (!entMan.TryGetComponent(args.BossEntity, out PhasesMegafaunaAiComponent? phasesComp))
            return false;

        return RequiredPhases.Contains(phasesComp.CurrentPhase);
    }
}
