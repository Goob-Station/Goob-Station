using Content.Server._Pirate.Plumbing.Components;
using Content.Server._Pirate.Plumbing.Nodes;
using Content.Shared.NodeContainer;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
/// Passive manifold bridge behavior.
/// Links connected side A and side B nodes as internal connections.
/// </summary>
public sealed class PlumbingManifoldSystem : EntitySystem
{
    /// <summary>
    /// Gets all sibling manifold nodes that should be internally bridged with the provided node.
    /// </summary>
    /// <param name="owner">The potential manifold entity.</param>
    /// <param name="nodeName">The source node name on the manifold.</param>
    /// <param name="nodeQuery">NodeContainer query cache from caller.</param>
    /// <param name="bridged">Resolved bridged sibling plumbing nodes.</param>
    /// <returns>True when one or more bridged siblings are found for <paramref name="nodeName"/>.</returns>
    public bool TryGetBridgedNodes(EntityUid owner,
        string nodeName,
        EntityQuery<NodeContainerComponent> nodeQuery,
        out List<Node> bridged)
    {
        var bridgedSet = new HashSet<Node>();
        bridged = new();

        if (!TryComp<PlumbingManifoldComponent>(owner, out var manifoldComp) ||
            !nodeQuery.TryGetComponent(owner, out var manifoldContainer))
            return false;

        var isSideA = IsConfiguredNode(nodeName, manifoldComp.SideANodeNames);
        var isSideB = IsConfiguredNode(nodeName, manifoldComp.SideBNodeNames);
        if (!isSideA && !isSideB)
            return false;

        // Normally a node is either side A or side B.
        // If misconfigured as both, bridge to both sets for resilience.
        var targetSets = new List<HashSet<string>>(2);
        if (isSideA)
            targetSets.Add(manifoldComp.SideBNodeNames);

        if (isSideB)
            targetSets.Add(manifoldComp.SideANodeNames);

        foreach (var (siblingName, siblingNode) in manifoldContainer.Nodes)
        {
            var isTarget = false;
            foreach (var targetSet in targetSets)
            {
                if (!IsConfiguredNode(siblingName, targetSet))
                    continue;

                isTarget = true;
                break;
            }

            if (!isTarget)
                continue;

            if (siblingNode is not PlumbingNode siblingPlumbingNode)
                continue;

            bridgedSet.Add(siblingPlumbingNode);
        }

        bridged.AddRange(bridgedSet);

        return bridged.Count > 0;
    }

    private static bool IsConfiguredNode(string nodeName, HashSet<string> configuredNames)
    {
        foreach (var configuredName in configuredNames)
        {
            if (nodeName.Equals(configuredName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
