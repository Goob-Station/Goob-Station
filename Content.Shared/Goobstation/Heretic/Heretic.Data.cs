using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Heretic;

[Prototype("hereticKnowledge")]
public sealed partial class HereticKnowledgePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public object? Event;
    [DataField] public List<ProtoId<HereticRitualPrototype>>? RitualPrototypes;
    [DataField] public List<EntProtoId>? ActionPrototypes;
}

[Prototype("hereticRitual")]
public sealed partial class HereticRitualPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public string Name = "heretic-ritual-unknown";
    [DataField] public Dictionary<string, int>? RequiredEntityNames;
    [DataField] public Dictionary<ProtoId<TagPrototype>, int>? RequiredTags;
    [DataField] public Dictionary<EntProtoId, int>? Output;
    [DataField] public object? OutputEvent;
    [DataField] public ProtoId<HereticKnowledgePrototype>? OutputKnowledge;
}

#region Ritual Events

[Serializable, NetSerializable] public sealed partial class HereticRitualSacrificeEvent : EntityEventArgs { }
[Serializable, NetSerializable] public sealed partial class HereticRitualAscendEvent : EntityEventArgs { }

#endregion
