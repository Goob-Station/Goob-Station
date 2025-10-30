// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Bokser815 <70928915+Bokser815@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.MartialArts.Components;

public abstract partial class GrantMartialArtKnowledgeComponent : Component
{
    [DataField]
    public virtual MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.CloseQuartersCombat;

    [DataField]
    public virtual LocId? LearnMessage { get; set; } = null;

    [DataField]
    public bool MultiUse;

    [DataField]
    public string? SpawnedProto = "Ash";

    [DataField]
    public SoundSpecifier? SoundOnUse = new SoundPathSpecifier("/Audio/Effects/fire.ogg", AudioParams.Default.WithVolume(10));
}

[RegisterComponent]
public sealed partial class GrantCqcComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public bool IsBlocked;

    public override LocId? LearnMessage { get; set; } = "cqc-success-learned";
}

[RegisterComponent]
public sealed partial class GrantCorporateJudoComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.CorporateJudo;
}

[RegisterComponent]
public sealed partial class GrantCapoeiraComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.Capoeira;

    public override LocId? LearnMessage { get; set; } = "capoeira-success-learned";
}

[RegisterComponent]
public sealed partial class GrantKungFuDragonComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.KungFuDragon;

    public override LocId? LearnMessage { get; set; } = "dragon-success-learned";
}

[RegisterComponent]
public sealed partial class GrantNinjutsuComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.Ninjutsu;

    public override LocId? LearnMessage { get; set; } = "ninjutsu-success-learned";
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

[RegisterComponent]
public sealed partial class GrantHellRipComponent : GrantMartialArtKnowledgeComponent
{
    [DataField]
    public override MartialArtsForms MartialArtsForm { get; set; } = MartialArtsForms.HellRip;

    public override LocId? LearnMessage { get; set; } = "hellrip-success-learned";
}
