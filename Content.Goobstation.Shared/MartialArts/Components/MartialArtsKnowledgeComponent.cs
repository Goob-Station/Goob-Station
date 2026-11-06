using Content.Goobstation.Shared.Zones;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Components;

[DataDefinition, Serializable]
public sealed partial class MartialArtEntryLimitations
{
    public List<ProtoId<ZonePrototype>> RestrictedZones;

    public MartialArtEntryLimitations(List<ProtoId<ZonePrototype>> restrictedZones)
    {
        RestrictedZones = restrictedZones;
    }
}

[DataDefinition, Serializable]
public sealed partial class MartialArtKnowledgeEntry
{
    public ProtoId<MartialArtPrototype> MartialArtId;
    public MartialArtEntryLimitations Limitations;

    public MartialArtKnowledgeEntry(ProtoId<MartialArtPrototype> martialArtId, MartialArtEntryLimitations limitations)
    {
        MartialArtId = martialArtId;
        Limitations = limitations;
    }
}

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MartialArtsKnowledgeComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public List<MartialArtKnowledgeEntry> Knowledge = [];
}
