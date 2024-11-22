using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

[Serializable, NetSerializable]
public sealed class AmmoSelectedMessage : BoundUserInterfaceMessage
{
    public ProtoId<SelectableAmmoPrototype> ProtoId;

    public AmmoSelectedMessage(ProtoId<SelectableAmmoPrototype> protoId)
    {
        ProtoId = protoId;
    }
}

[Serializable, NetSerializable]
public enum AmmoSelectorUiKey : byte
{
    Key
}
