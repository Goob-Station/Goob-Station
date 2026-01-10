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
public sealed class GpsEntry
{
    public NetEntity NetEntity;
    public string? Name;
    public EntProtoId? PrototypeId;
    public bool IsDistress;
    public Color Color;
    public MapCoordinates Coordinates;

    public bool Equals(GpsEntry? other)
    {
        // We compare only some stuff, since this comparer is used only to
        // update the entries and not the compass.
        return !(NetEntity != other?.NetEntity
                || Name != other.Name
                || PrototypeId != other.PrototypeId
                || IsDistress != other.IsDistress
                || Color != other.Color);
    }
}

[Serializable, NetSerializable]
public sealed class GpsSetTrackedEntityMessage(NetEntity? netEntity) : BoundUserInterfaceMessage
{
    public NetEntity? NetEntity = netEntity;
}

[Serializable, NetSerializable]
public sealed class GpsSetGpsNameMessage(string gpsName) : BoundUserInterfaceMessage
{
    public string GpsName = gpsName;
}

[Serializable, NetSerializable]
public sealed class GpsSetInDistressMessage(bool inDistress) : BoundUserInterfaceMessage
{
    public bool InDistress = inDistress;
}

[Serializable, NetSerializable]
public sealed class GpsSetEnabledMessage(bool inDistress) : BoundUserInterfaceMessage
{
    public bool Enabled = inDistress;
}
