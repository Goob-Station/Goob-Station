using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MartialArts.Components;

[RegisterComponent]
public sealed partial class MartialArtBlockedComponent  : Component
{
    [DataField]
    public MartialArtsForms Form;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class MartialArtsKnowledgeComponent : GrabStagesOverrideComponent
{
    [DataField]
    public MartialArtsForms MartialArtsForm = MartialArtsForms.SleepingCarp;

    [DataField]
    public FixedPoint2 MinDamageModifier = 2.0;

    [DataField]
    public FixedPoint2 MaxDamageModifier = 3.0;

    [DataField]
    public bool RandomDamageModifier;

    [DataField]
    public bool HarmAsStamina;

    [DataField]
    public ProtoId<ComboListPrototype> RoundstartCombos = "CQCMoves";

    [DataField]
    public bool Blocked = true;

    [DataField]
    public List<LocId> RandomSayings = [];

    [DataField]
    public List<LocId> RandomSayingsDowned = [];
}

public enum MartialArtsForms
{
    CorporateJudo,
    CloseQuartersCombat,
    SleepingCarp,
}
