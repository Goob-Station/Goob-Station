using Content.Server.NodeContainer.NodeGroups;
using Content.Shared.NodeContainer.NodeGroups;

namespace Content.Server._Pirate.Plumbing.NodeGroups;

public interface IPlumbingNet : INodeGroup
{
}

[NodeGroup(NodeGroupID.Plumbing)]
public sealed class PlumbingNet : BaseNodeGroup, IPlumbingNet
{
    public override string? GetDebugData()
        => $"Nodes: {NodeCount}";
}
