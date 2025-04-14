using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.Borgs;

[Serializable, NetSerializable]
public sealed class BorgSelectSubtypeMessage(ProtoId<BorgSubtypePrototype> subtype) : BoundUserInterfaceMessage
{
    public ProtoId<BorgSubtypePrototype> Subtype = subtype;
}

[ByRefEvent]
public readonly record struct BorgSubtypeChangedEvent(ProtoId<BorgSubtypePrototype> Subtype);
