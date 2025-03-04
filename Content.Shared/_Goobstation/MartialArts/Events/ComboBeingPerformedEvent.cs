using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.MartialArts.Events;

[Serializable,NetSerializable]
public sealed class ComboBeingPerformedEvent(ProtoId<ComboPrototype> protoId) : EntityEventArgs
{
    public ProtoId<ComboPrototype> ProtoId = protoId;
}
