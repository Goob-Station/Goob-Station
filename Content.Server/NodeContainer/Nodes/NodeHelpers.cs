// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Shared.NodeContainer;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.NodeContainer.Nodes
{
    /// <summary>
    ///     Helper utilities for implementing <see cref="Node"/>.
    /// </summary>
    public static class NodeHelpers
    {
        private static SharedMapSystem MapSystem => IoCManager.Resolve<IEntityManager>().System<SharedMapSystem>();

        public static IEnumerable<Node> GetNodesInTile(EntityQuery<NodeContainerComponent> nodeQuery, MapGridComponent grid, Vector2i coords)
        {
            foreach (var entityUid in MapSystem.GetAnchoredEntities(grid.Owner, grid, coords))
            {
                if (!nodeQuery.TryGetComponent(entityUid, out var container))
                    continue;

                foreach (var node in container.Nodes.Values)
                {
                    yield return node;
                }
            }
        }

        public static IEnumerable<(Direction dir, Node node)> GetCardinalNeighborNodes(
            EntityQuery<NodeContainerComponent> nodeQuery,
            MapGridComponent grid,
            Vector2i coords,
            bool includeSameTile = true)
        {
            foreach (var (dir, entityUid) in GetCardinalNeighborCells(grid, coords, includeSameTile))
            {
                if (!nodeQuery.TryGetComponent(entityUid, out var container))
                    continue;

                foreach (var node in container.Nodes.Values)
                {
                    yield return (dir, node);
                }
            }
        }

        [SuppressMessage("ReSharper", "EnforceForeachStatementBraces")]
        public static IEnumerable<(Direction dir, EntityUid entity)> GetCardinalNeighborCells(
            MapGridComponent grid,
            Vector2i coords,
            bool includeSameTile = true)
        {
            if (includeSameTile)
            {
                foreach (var uid in MapSystem.GetAnchoredEntities(grid.Owner, grid, coords))
                    yield return (Direction.Invalid, uid);
            }

            foreach (var uid in MapSystem.GetAnchoredEntities(grid.Owner, grid, coords + (0, 1)))
                yield return (Direction.North, uid);

            foreach (var uid in MapSystem.GetAnchoredEntities(grid.Owner, grid, coords + (0, -1)))
                yield return (Direction.South, uid);

            foreach (var uid in MapSystem.GetAnchoredEntities(grid.Owner, grid, coords + (1, 0)))
                yield return (Direction.East, uid);

            foreach (var uid in MapSystem.GetAnchoredEntities(grid.Owner, grid, coords + (-1, 0)))
                yield return (Direction.West, uid);
        }
    }
}