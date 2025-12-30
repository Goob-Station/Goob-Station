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
public sealed class GpsEntry : IEquatable<GpsEntry>
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
public sealed class GpsSetEnabledMessage : BoundUserInterfaceMessage
{
    public bool Enabled;
    public GpsSetEnabledMessage(bool inDistress)
    {
        Enabled = inDistress;
    }
}
