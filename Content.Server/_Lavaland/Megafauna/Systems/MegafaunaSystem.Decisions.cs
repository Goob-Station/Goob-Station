// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Random.Helpers;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem
{
    /// <summary>
    /// Picks an attack from the list at random, accounting for their weights.
    /// </summary>
    private bool PickRandomMegafaunaAttack(
        List<MegafaunaAction> actionsData,
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

    /// <summary>
    /// Picks megafauna attack for the megafauna AI, running conditions for each attack
    /// </summary>
    private bool TryPickMegafaunaAttack(MegafaunaThinkBaseArgs args, [NotNullWhen(true)] out MegafaunaAction? action)
    {
        action = null;
        var comp = args.AiComponent;

        // Stores amount of condition fails for each attack
        var conditionChecks = new List<(MegafaunaAction Action, int FailAmout)>();

        // Some attacks are using same conditions, so to prevent running same code twice we hash the results
        // TODO check if this dictionary actually works for all conditions
        var conditionHash = new Dictionary<MegafaunaCondition, bool>();

        // behold, THE NEST of LOOPS for OPTIMIZATION glory!!!!
        foreach (var data in comp.ActionsData)
        {
            var failCount = 0;
            foreach (var condition in data.Conditions)
            {
                if (conditionHash.TryGetValue(condition, out var check)
                    && !check)
                    failCount++;
                else
                {
                    var condPassed = condition.Check(args);
                    if (!condPassed)
                        failCount++;

                    conditionHash.Add(condition, condPassed);
                }
            }

            conditionChecks.Add((data.Action, failCount));
        }

        // Simple math to get a list containing only more appropriate actions
        var leastFails = int.MaxValue;
        foreach (var (_, amount) in conditionChecks)
        {
            if (leastFails > amount)
                leastFails = amount;
        }

        // Add only best actions
        var passed = new List<MegafaunaAction>();
        foreach (var (pass, amount) in conditionChecks)
        {
            if (amount == leastFails)
                passed.Add(pass);
        }

        return PickRandomMegafaunaAttack(passed, comp.PreviousAttack, out action);
    }
}
