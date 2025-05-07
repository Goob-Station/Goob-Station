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
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Construction;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.Construction.Conditions;

/// <summary>
/// Requires that a certain solution has a minimum amount of a reagent to proceed.
/// </summary>
[DataDefinition]
public sealed partial class MinSolution : IGraphCondition
{
    /// <summary>
    /// The solution that needs to have the reagent.
    /// </summary>
    [DataField(required: true)]
    public string Solution = string.Empty;

    /// <summary>
    /// The reagent that needs to be present.
    /// </summary>
    [DataField(required: true)]
    public ReagentId Reagent = new();

    /// <summary>
    /// How much of the reagent must be present.
    /// </summary>
    [DataField]
    public FixedPoint2 Quantity = 1;

    public bool Condition(EntityUid uid, IEntityManager entMan)
    {
        var containerSys = entMan.System<SharedSolutionContainerSystem>();
        if (!containerSys.TryGetSolution(uid, Solution, out _, out var solution))
            return false;

        solution.TryGetReagentQuantity(Reagent, out var quantity);
        return quantity >= Quantity;
    }

    public bool DoExamine(ExaminedEvent args)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var uid = args.Examined;

        var containerSys = entMan.System<SharedSolutionContainerSystem>();
        if (!containerSys.TryGetSolution(uid, Solution, out _, out var solution))
            return false;

        solution.TryGetReagentQuantity(Reagent, out var quantity);

        // already has enough so dont show examine
        if (quantity >= Quantity)
            return false;

        args.PushMarkup(Loc.GetString("construction-examine-condition-min-solution",
            ("quantity", Quantity - quantity), ("reagent", Name())) + "\n");
        return true;
    }

    public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
    {
        yield return new ConstructionGuideEntry()
        {
            Localization = "construction-guide-condition-min-solution",
            Arguments = new (string, object)[]
            {
                ("quantity", Quantity),
                ("reagent", Name())
            }
        };
    }

    private string Name()
    {
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var proto = protoMan.Index<ReagentPrototype>(Reagent.Prototype);
        return proto.LocalizedName;
    }
}