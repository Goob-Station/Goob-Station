using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.GPS;

[Serializable, NetSerializable]
public enum GpsUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class GpsBoundUserInterfaceState : BoundUserInterfaceState
{
    public string GpsName;
    public bool InDistress;
    public NetEntity? TrackedEntity;
    public MapCoordinates? GpsCoordinates;
    public List<GpsEntry> GpsEntries;

    public GpsBoundUserInterfaceState(string gpsName, bool inDistress, NetEntity? trackedEntity, MapCoordinates? gpsCoordinates, List<GpsEntry> gpsEntries)
    {
        GpsName = gpsName;
        InDistress = inDistress;
        TrackedEntity = trackedEntity;
        GpsCoordinates = gpsCoordinates;
        GpsEntries = gpsEntries;
    }
}

[Serializable, NetSerializable]
public sealed class GpsEntry
{
    public NetEntity NetEntity;
    public string? Name;
    public EntProtoId? PrototypeId;
    public bool IsDistress;
    public Color Color;
    public MapCoordinates Coordinates;
}


[Serializable, NetSerializable]
public sealed class GpsSetTrackedEntityMessage : BoundUserInterfaceMessage
{
    public NetEntity? NetEntity;
    public GpsSetTrackedEntityMessage(NetEntity? netEntity)
    {
        NetEntity = netEntity;
    }
}

[Serializable, NetSerializable]
public sealed class GpsSetGpsNameMessage : BoundUserInterfaceMessage
{
    public string GpsName;
    public GpsSetGpsNameMessage(string gpsName)
    {
        GpsName = gpsName;
    }
}

[Serializable, NetSerializable]
public sealed class GpsSetInDistressMessage : BoundUserInterfaceMessage
{
    public bool InDistress;
    public GpsSetInDistressMessage(bool inDistress)
    {
        InDistress = inDistress;
    }
}

[Serializable, NetSerializable]
public sealed class GpsNameChangedMessage : BoundUserInterfaceMessage
{
    public readonly string GpsName;
    public GpsNameChangedMessage(string gpsName)
    {
        GpsName = gpsName;
    }
}

[Serializable, NetSerializable]
public sealed class GpsDistressChangedMessage : BoundUserInterfaceMessage
{
    public readonly bool InDistress;
    public GpsDistressChangedMessage(bool inDistress)
    {
        InDistress = inDistress;
    }
}

[Serializable, NetSerializable]
public sealed class GpsTrackedEntityChangedMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? TrackedEntity;
    public GpsTrackedEntityChangedMessage(NetEntity? trackedEntity)
    {
        TrackedEntity = trackedEntity;
    }
}

[Serializable, NetSerializable]
public sealed class GpsEntriesChangedMessage : BoundUserInterfaceMessage
{
    public readonly List<GpsEntry> GpsEntries;
    public GpsEntriesChangedMessage(List<GpsEntry> gpsEntries)
    {
        GpsEntries = gpsEntries;
    }
}

[Serializable, NetSerializable]
public sealed class GpsUpdateTrackedCoordinatesMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity NetEntity;
    public readonly MapCoordinates Coordinates;

    public GpsUpdateTrackedCoordinatesMessage(NetEntity netEntity, MapCoordinates coordinates)
    {
        NetEntity = netEntity;
        Coordinates = coordinates;
    }
}
