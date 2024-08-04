using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Heretic;

[Prototype("hereticKnowledge")]
public sealed partial class HereticKnowledgePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public int Stage = 1;
    [DataField] public object? Event;
    [DataField] public List<ProtoId<HereticRitualPrototype>>? RitualPrototypes;
    [DataField] public List<EntProtoId>? ActionPrototypes;
}

[Prototype("hereticRitual")]
public sealed partial class HereticRitualPrototype : IPrototype, ICloneable
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public string Name = "heretic-ritual-unknown";
    [DataField] public Dictionary<string, int>? RequiredEntityNames;
    [DataField] public Dictionary<ProtoId<TagPrototype>, int>? RequiredTags;
    [DataField] public Dictionary<EntProtoId, int>? Output;
    [DataField] public object? OutputEvent;
    [DataField] public ProtoId<HereticKnowledgePrototype>? OutputKnowledge;

    // i have to do this because of the way how rituals are processed
    // also who the fuck passes prototypes by reference and not value?
    // man fuck those C# classes byref byval type shit i hate it and i want it to die in a fire
    // thank you for reading my yapping.
    public object Clone()
    {
        return new HereticRitualPrototype()
        {
            ID = this.ID,
            Name = this.Name,
            RequiredEntityNames = this.RequiredEntityNames,
            RequiredTags = this.RequiredTags,
            Output = this.Output,
            OutputEvent = this.OutputEvent,
            OutputKnowledge = this.OutputKnowledge
        };
    }
}

#region Ritual Events

[Serializable, NetSerializable] public sealed partial class HereticRitualSacrificeEvent : EntityEventArgs { }
[Serializable, NetSerializable] public sealed partial class HereticRitualAscendEvent : EntityEventArgs { }

#endregion

#region Shop Events

// will only work if listing id matches the knowledge id
[Serializable, NetSerializable] public sealed partial class HereticAddKnowledgeEvent : EntityEventArgs { }

#endregion
