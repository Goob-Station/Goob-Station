using Content.Server._Pirate.Plumbing.Nodes;
using Content.Server._Pirate.Plumbing.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared._Pirate.Plumbing;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Maps;
using Content.Shared.NodeContainer;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Server system that sends PlumbingNode directions and connection state to clients.
///     Also tracks floor coverage to hide connectors under floor tiles.
/// </summary>
public sealed partial class PlumbingConnectorAppearanceSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private ITileDefinitionManager _tileDefManager = default!;

    private static readonly PipeDirection[] _cardinalDirections =
    [
        PipeDirection.North,
        PipeDirection.South,
        PipeDirection.East,
        PipeDirection.West
    ];

    private const PipeDirection ManifoldSideADirection = PipeDirection.North;
    private const PipeDirection ManifoldSideBDirection = PipeDirection.South;
    private const int ManifoldSideAVisualSlots = 3;
    private const int ManifoldSideBVisualSlots = 3;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlumbingConnectorAppearanceComponent, NodeGroupsRebuilt>(OnNodeUpdate);
        SubscribeLocalEvent<NodeContainerComponent, NodeGroupsRebuilt>(OnAnyNodeGroupsRebuilt);
        SubscribeLocalEvent<PlumbingConnectorAppearanceComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TileChangedEvent>(OnTileChanged);
    }

    private void OnStartup(EntityUid uid, PlumbingConnectorAppearanceComponent component, ComponentStartup args)
        => UpdateAppearance(uid);

    private void OnNodeUpdate(EntityUid uid, PlumbingConnectorAppearanceComponent component, ref NodeGroupsRebuilt args)
        => UpdateAppearance(uid);

    private void OnAnyNodeGroupsRebuilt(EntityUid uid, NodeContainerComponent component, ref NodeGroupsRebuilt args)
    {
        // Avoid broad connector refreshes for unrelated node graphs.
        // This keeps rebuild churn localized to plumbing entities.
        if (!ContainsPlumbingNode(component))
            return;

        UpdateNearbyConnectorAppearances(uid);
    }

    private static bool ContainsPlumbingNode(NodeContainerComponent component)
    {
        foreach (var node in component.Nodes.Values)
        {
            if (node is PlumbingNode)
                return true;
        }

        return false;
    }

    private void UpdateNearbyConnectorAppearances(EntityUid uid)
    {
        if (!TryComp(uid, out TransformComponent? xform) ||
            xform.GridUid is not { } gridUid ||
            !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var tile = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);

        foreach (var checkTile in TilesToCheck(tile))
        {
            foreach (var ent in _map.GetAnchoredEntities(gridUid, grid, checkTile))
            {
                if (HasComp<PlumbingConnectorAppearanceComponent>(ent))
                    UpdateAppearance(ent);
            }
        }
    }

    private void OnTileChanged(ref TileChangedEvent ev)
    {
        var grid = ev.Entity.Comp;

        foreach (var change in ev.Changes)
        {
            // Refresh the changed tile and its neighbours: a machine next to the changed
            // tile has a connector stub pointing into it whose coverage may have flipped.
            foreach (var checkTile in TilesToCheck(change.GridIndices))
            {
                foreach (var uid in _map.GetAnchoredEntities(ev.Entity, grid, checkTile))
                {
                    if (HasComp<PlumbingConnectorAppearanceComponent>(uid))
                        UpdateAppearance(uid);
                }
            }
        }
    }

    private static IEnumerable<Vector2i> TilesToCheck(Vector2i center)
    {
        yield return center;
        yield return center + (0, 1);
        yield return center + (0, -1);
        yield return center + (1, 0);
        yield return center + (-1, 0);
    }

    private bool HasFloorCover(EntityUid gridUid, MapGridComponent grid, Vector2i position)
    {
        var tileRef = _map.GetTileRef(gridUid, grid, position);
        var tileDef = (ContentTileDefinition)_tileDefManager[tileRef.Tile.TypeId];
        return !tileDef.IsSubFloor;
    }

    private void UpdateAppearance(EntityUid uid, AppearanceComponent? appearance = null,
        NodeContainerComponent? container = null, TransformComponent? xform = null)
    {
        if (!Resolve(uid, ref appearance, ref container, ref xform, false))
            return;

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        if (!HasComp<PlumbingConnectorAppearanceComponent>(uid))
            return;

        var tile = _map.TileIndicesFor(xform.GridUid.Value, grid, xform.Coordinates);

        if (TryComp<PlumbingManifoldComponent>(uid, out var manifoldComp))
        {
            UpdateManifoldAppearance(uid, manifoldComp, appearance, container, xform, grid, tile);
            return;
        }

        var nodeDirections = PipeDirection.None;
        var connectedDirections = PipeDirection.None;
        var connectedLayers = 0;
        var inletDirections = PipeDirection.None;
        var outletDirections = PipeDirection.None;
        var mixingInletDirections = PipeDirection.None;

        var inletNodeNames = BuildInletNodeNameSet(uid);
        var outletNodeNames = BuildOutletNodeNameSet(uid);
        var mixingNodeNames = BuildMixingNodeNameSet(uid);

        foreach (var (nodeName, node) in container.Nodes)
        {
            if (node is not PlumbingNode plumbingNode)
                continue;

            var nodeDir = plumbingNode.CurrentPipeDirection;
            nodeDirections |= nodeDir;

            if (IsConfiguredNode(nodeName, mixingNodeNames))
            {
                mixingInletDirections |= nodeDir;
            }
            else
            {
                var isInlet = IsConfiguredNode(nodeName, inletNodeNames);
                var isOutlet = IsConfiguredNode(nodeName, outletNodeNames);

                if (isInlet)
                    inletDirections |= nodeDir;
                else if (isOutlet)
                    outletDirections |= nodeDir;
            }

            connectedDirections |= GetConnectedDirections(plumbingNode, nodeDir, tile, xform.GridUid.Value, grid, ref connectedLayers);
        }

        var coveredByFloor = HasFloorCover(xform.GridUid.Value, grid, tile);
        var coveredDirections = GetCoveredDirections(xform.GridUid.Value, grid, tile);

        _appearance.SetData(uid, PlumbingVisuals.NodeDirections, (int)nodeDirections, appearance);
        _appearance.SetData(uid, PlumbingVisuals.ConnectedDirections, (int)connectedDirections, appearance);
        _appearance.SetData(uid, PlumbingVisuals.ConnectedLayerByDirection, connectedLayers, appearance);
        _appearance.SetData(uid, PlumbingVisuals.InletDirections, (int)inletDirections, appearance);
        _appearance.SetData(uid, PlumbingVisuals.OutletDirections, (int)outletDirections, appearance);
        _appearance.SetData(uid, PlumbingVisuals.MixingInletDirections, (int)mixingInletDirections, appearance);
        _appearance.SetData(uid, PlumbingVisuals.ManifoldMode, false, appearance);
        _appearance.SetData(uid, PlumbingVisuals.CoveredByFloor, coveredByFloor, appearance);
        _appearance.SetData(uid, PlumbingVisuals.CoveredDirections, (int)coveredDirections, appearance);
    }

    /// <summary>
    ///     Returns the cardinal directions whose neighbouring tile is covered by a floor,
    ///     so connector stubs pointing under a floored tile can hide with the ducts there.
    /// </summary>
    private PipeDirection GetCoveredDirections(EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        var covered = PipeDirection.None;
        foreach (var dir in _cardinalDirections)
        {
            var neighborTile = dir switch
            {
                PipeDirection.North => tile + (0, 1),
                PipeDirection.South => tile + (0, -1),
                PipeDirection.East => tile + (1, 0),
                PipeDirection.West => tile + (-1, 0),
                _ => tile
            };

            if (HasFloorCover(gridUid, grid, neighborTile))
                covered |= dir;
        }

        return covered;
    }

    private void UpdateManifoldAppearance(EntityUid uid,
        PlumbingManifoldComponent manifoldComp,
        AppearanceComponent appearance,
        NodeContainerComponent container,
        TransformComponent xform,
        MapGridComponent grid,
        Vector2i tile)
    {
        var nodeDirections = PipeDirection.None;
        var connectedDirections = PipeDirection.None;
        var connectedLayers = 0;
        var connectedSlotsPacked = 0;
        var sideARoleMask = ManifoldSideADirection.RotatePipeDirection(xform.LocalRotation);
        var sideBRoleMask = ManifoldSideBDirection.RotatePipeDirection(xform.LocalRotation);
        var sideASlotCount = ManifoldSideAVisualSlots;
        var sideBSlotCount = ManifoldSideBVisualSlots;

        foreach (var (nodeName, node) in container.Nodes)
        {
            if (node is not PlumbingNode plumbingNode)
                continue;

            var nodeDir = plumbingNode.CurrentPipeDirection;
            nodeDirections |= nodeDir;

            var nodeConnectedDirs = GetConnectedDirections(plumbingNode, nodeDir, tile, xform.GridUid!.Value, grid, ref connectedLayers);
            connectedDirections |= nodeConnectedDirs;

            var isConnectedOnSide = nodeConnectedDirs.HasDirection(nodeDir);
            if (!isConnectedOnSide)
                continue;

            var isSideA = IsConfiguredNode(nodeName, manifoldComp.SideANodeNames)
                || nodeDir.HasDirection(sideARoleMask);

            if (isSideA)
            {
                var slotIndex = GetSlotIndexForLayer(plumbingNode.CurrentPipeLayer, sideASlotCount);
                connectedSlotsPacked = SetManifoldSlotConnected(connectedSlotsPacked, plumbingNode.CurrentPipeDirection, slotIndex);
            }

            var isSideB = IsConfiguredNode(nodeName, manifoldComp.SideBNodeNames)
                || nodeDir.HasDirection(sideBRoleMask);

            if (isSideB)
            {
                var slotIndex = GetSlotIndexForLayer(plumbingNode.CurrentPipeLayer, sideBSlotCount);
                connectedSlotsPacked = SetManifoldSlotConnected(connectedSlotsPacked, nodeDir, slotIndex);
            }
        }

        var coveredByFloor = HasFloorCover(xform.GridUid!.Value, grid, tile);

        _appearance.SetData(uid, PlumbingVisuals.NodeDirections, (int) nodeDirections, appearance);
        _appearance.SetData(uid, PlumbingVisuals.ConnectedDirections, (int) connectedDirections, appearance);
        _appearance.SetData(uid, PlumbingVisuals.ConnectedLayerByDirection, connectedLayers, appearance);
        _appearance.SetData(uid, PlumbingVisuals.InletDirections, (int) sideARoleMask, appearance);
        _appearance.SetData(uid, PlumbingVisuals.OutletDirections, (int) sideBRoleMask, appearance);
        _appearance.SetData(uid, PlumbingVisuals.MixingInletDirections, 0, appearance);
        _appearance.SetData(uid, PlumbingVisuals.ManifoldMode, true, appearance);
        _appearance.SetData(uid, PlumbingVisuals.ManifoldConnectedSlotsByDirection, connectedSlotsPacked, appearance);
        _appearance.SetData(uid, PlumbingVisuals.CoveredByFloor, coveredByFloor, appearance);
    }

    private static bool IsConfiguredNode(string nodeName, IEnumerable<string> configuredNames)
    {
        foreach (var configuredName in configuredNames)
        {
            if (nodeName.Equals(configuredName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private PipeDirection GetConnectedDirections(PlumbingNode node, PipeDirection nodeDir, Vector2i tile,
        EntityUid gridUid, MapGridComponent grid, ref int connectedLayers)
    {
        var connected = PipeDirection.None;

        foreach (var dir in _cardinalDirections)
        {
            if (!nodeDir.HasFlag(dir))
                continue;

            var neighborTile = dir switch
            {
                PipeDirection.North => tile + (0, 1),
                PipeDirection.South => tile + (0, -1),
                PipeDirection.East => tile + (1, 0),
                PipeDirection.West => tile + (-1, 0),
                _ => tile
            };

            foreach (var reachable in node.ReachableNodes)
            {
                if (reachable is not PlumbingNode reachablePlumbingNode || reachablePlumbingNode.Owner == node.Owner)
                    continue;

                if (reachablePlumbingNode.CurrentPipeLayer != node.CurrentPipeLayer)
                    continue;

                var otherTile = _map.TileIndicesFor(gridUid, grid, Transform(reachablePlumbingNode.Owner).Coordinates);
                if (otherTile == neighborTile)
                {
                    connected |= dir;
                    connectedLayers = SetConnectedLayer(connectedLayers, dir, node.CurrentPipeLayer);
                    break;
                }
            }
        }

        return connected;
    }

    private static int SetConnectedLayer(int packedData, PipeDirection direction, AtmosPipeLayer layer)
    {
        var shift = direction switch
        {
            PipeDirection.North => 0,
            PipeDirection.South => 4,
            PipeDirection.East => 8,
            PipeDirection.West => 12,
            _ => -1,
        };

        if (shift < 0)
            return packedData;

        var value = ((int) layer + 1) & 0xF;
        var clearMask = ~(0xF << shift);
        return (packedData & clearMask) | (value << shift);
    }

    private static int SetManifoldSlotConnected(int packedData, PipeDirection direction, int slotIndex)
    {
        if ((uint) slotIndex > 3)
            return packedData;

        var shift = direction switch
        {
            PipeDirection.North => 0,
            PipeDirection.South => 4,
            PipeDirection.East => 8,
            PipeDirection.West => 12,
            _ => -1,
        };

        if (shift < 0)
            return packedData;

        var slotMask = 1 << slotIndex;
        return packedData | (slotMask << shift);
    }

    private static int GetSlotIndexForLayer(AtmosPipeLayer layer, int slotCount)
    {
        if (slotCount <= 1)
            return 0;

        // For 3-slot manifold visuals, align slots to Tertiary/Primary/Secondary offsets.
        if (slotCount >= 3)
        {
            return layer switch
            {
                AtmosPipeLayer.Tertiary => 0,
                AtmosPipeLayer.Primary => 1,
                AtmosPipeLayer.Secondary => 2,
                _ => 1,
            };
        }

        return layer == AtmosPipeLayer.Primary ? 0 : 1;
    }

    private HashSet<string> BuildInletNodeNameSet(EntityUid uid)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!TryComp<PlumbingInletComponent>(uid, out var inletComp))
            return names;

        foreach (var name in inletComp.InletNames)
            names.Add(name);

        return names;
    }

    private HashSet<string> BuildOutletNodeNameSet(EntityUid uid)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!TryComp<PlumbingOutletComponent>(uid, out var outletComp))
            return names;

        foreach (var name in outletComp.OutletNames)
            names.Add(name);

        return names;
    }

    private HashSet<string> BuildMixingNodeNameSet(EntityUid uid)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!TryComp<PlumbingPillPressComponent>(uid, out var pillPressComp))
            return names;

        if (!string.IsNullOrWhiteSpace(pillPressComp.InletEastNodeName))
            names.Add(pillPressComp.InletEastNodeName);

        if (!string.IsNullOrWhiteSpace(pillPressComp.InletWestNodeName))
            names.Add(pillPressComp.InletWestNodeName);

        return names;
    }
}
