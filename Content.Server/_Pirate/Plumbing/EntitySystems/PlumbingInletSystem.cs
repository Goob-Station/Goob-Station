using Content.Server._Pirate.Plumbing.Components;
using Content.Server._Pirate.Plumbing.Nodes;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.NodeContainer;
using JetBrains.Annotations;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Handles plumbing inlet behavior: actively pulls reagents from inlet nodes into a solution.
/// </summary>
[UsedImplicitly]
public sealed partial class PlumbingInletSystem : EntitySystem
{
    [Dependency] private SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private PlumbingPullSystem _pullSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlumbingInletComponent, PlumbingDeviceUpdateEvent>(OnInletUpdate);
    }

    private void OnInletUpdate(Entity<PlumbingInletComponent> ent, ref PlumbingDeviceUpdateEvent args)
    {
        // When the pill press is in mixing mode, don't pull from the normal inlet
        // mixing inlets handle all pulling instead.
        if (TryComp<PlumbingPillPressComponent>(ent.Owner, out var pillPress) && pillPress.MixingEnabled)
            return;

        // Plumbing filters handle inlet pulling in PiratePlumbingFilterSystem to split into lanes.
        if (HasComp<PiratePlumbingFilterComponent>(ent.Owner))
            return;

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out var solutionEnt, out var solution))
            return;

        if (solution.AvailableVolume <= 0)
            return;

        if (!TryComp<NodeContainerComponent>(ent.Owner, out var nodeContainer))
            return;

        // Pull from each inlet's network independently, sharing a single transfer budget
        // so total throughput stays at TransferAmount regardless of inlet count.
        var remaining = ent.Comp.TransferAmount;

        foreach (var inletName in ent.Comp.InletNames)
        {
            if (remaining <= 0 || solution.AvailableVolume <= 0)
                break;

            if (!nodeContainer.Nodes.TryGetValue(inletName, out var node))
                continue;

            if (node is not PlumbingNode plumbingNode || plumbingNode.PlumbingNet == null)
                continue;

            var roundRobinIndex = ent.Comp.RoundRobinIndices.GetValueOrDefault(inletName, 0);
            var (pulled, nextIndex) = _pullSystem.PullFromNetwork(ent.Owner, plumbingNode.PlumbingNet, solutionEnt.Value, remaining, roundRobinIndex);
            ent.Comp.RoundRobinIndices[inletName] = nextIndex;
            remaining -= pulled;
        }
    }
}
