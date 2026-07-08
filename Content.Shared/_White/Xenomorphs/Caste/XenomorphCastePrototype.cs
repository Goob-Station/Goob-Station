using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Caste;

[Prototype("xenomorphCaste")]
public sealed partial class XenomorphCastePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public string Name = string.Empty;

    [DataField]
    public ProtoId<XenomorphCastePrototype>? NeedCasteDeath;

    [DataField]
    public int MaxCount;
}
