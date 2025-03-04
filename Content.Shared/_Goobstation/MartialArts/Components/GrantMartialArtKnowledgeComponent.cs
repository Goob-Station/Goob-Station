using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.MartialArts.Components;

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
}

[RegisterComponent]
public sealed partial class GrantCorporateJudoComponent  : GrantMartialArtKnowledgeComponent
{
    public override MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.CorporateJudo;
}

[RegisterComponent]
public sealed partial class GrantSleepingCarpComponent  : GrantMartialArtKnowledgeComponent
{
    public override MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.SleepingCarp;

    public int Stage = 1;

    public TimeSpan UseAgainTime = TimeSpan.Zero;

    public readonly int MaxUseDelay = 2;

    public readonly int MinUseDelay = 1;
}
