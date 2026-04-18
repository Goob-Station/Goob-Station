// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.EntityConditions;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects.EffectConditions;

public sealed partial class UniqueBloodstreamChemThresholdSystem : EntityConditionSystem<BloodstreamComponent, UniqueBloodstreamChemThreshold>
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    protected override void Condition(Entity<BloodstreamComponent> entity, ref EntityConditionEvent<UniqueBloodstreamChemThreshold> args)
    {
        if (_solution.ResolveSolution(entity.Owner, entity.Comp.ChemicalSolutionName, ref entity.Comp.ChemicalSolution, out var chemSolution))
        {
            args.Result = chemSolution.Contents.Count > args.Condition.Min &&
                          chemSolution.Contents.Count < args.Condition.Max;
            return;
        }
        args.Result = false;
    }
}

public sealed partial class UniqueBloodstreamChemThreshold : EntityConditionBase<UniqueBloodstreamChemThreshold>
{
    [DataField]
    public int Max = int.MaxValue;

    [DataField]
    public int Min = -1;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-unique-bloodstream-chem-threshold",
            ("max", Max),
            ("min", Min));
    }
}
