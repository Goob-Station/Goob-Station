using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic;

[Prototype("hereticKnowledge")]
public sealed partial class HereticKnowledgePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public object? Event;
    [DataField] public HereticRitualPrototype? RitualPrototype;
    [DataField] public EntProtoId? ActionPrototype;
}

[Prototype("hereticRitual")]
public sealed partial class HereticRitualPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public string Name = "Unknown Ritual";
    [DataField] public Dictionary<EntProtoId, int>? Requirements;
    [DataField] public Dictionary<EntProtoId, int>? Output;
    [DataField] public object? OutputEvent;
}
