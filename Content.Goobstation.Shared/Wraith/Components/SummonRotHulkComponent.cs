using Content.Shared.Tag;
using Linguini.Shared.Util;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SummonRotHulkComponent : Component
{
    [DataField]
    public ProtoId<TagPrototype> TrashTag = "Trash";

    [DataField]
    public EntProtoId RotHulkProto = "MobRotHulk";

    [DataField]
    public EntProtoId BuffRotHulkProto = "MobRotHulkBuff";

    [DataField]
    public int MinTrash = 10;

    [DataField]
    public int MaxTrash = 40;

    [DataField]
    public int BuffThreshold = 30;

    [DataField]
    public float SearchRadius = 3f;
}
