using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Caste;

[Prototype]
public sealed partial class XenomorphCastePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = string.Empty;

    [DataField]
    public ProtoId<XenomorphCastePrototype>? NeedCasteDeath;

    [DataField]
    public int MaxCount;
}
