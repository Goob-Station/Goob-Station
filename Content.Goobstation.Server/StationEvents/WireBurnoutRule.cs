using System.Diagnostics;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Server.Power.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using JetBrains.Annotations;
using Content.Server.StationEvents.Events;
using Content.Server.Station.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.Power.NodeGroups;
using Content.Shared.Electrocution;
using Content.Shared.NodeContainer;

namespace Content.Goobstation.Server.StationEvents;

/// <summary>
/// This handles...
/// </summary>

[UsedImplicitly]
public sealed class WireBurnoutRule : StationEventSystem<WireBurnoutRuleComponent>
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

    protected override void Started(EntityUid uid,
        WireBurnoutRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        if (!TryGetRandomStation(out var chosenStation))
            return;
        if (!TryComp<StationDataComponent>(chosenStation, out var stationData))
            return;
        var grid = StationSystem.GetLargestGrid(stationData);
        if (grid is null)
            return;

        var stationWires = new List<Entity<TransformComponent>>();
        var query = EntityQueryEnumerator<CableComponent,ElectrifiedComponent, TransformComponent>();
        while (query.MoveNext(out var wire, out var cable,out var electric, out var xform))
        {
            if ( xform.GridUid == grid && //only affects cables on the main station
                 IsPowered(wire,electric, xform) //does not affect wires without power
                )
                stationWires.Add((wire,xform));
        }

        if(stationWires.Count == 0)
            return;

        RobustRandom.Shuffle(stationWires);

        var toBurn = Math.Min(RobustRandom.Next(1,10),stationWires.Count);

        for (var i = 0; i < toBurn; i++)
        {
            _transformSystem.Unanchor(stationWires[i]);
        }
    }

    private bool IsPowered(EntityUid uid, ElectrifiedComponent electrified, TransformComponent transform)
    {
        if (!electrified.Enabled && electrified.RequirePower && PoweredNode(uid, electrified) == null)
            return false;

        return true;
    }

    private Node? PoweredNode(EntityUid uid, ElectrifiedComponent electrified, NodeContainerComponent? nodeContainer = null)
    {
        if (!Resolve(uid, ref nodeContainer, false))
            return null;

        return TryNode(electrified.HighVoltageNode) ?? TryNode(electrified.MediumVoltageNode) ?? TryNode(electrified.LowVoltageNode);

        Node? TryNode(string? id)
        {
            if (id != null &&
                _nodeContainer.TryGetNode<Node>(nodeContainer, id, out var tryNode) &&
                tryNode.NodeGroup is IBasePowerNet { NetworkNode: { LastCombinedMaxSupply: > 0 } })
            {
                return tryNode;
            }
            return null;
        }
    }
}
