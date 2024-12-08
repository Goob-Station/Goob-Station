using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Gateway;

[Serializable, NetSerializable]
public enum LavalandGatewayVisuals : byte
{
    Active
}

[Serializable, NetSerializable]
public enum LavalandGatewayVisualLayers : byte
{
    Portal
}

[Serializable, NetSerializable]
public enum LavalandGatewayUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class LavalandGatewayBoundUserInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// List of enabled destinations and information about them.
    /// </summary>
    public readonly List<(NetEntity, string, TimeSpan, bool)> Destinations;

    /// <summary>
    /// Which destination it is currently linked to, if any.
    /// </summary>
    public readonly NetEntity? Current;

    /// <summary>
    /// Time the portal will close at.
    /// </summary>
    public readonly TimeSpan NextClose;

    /// <summary>
    /// Time the portal last opened at.
    /// </summary>
    public readonly TimeSpan LastOpen;

    public LavalandGatewayBoundUserInterfaceState(List<(NetEntity, string, TimeSpan, bool)> destinations,
        NetEntity? current, TimeSpan nextClose, TimeSpan lastOpen)
    {
        Destinations = destinations;
        Current = current;
        NextClose = nextClose;
        LastOpen = lastOpen;
    }
}

[Serializable, NetSerializable]
public sealed class LavalandGatewayOpenPortalMessage : BoundUserInterfaceMessage
{
    public NetEntity Destination;

    public LavalandGatewayOpenPortalMessage(NetEntity destination)
    {
        Destination = destination;
    }
}
