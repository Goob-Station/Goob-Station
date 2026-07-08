using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil;

[Prototype("devilBranchPrototype")]
public sealed partial class DevilBranchPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField("powerActions", required: true)]
    public Dictionary<DevilPowerLevel, List<EntProtoId>> PowerActions = new();
}

public enum DevilPowerLevel : byte
{
    None,
    Weak,
    Moderate,
    Powerful,
}
