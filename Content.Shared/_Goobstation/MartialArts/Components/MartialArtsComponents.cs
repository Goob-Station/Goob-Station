using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MartialArts.Components;

[RegisterComponent]
public sealed partial class MartialArtBlockedComponent : Component
{
    [DataField]
    public MartialArtsForms Form;
}

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MartialArtsKnowledgeComponent : GrabStagesOverrideComponent
{
    [DataField]
    [AutoNetworkedField]
    public MartialArtsForms MartialArtsForm = MartialArtsForms.CloseQuartersCombat;

    [DataField]
    [AutoNetworkedField]
    public int MinRandomDamageModifier;

    [DataField]
    [AutoNetworkedField]
    public int MaxRandomDamageModifier = 5;

    [DataField]
    [AutoNetworkedField]
    public FixedPoint2 BaseDamageModifier;

    [DataField]
    [AutoNetworkedField]
    public bool RandomDamageModifier;

    [DataField]
    [AutoNetworkedField]
    public ProtoId<ComboListPrototype> RoundstartCombos = "CQCMoves";

    [DataField]
    [AutoNetworkedField]
    public bool Blocked = false;

    [DataField]
    [AutoNetworkedField]
    public List<LocId> RandomSayings = [];

    [DataField]
    [AutoNetworkedField]
    public List<LocId> RandomSayingsDowned = [];
}

public enum MartialArtsForms
{
    CorporateJudo,
    CloseQuartersCombat,
    SleepingCarp,
}
