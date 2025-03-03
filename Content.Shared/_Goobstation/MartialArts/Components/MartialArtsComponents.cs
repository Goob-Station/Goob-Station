using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MartialArts.Components;

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
public sealed partial class GrantSleepingCarpComponent  : GrantMartialArtKnowledgeComponent
{
    public const MartialArtsForms MartialArtsForm = MartialArtsForms.SleepingCarp;

    public int Stage = 1;

    public TimeSpan UseAgainTime = TimeSpan.Zero;

    public readonly int MaxUseDelay = 2;

    public readonly int MinUseDelay = 1;
}

[RegisterComponent]
public sealed partial class GrantKravMagaComponent : Component
{

}


[RegisterComponent]
public sealed partial class MartialArtBlockedComponent  : Component
{
    [DataField]
    public MartialArtsForms Form;
}

[RegisterComponent]
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
