using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MartialArts;

[RegisterComponent]
public sealed partial class GrantCqcComponent : GrantMartialArtKnowledgeComponent
{

}
public sealed partial class GrantCorporateJudo : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public ProtoId<ComboListPrototype> RoundstartCombos = "CorporateJudo";
}

[RegisterComponent]
public sealed partial class MartialArtsKnowledgeComponent : GrabStagesOverrideComponent
{
    [DataField]
    public MartialArtsForms MartialArtsForm = MartialArtsForms.CloseQuartersCombat;

    [DataField]
    public ProtoId<ComboListPrototype> RoundstartCombos = "CQCMoves";

    [DataField]
    public bool Blocked = true;
}

public enum MartialArtsForms
{
    CorporateJudo,
    CloseQuartersCombat,
}
