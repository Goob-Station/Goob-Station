using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.UserInterface;

[Serializable, NetSerializable]
public enum EntityRadialMenuKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed partial class EntityRadialMenuSelectMessage(EntProtoId id) : BoundUserInterfaceMessage
{
    public EntProtoId ID = id;
}

[Serializable, NetSerializable]
public sealed partial class EntityRadialMenuUpdateMessage(List<EntProtoId> ids) : BoundUserInterfaceMessage
{
    public List<EntProtoId> IDs = ids;
}
