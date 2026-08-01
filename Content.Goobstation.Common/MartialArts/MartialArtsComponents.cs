// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Grab;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.MartialArts;

[RegisterComponent]
public sealed partial class MartialArtBlockedComponent : Component
{
    [DataField]
    public MartialArtsForms Form;
}
public abstract partial class GrabStagesOverrideComponent : Component
{
    public readonly GrabStage StartingStage = GrabStage.Soft;
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
    public bool Blocked;

    [DataField]
    [AutoNetworkedField]
    public float OriginalFistDamage;

    [DataField]
    [AutoNetworkedField]
    public string OriginalFistDamageType;

}

public enum MartialArtsForms
{
    CorporateJudo,
    CloseQuartersCombat,
    SleepingCarp,
    Capoeira,
    KungFuDragon,
    Ninjutsu,
    HellRip,
}
