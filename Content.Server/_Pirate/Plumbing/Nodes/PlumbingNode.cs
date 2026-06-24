using Content.Server._Pirate.Plumbing.NodeGroups;
using Content.Server._Pirate.Plumbing.EntitySystems;
using Content.Server._Pirate.Plumbing.Components;
using Content.Server.NodeContainer.Nodes;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.NodeContainer;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Map.Components;

namespace Content.Server._Pirate.Plumbing.Nodes;

/// <summary>
///     A pipe node for plumbing systems using reagents instead of gases.
///     Extends PipeNode to reuse all pipe connection logic (direction, color, layer matching).
///     The only difference is it uses PlumbingNet instead of PipeNet.
/// </summary>
[DataDefinition]
[Virtual]
public partial class PlumbingNode : PipeNode
{
    private static readonly ProtoId<TagPrototype> _plumbingDuctTag = "PlumbingDuct";
    private static readonly Dictionary<(EntityUid Owner, string NodeName, PipeDirection Direction), EntityUid> _selectedDuctByMachineSide = new();

    /// <summary>
    ///     The <see cref="IPlumbingNet"/> this plumbing duct is part of.
    /// </summary>
    [ViewVariables]
    public IPlumbingNet? PlumbingNet => (IPlumbingNet?) NodeGroup;

    public override void Initialize(EntityUid owner, IEntityManager entMan)
    {
        base.Initialize(owner, entMan);

        if (entMan.HasComponent<PlumbingManifoldComponent>(owner))
            return;

        if (entMan.TryGetComponent<AtmosPipeLayersComponent>(owner, out var layers))
            CurrentPipeLayer = layers.CurrentPipeLayer;
    }

    public override IEnumerable<Node> GetReachableNodes(
        TransformComponent xform,
        EntityQuery<NodeContainerComponent> nodeQuery,
        EntityQuery<TransformComponent> xformQuery,
        MapGridComponent? grid,
        IEntityManager entMan)
    {
        var tags = entMan.System<TagSystem>();
        var isPlumbingDuct = tags.HasTag(Owner, _plumbingDuctTag);
        var yielded = new HashSet<Node>();
        var nodeName = Name ?? "__unnamed";

        var manifoldSystem = entMan.System<PlumbingManifoldSystem>();
        if (manifoldSystem.TryGetBridgedNodes(Owner, nodeName, nodeQuery, out var bridgedNodes))
        {
            foreach (var siblingNode in bridgedNodes)
            {
                if (siblingNode == this)
                    continue;

                if (yielded.Add(siblingNode))
                    yield return siblingNode;
            }
        }

        if (!isPlumbingDuct &&
            xform.Anchored &&
            grid is { } machineGrid)
        {
            var position = machineGrid.TileIndicesFor(xform.Coordinates);
            var selectedByDirection = new Dictionary<PipeDirection, PipeNode>();

            // Optional internal outlet linking.
            // When enabled on PlumbingOutletComponent, all configured outlet nodes on the same
            // machine are connected together in the graph so attached duct networks bridge.
            if (entMan.TryGetComponent<PlumbingOutletComponent>(Owner, out var outletComp) &&
                outletComp.ConnectedOutlets &&
                IsConfiguredOutletNode(nodeName, outletComp) &&
                nodeQuery.TryGetComponent(Owner, out var ownerContainer))
            {
                foreach (var (siblingName, siblingNode) in ownerContainer.Nodes)
                {
                    if (!IsConfiguredOutletNode(siblingName, outletComp))
                        continue;

                    if (siblingNode is not PlumbingNode siblingPlumbingNode || siblingPlumbingNode == this)
                        continue;

                    if (yielded.Add(siblingPlumbingNode))
                        yield return siblingPlumbingNode;
                }
            }

            foreach (var direction in GetCardinalDirections(CurrentPipeDirection))
            {
                var sideKey = (Owner, nodeName, direction);
                PipeNode? firstConnectedCandidate = null;

                foreach (var pipe in PipesInDirection(position, direction, machineGrid, nodeQuery))
                {
                    if (!pipe.CurrentPipeDirection.HasDirection(direction.GetOpposite()))
                        continue;

                    if (pipe.NodeGroupID != NodeGroupID)
                        continue;

                    firstConnectedCandidate ??= pipe;

                    break;
                }

                if (firstConnectedCandidate == null)
                {
                    _selectedDuctByMachineSide.Remove(sideKey);
                    continue;
                }

                _selectedDuctByMachineSide[sideKey] = firstConnectedCandidate.Owner;
                CurrentPipeLayer = firstConnectedCandidate.CurrentPipeLayer;
                selectedByDirection[direction] = firstConnectedCandidate;
            }

            if (selectedByDirection.Count > 0)
            {
                foreach (var node in selectedByDirection.Values)
                {
                    if (yielded.Add(node))
                        yield return node;
                }
            }
            yield break;
        }

        if (isPlumbingDuct &&
            xform.Anchored &&
            grid is { } ductGrid)
        {
            var pos = ductGrid.TileIndicesFor(xform.Coordinates);

            foreach (var direction in GetCardinalDirections(CurrentPipeDirection))
            {
                foreach (var pipe in PipesInDirection(pos, direction, ductGrid, nodeQuery))
                {
                    if (pipe.NodeGroupID != NodeGroupID)
                        continue;

                    if (!pipe.CurrentPipeDirection.HasDirection(direction.GetOpposite()))
                        continue;

                    var otherIsPlumbingDuct = tags.HasTag(pipe.Owner, _plumbingDuctTag);
                    if (otherIsPlumbingDuct)
                        continue;

                    var machineNodeName = pipe.Name ?? "__unnamed";
                    var machineSide = direction.GetOpposite();
                    var machineSideKey = (pipe.Owner, machineNodeName, machineSide);
                    if (_selectedDuctByMachineSide.TryGetValue(machineSideKey, out var selectedDuct) &&
                        selectedDuct != Owner)
                        continue;

                    if (yielded.Add(pipe))
                        yield return pipe;
                }
            }
        }

        foreach (var node in base.GetReachableNodes(xform, nodeQuery, xformQuery, grid, entMan))
        {
            if (yielded.Add(node))
                yield return node;
        }
    }

    private static IEnumerable<PipeDirection> GetCardinalDirections(PipeDirection directions)
    {
        if (directions.HasDirection(PipeDirection.North))
            yield return PipeDirection.North;
        if (directions.HasDirection(PipeDirection.South))
            yield return PipeDirection.South;
        if (directions.HasDirection(PipeDirection.East))
            yield return PipeDirection.East;
        if (directions.HasDirection(PipeDirection.West))
            yield return PipeDirection.West;
    }

    private static bool IsConfiguredOutletNode(string nodeName, PlumbingOutletComponent outlet)
    {
        foreach (var configuredName in outlet.OutletNames)
        {
            if (nodeName.Equals(configuredName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
