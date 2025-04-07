// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Measure the mess of the station in puddles on the floor
///
///   Jani - JaniMetricComponent.Puddles points per BaselineQty of various substances
/// </summary>
public sealed class PuddleMetricSystem : ChaosMetricSystem<PuddleMetricComponent>
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

    public override ChaosMetrics CalculateChaos(EntityUid uid, PuddleMetricComponent component, CalculateChaosEvent args)
    {
        // Add up the pain of all the puddles
        var query = EntityQueryEnumerator<PuddleComponent, SolutionContainerManagerComponent>();
        double mess = 0;
        while (query.MoveNext(out var puddleUid, out var puddle, out var solutionMgr))
        {
            if (!_solutionContainerSystem.TryGetSolution(puddleUid, puddle.SolutionName, out var puddleSolution))
                continue;

            double puddleChaos = 0.0f;
            foreach (var substance in puddleSolution.Value.Comp.Solution.Contents)
            {
                var substanceChaos = component.Puddles.GetValueOrDefault(substance.Reagent.Prototype, component.PuddleDefault).Double();
                puddleChaos += Math.Round(substanceChaos * substance.Quantity.Double());
            }

            mess += puddleChaos;
        }

        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, double>()
        {
            {ChaosMetric.Mess, mess},
        });
        return chaos;
    }
}