using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.MartialArts;

[Serializable,NetSerializable]
public sealed class ComboBeingPerformedEvent(ProtoId<ComboPrototype> protoId) : EntityEventArgs
{
    public ProtoId<ComboPrototype> ProtoId = protoId;
}
