using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MartialArts;

[RegisterComponent]
public sealed partial class GrantCqcComponent : GrantMartialArtKnowledgeComponent
{
    public MartialArtsForms MartialArtsForm = MartialArtsForms.CloseQuartersCombat;
}

[RegisterComponent]
public sealed partial class GrantCorporateJudoComponent  : GrantMartialArtKnowledgeComponent
{
    public MartialArtsForms MartialArtsForm = MartialArtsForms.CorporateJudo;
}

[RegisterComponent]
public sealed partial class MartialArtsKnowledgeComponent : GrabStagesOverrideComponent
{
    [DataField]
    public MartialArtsForms MartialArtsForm = MartialArtsForms.CloseQuartersCombat;

    [DataField]
    public FixedPoint2 DamageModifier = 2.0f;

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
