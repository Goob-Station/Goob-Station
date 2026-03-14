using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil;

[Prototype("devilBranchPrototype")]
public sealed class DevilBranchPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField("powerActions", required: true)]
    public Dictionary<DevilPowerLevel, List<EntProtoId>> PowerActions = new();

    [DataField("conditionalUnlocks")]
    public List<DevilConditionalUnlock> ConditionalUnlocks = new();

}

public enum DevilPowerLevel : byte
{
    None,
    Weak,
    Moderate,
    Powerful,
}

[DataDefinition]
public sealed partial class DevilConditionalUnlock
{
    [DataField(required: true)]
    public string RequiredComponent = default!;

    [DataField(required: true)]
    public int SoulsRequired;

    [DataField(required: true)]
    public List<EntProtoId> Actions = new();
}
