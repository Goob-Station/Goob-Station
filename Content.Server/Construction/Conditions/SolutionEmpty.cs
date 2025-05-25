// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Construction;
using Content.Shared.Examine;

namespace Content.Server.Construction.Conditions;

/// <summary>
/// Requires that a certain solution be empty to proceed.
/// </summary>
[DataDefinition]
public sealed partial class SolutionEmpty : IGraphCondition
{
    /// <summary>
    /// The solution that needs to be empty.
    /// </summary>
    [DataField]
    public string Solution;

    public bool Condition(EntityUid uid, IEntityManager entMan)
    {
        var containerSys = entMan.System<SharedSolutionContainerSystem>();
        if (!containerSys.TryGetSolution(uid, Solution, out _, out var solution))
            return false;

        return solution.Volume == 0;
    }

    public bool DoExamine(ExaminedEvent args)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var uid = args.Examined;

        var containerSys = entMan.System<SharedSolutionContainerSystem>();
        if (!containerSys.TryGetSolution(uid, Solution, out _, out var solution))
            return false;

        // already empty so dont show examine
        if (solution.Volume == 0)
            return false;

        args.PushMarkup(Loc.GetString("construction-examine-condition-solution-empty"));
        return true;
    }

    public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
    {
        yield return new ConstructionGuideEntry()
        {
            Localization = "construction-guide-condition-solution-empty"
        };
    }
}