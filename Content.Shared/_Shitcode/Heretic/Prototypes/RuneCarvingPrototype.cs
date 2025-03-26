using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Heretic.Prototypes;

[Serializable, NetSerializable, DataDefinition]
[Prototype("runeCarving")]
public sealed partial class RuneCarvingPrototype : IPrototype, ICloneable
{
    [IdDataField]
    public string ID { get; private set; }

    [DataField(required: true)]
    public SpriteSpecifier Icon;

    [DataField(required: true)]
    public string Desc;

    [DataField(required: true)]
    public EntProtoId ProtoId;

    public object Clone()
    {
        return new RuneCarvingPrototype
        {
            ID = ID,
            Icon = Icon,
            Desc = Desc,
            ProtoId = ProtoId,
        };
    }
}
