// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Zones;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Components;

[Serializable, NetSerializable]
[Flags]
public enum MartialArtsGrantType
{
    OnlyMapInit,
    Use,
    Clothing
}

public abstract partial class GrantMartialArtKnowledgeComponent : Component
{
    [DataField]
    public virtual ProtoId<MartialArtPrototype> MartialArtsForm { get; set; }

    [DataField]
    public virtual LocId? LearnMessage { get; set; } = null;

    [DataField]
    public MartialArtsGrantType GrantType { get; set; } = MartialArtsGrantType.OnlyMapInit;

    [DataField]
    public bool MultiUse = false;

    [DataField]
    public string? SpawnedProto = "Ash";

    [DataField]
    public SoundSpecifier? SoundOnUse = new SoundPathSpecifier("/Audio/Effects/fire.ogg", AudioParams.Default.WithVolume(10));

    [DataField]
    public virtual List<ProtoId<ZonePrototype>> RestrictedZones { get; set; } = [];
}

[RegisterComponent]
public sealed partial class GrantCqcComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "CloseQuartersCombat";

    [DataField]
    public override LocId? LearnMessage { get; set; } = "cqc-success-learned";
}

[RegisterComponent]
public sealed partial class GrantCorporateJudoComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "CorporateJudo";
}

[RegisterComponent]
public sealed partial class GrantCapoeiraComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "Capoeira";

    [DataField]
    public override LocId? LearnMessage { get; set; } = "capoeira-success-learned";
}

[RegisterComponent]
public sealed partial class GrantKungFuDragonComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "KungFuDragon";

    [DataField]
    public override LocId? LearnMessage { get; set; } = "dragon-success-learned";
}

[RegisterComponent]
public sealed partial class GrantNinjutsuComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "Ninjutsu";

    [DataField]
    public override LocId? LearnMessage { get; set; } = "ninjutsu-success-learned";
}

[RegisterComponent]
public sealed partial class GrantSleepingCarpComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "SleepingCarp";

    [DataField]
    public int MaximumMasters = 1;

    [ViewVariables]
    public int CurrentMasters = 0;
}

[RegisterComponent]
public sealed partial class SleepingCarpStudentComponent : Component
{
    [DataField]
    public int LearnStage = 3;

    [ViewVariables]
    public int CurrentStage = 0;

    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(30);

    [ViewVariables]
    public TimeSpan UseAgainTime = TimeSpan.Zero;
}

[RegisterComponent]
public sealed partial class GrantHellRipComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "HellRip";

    [DataField]
    public override LocId? LearnMessage { get; set; } = "hellrip-success-learned";
}

[RegisterComponent]
public sealed partial class GrantBearFuComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "BearFu";

    [DataField]
    public override LocId? LearnMessage { get; set; } = "bearfu-success-learned";
}

[RegisterComponent]
public sealed partial class GrantPlasmaFistComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "PlasmaFist";

    [DataField]
    public override LocId? LearnMessage { get; set; } = "plasmafist-success-learned";
}

[RegisterComponent]
public sealed partial class GrantMimejutsuComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "Mimejutsu";

    [DataField]
    public override LocId? LearnMessage { get; set; } = "mimejutsu-success-learned";
}

[RegisterComponent]
public sealed partial class GrantAdminFuComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override ProtoId<MartialArtPrototype> MartialArtsForm { get; set; } = "AdminFu";

    [DataField]
    public override LocId? LearnMessage { get; set; } = "adminfu-success-learned";
}

