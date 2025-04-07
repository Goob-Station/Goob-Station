using Content.Goobstation.Common.MartialArts;

namespace Content.Goobstation.Shared.MartialArts.Components;

public abstract partial class GrantMartialArtKnowledgeComponent : Component
{
    [DataField]
    public bool Used;

    [DataField]
    public virtual MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.CloseQuartersCombat;
}

[RegisterComponent]
public sealed partial class GrantCqcComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public bool IsBlocked;
}

[RegisterComponent]
public sealed partial class GrantCorporateJudoComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.CorporateJudo;
}

[RegisterComponent]
public sealed partial class GrantSleepingCarpComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.SleepingCarp;
    [DataField]
    public int MaximumUses = 1;
    public int CurrentUses = 0;
}

[RegisterComponent]
public sealed partial class SleepingCarpStudentComponent : Component
{
    [DataField]
    public int Stage = 1;

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan UseAgainTime = TimeSpan.Zero;

    [DataField]
    public int MaxUseDelay = 90;

    [DataField]
    public int MinUseDelay = 30;
}

