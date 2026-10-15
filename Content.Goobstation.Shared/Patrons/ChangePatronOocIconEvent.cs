using Content.Goobstation.Common.Patrons;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Patrons;

[Serializable, NetSerializable]
public sealed class ChangePatronOocIconEvent : EntityEventArgs
{
    public ProtoId<OocIconPrototype>? Icon;
}
