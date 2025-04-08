// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Andrew <blackledgecreates@gmail.com>
// SPDX-FileCopyrightText: 2024 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._White.FootPrint;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Robust.Shared.Physics.Events;

namespace Content.Server._White.FootPrint;

public sealed class PuddleFootPrintsSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

   public override void Initialize()
   {
       base.Initialize();
       SubscribeLocalEvent<PuddleFootPrintsComponent, EndCollideEvent>(OnStepTrigger);
   }

    private void OnStepTrigger(EntityUid uid, PuddleFootPrintsComponent component, ref EndCollideEvent args)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance)
            || !TryComp<PuddleComponent>(uid, out var puddle)
            || puddle.LifeStage >= ComponentLifeStage.Stopping // Goobstation - container lookup causes ResolveSolution to assert when deleted first (EndCollide was triggered by component shutdown)
            || !TryComp<FootPrintsComponent>(args.OtherEntity, out var tripper)
            || !TryComp<SolutionContainerManagerComponent>(uid, out var solutionManager)
            || !_solutionContainer.ResolveSolution((uid, solutionManager), puddle.SolutionName, ref puddle.Solution, out var solutions))
            return;

        // alles gut!
        var totalSolutionQuantity = solutions.Contents.Sum(sol => (float)sol.Quantity);
        var waterQuantity = (from sol in solutions.Contents where sol.Reagent.Prototype == "Water" select (float) sol.Quantity).FirstOrDefault();

        if (waterQuantity / (totalSolutionQuantity / 100f) > component.OffPercent || solutions.Contents.Count <= 0)
            return;

        tripper.ReagentToTransfer =
            solutions.Contents.Aggregate((l, r) => l.Quantity > r.Quantity ? l : r).Reagent.Prototype;

        if (_appearance.TryGetData(uid, PuddleVisuals.SolutionColor, out var color, appearance)
            && _appearance.TryGetData(uid, PuddleVisuals.CurrentVolume, out var volume, appearance))
            AddColor((Color)color, (float)volume * component.SizeRatio, tripper);

        _solutionContainer.RemoveEachReagent(puddle.Solution.Value, 1);
    }

    private void AddColor(Color col, float quantity, FootPrintsComponent component)
    {
        component.PrintsColor = component.ColorQuantity == 0f ? col : Color.InterpolateBetween(component.PrintsColor, col, component.ColorInterpolationFactor);
        component.ColorQuantity += quantity;
    }
}