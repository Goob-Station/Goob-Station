using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Traitor.Contracts;

[Prototype("contract")]
[Serializable, NetSerializable]
public sealed partial class ContractPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name { get; private set; } = string.Empty;

    [DataField(required: true)]
    public LocId Description { get; private set; } = string.Empty;

    [DataField(required: true)]
    public int Reward { get; private set; }

    [DataField]
    public int Difficulty { get; private set; } = 1;

    [DataField(required: true)]
    public EntProtoId ObjectivePrototype { get; private set; } = string.Empty;

    [DataField]
    public SpriteSpecifier? Icon { get; private set; }

    [DataField]
    public bool Repeatable { get; private set; } = false;

    [DataField]
    public int MaxActive { get; private set; } = 1;

    [DataField]
    public bool PartialReward { get; private set; } = false;

    [DataField]
    public ProtoId<ContractCategoryPrototype>? Category { get; private set; }
}

[Prototype("contractCategory")]
[Serializable, NetSerializable]
public sealed partial class ContractCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name { get; private set; } = string.Empty;

    [DataField]
    public int Priority { get; private set; } = 0;
}
