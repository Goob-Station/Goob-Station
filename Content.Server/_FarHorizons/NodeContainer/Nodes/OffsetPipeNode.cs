using Content.Server.NodeContainer.Nodes;
using Content.Shared.Atmos;
using Content.Shared.NodeContainer;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;

namespace Content.Server._FarHorizons.NodeContainer.Nodes;

/// <summary>
///     Connects with other <see cref="PipeNode"/>s whose <see cref="PipeDirection"/>
///     and <see cref="CurrentPipeLayer"/> correctly correspond, but at a distance.
/// </summary>
/// <remarks>
/// <para>
/// Due to its nature, it should be queued to reflood via the <see cref="NodeGroupSystem"/> if it does not have any ReachableNodes.
/// </para>
/// </remarks>
/// This is kinda just a nasty copy of <see cref="PipeNode"/>, but the LinkableNodesInDirection() 
/// and PipesInDirection() methods had too restricted of access.
[DataDefinition]
[Virtual]
public partial class OffsetPipeNode : PipeNode
{
    // Described in terms of directions to make YML easier to understand/wrtie
    /// <summary>
    /// Offest of the node East relative to the grid plane. Negative values offset West.
    /// </summary>
    [DataField]
    public int OffsetEast = 0;
    /// <summary>
    /// Offest of the node North relative to the grid plane. Negative values offset South.
    /// </summary>
    [DataField]
    public int OffsetNorth = 0;

    public override IEnumerable<Node> GetReachableNodes(TransformComponent xform,
            EntityQuery<NodeContainerComponent> nodeQuery,
            EntityQuery<TransformComponent> xformQuery,
            MapGridComponent? grid,
            IEntityManager entMan)
    {

        if (!xform.Anchored || grid == null)
            yield break;

        var pos = grid.TileIndicesFor(xform.Coordinates);

        for (var i = 0; i < PipeDirectionHelpers.PipeDirections; i++)
        {
            var pipeDir = (PipeDirection)(1 << i);

            if (!CurrentPipeDirection.HasDirection(pipeDir))
                continue;

            foreach (var pipe in OffsetLinkableNodesInDirection(pos, pipeDir, grid, nodeQuery))
            {
                yield return pipe;
            }
        }
    }

    /// <summary>
    ///     Gets the pipes that can connect to us from entities on the tile or adjacent/distant in a direction.
    /// </summary>
    private IEnumerable<PipeNode> OffsetLinkableNodesInDirection(Vector2i pos, PipeDirection pipeDir, MapGridComponent grid,
        EntityQuery<NodeContainerComponent> nodeQuery)
    {
        foreach (var pipe in OffsetPipesInDirection(pos, pipeDir, grid, nodeQuery))
        {
            if (pipe.NodeGroupID == NodeGroupID
                && pipe.CurrentPipeLayer == CurrentPipeLayer
                && pipe.CurrentPipeDirection.HasDirection(pipeDir.GetOpposite()))
            {
                yield return pipe;
            }
        }
    }

    /// <summary>
    ///     Gets the pipes from entities on the tile adjacent/distant in a direction.
    /// </summary>
    protected IEnumerable<PipeNode> OffsetPipesInDirection(Vector2i pos, PipeDirection pipeDir, MapGridComponent grid,
        EntityQuery<NodeContainerComponent> nodeQuery)
    {
        var OffsetVector = new Vector2i(OffsetEast, OffsetNorth);
        var offsetPos = pos + OffsetVector.Rotate(OriginalPipeDirection.ToAngle() - pipeDir.ToAngle());

        foreach (var entity in grid.GetAnchoredEntities(offsetPos))
        {
            if (!nodeQuery.TryGetComponent(entity, out var container))
                continue;

            foreach (var node in container.Nodes.Values)
            {
                if (node is PipeNode pipe)
                    yield return pipe;
            }
        }
    }
}