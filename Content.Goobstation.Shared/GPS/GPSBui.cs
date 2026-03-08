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
public sealed class GpsEntry(
    NetEntity netEntity,
    string? name,
    EntProtoId? prototypeId,
    bool isDistress,
    Color color,
    MapCoordinates coordinates)
    : IEquatable<GpsEntry>
{
    public readonly NetEntity NetEntity = netEntity;
    public readonly string? Name = name;
    public readonly EntProtoId? PrototypeId = prototypeId;
    public readonly bool IsDistress = isDistress;
    public readonly Color Color = color;
    public readonly MapCoordinates Coordinates = coordinates;

    // We compare only some stuff, since this comparer is used only to
    // update the entries and not the compass. Beware of the shitcode that was written to
    // satisfy the LINQ gods!!!
    public bool Equals(GpsEntry? other)
    {
        return !(NetEntity != other?.NetEntity
                || Name != other.Name
                || PrototypeId != other.PrototypeId
                || IsDistress != other.IsDistress
                || Color != other.Color);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is GpsEntry other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(NetEntity, Name, PrototypeId, IsDistress, Color);
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
