// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._Lavaland.Megafauna.Components;
using Content.Shared.Random.Helpers;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem
{
    private bool TryPickMegafaunaAttack(
        IReadOnlyList<MegafaunaAction> actionsData,
        string? previousAttack,
        [NotNullWhen(true)] out MegafaunaAction? attack)
    {
        attack = null;
        if (actionsData.Count == 0)
            return false;

        var resolvedWeights = actionsData.ToDictionary(action => action, action => action.Weight);
        attack = SharedRandomExtensions.Pick(resolvedWeights, _random.GetRandom());

        if (actionsData.Count > 1 && attack.Name == previousAttack)
        {
            // Pick once again if this attack was already picked before
            resolvedWeights.Remove(attack);
            attack = SharedRandomExtensions.Pick(resolvedWeights, _random.GetRandom());
        }

        return true;
    }

    private bool TryPickAggressionAttack(Entity<MegafaunaAiComponent, AggressiveMegafaunaAiComponent> ent, [NotNullWhen(true)] out MegafaunaAction? action)
    {
        return TryPickMegafaunaAttack(ent.Comp2.ActionsData, ent.Comp1.PreviousAttack, out action);
    }

    private bool TryPickPhasesAttack(Entity<MegafaunaAiComponent, PhasesMegafaunaAiComponent> ent, [NotNullWhen(true)] out MegafaunaAction? action)
    {
        action = null;
        if (!ent.Comp2.PhasedActionsData.TryGetValue(ent.Comp2.CurrentPhase, out var actionData))
            return false;

        return TryPickMegafaunaAttack(actionData, ent.Comp1.PreviousAttack, out action);
    }

    private bool TryPickMegafaunaAttack(Entity<MegafaunaAiComponent> ent, [NotNullWhen(true)] out MegafaunaAction? action)
    {
        action = null;

        // While in decision-making, Phases > Aggressive
        if (_phasesQuery.TryComp(ent.Owner, out var phasesAiComp)
            && TryPickPhasesAttack((ent.Owner, ent.Comp, phasesAiComp), out action))
            return true;

        if (_agressiveQuery.TryComp(ent.Owner, out var aggressiveAiComp)
            && TryPickAggressionAttack((ent.Owner, ent.Comp, aggressiveAiComp), out action))
            return true;

        return false;
    }
}
