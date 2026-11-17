using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Components;

[Flags]
public enum MartialArtsBlockType
{
    Equipped
}

[DataDefinition]
public sealed partial class MartialArtsBlockerEntry
{
    [DataField(required: true)]
    public List<ProtoId<MartialArtPrototype>> BlockedArts = [];

    [DataField(required: true)]
    public MartialArtsBlockType BlockType;
}

[RegisterComponent]
public sealed partial class MartialArtBlockerComponent : Component
{
    [DataField]
    public List<MartialArtsBlockerEntry> Entries = [];
}

[RegisterComponent]
public sealed partial class MartialArtBlockedComponent : Component
{
    [DataField]
    public ProtoId<MartialArtPrototype> Form;

    [DataField]
    public bool StaminaDmgBlock = false;
}