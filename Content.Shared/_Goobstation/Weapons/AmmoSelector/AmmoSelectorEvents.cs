using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

[Serializable, NetSerializable]
public sealed class AmmoSelectedMessage(ProtoId<SelectableAmmoPrototype> protoId) : BoundUserInterfaceMessage
{
    public ProtoId<SelectableAmmoPrototype> ProtoId { get; } = protoId;
}

[Serializable, NetSerializable]
public enum AmmoSelectorUiKey : byte
{
    Key
}
