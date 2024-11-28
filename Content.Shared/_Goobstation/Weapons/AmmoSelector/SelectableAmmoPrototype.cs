using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

[Serializable, NetSerializable, DataDefinition]
[Prototype("selectableAmmo")]
public sealed partial class SelectableAmmoPrototype : IPrototype, ICloneable
{
    [IdDataField]
    public string ID { get; private set; }

    [DataField(required: true)]
    public SpriteSpecifier Icon;

    [DataField(required: true)]
    public string Desc;

    [DataField(required: true)]
    public EntProtoId ProtoId;

    [DataField]
    public Color? Color;

    public object Clone()
    {
        return new SelectableAmmoPrototype
        {
            ID = ID,
            Icon = Icon,
            Desc = Desc,
            ProtoId = ProtoId,
            Color = Color,
        };
    }
}
