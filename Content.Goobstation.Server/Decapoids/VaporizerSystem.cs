// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Shared.Decapoids;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.Decapoids;

public sealed partial class VaporizerSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private void ProcessVaporizerTank(EntityUid uid, VaporizerComponent vaporizer, GasTankComponent gasTank, SolutionContainerManagerComponent solutionManager)
    {
        if (gasTank.Air.Pressure >= vaporizer.MaxPressure)
            return;

        if (!_solution.TryGetSolution((uid, solutionManager), vaporizer.LiquidTank, out _, out var solution))
            return;

        // Look for a valid reagent
        foreach (var reagent in solution.Contents)
        {
            if (reagent.Reagent.Prototype != vaporizer.ExpectedReagent)
                continue;

            // If a valid reagent is found, consume it and produce gas.
            var reagentConsumed = solution.RemoveReagent(new ReagentQuantity(reagent.Reagent, vaporizer.ReagentPerSecond * vaporizer.ProcessDelay.TotalSeconds));
            gasTank.Air.AdjustMoles((int) vaporizer.OutputGas, (float) reagentConsumed * vaporizer.ReagentToMoles);
            break;
        }
    }

    public override void Update(float frameTime)
    {
        var enumerator = EntityQueryEnumerator<VaporizerComponent, GasTankComponent, SolutionContainerManagerComponent>();

        while (enumerator.MoveNext(out var uid, out var vaporizer, out var gasTank, out var solutionManager))
        {
            if (_timing.CurTime >= vaporizer.NextProcess)
            {
                ProcessVaporizerTank(uid, vaporizer, gasTank, solutionManager);
                vaporizer.NextProcess = _timing.CurTime + vaporizer.ProcessDelay;
            }
        }
    }
}
