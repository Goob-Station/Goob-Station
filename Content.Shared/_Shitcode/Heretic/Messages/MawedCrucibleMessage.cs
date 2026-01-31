using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Heretic.Messages;

[Serializable, NetSerializable]
public sealed class MawedCrucibleMessage(EntProtoId proto) : BoundUserInterfaceMessage
{
    public readonly EntProtoId Proto = proto;
}

[Serializable, NetSerializable]
public enum MawedCrucibleUiKey : byte
{
    Key
}
