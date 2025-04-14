// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 TekuNut <13456422+TekuNut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 2013HORSEMEATSCANDAL <146540817+2013HORSEMEATSCANDAL@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 OctoRocket <88291550+OctoRocket@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Robert Hardy <robertblake.hardy@outlook.com>
// SPDX-FileCopyrightText: 2023 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vyacheslav Titov <rincew1nd@ya.ru>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 csqrb <56765288+CaptainSqrBeard@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 lzk228 <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Титов Вячеслав Витальевич <rincew1nd@yandex.ru>
// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rainfall <rainfey0+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.RoundEnd;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(NukeopsRuleSystem))]
public sealed partial class NukeopsRuleComponent : Component
{
    /// <summary>
    /// What will happen if all of the nuclear operatives will die. Used by LoneOpsSpawn event.
    /// </summary>
    [DataField]
    public RoundEndBehavior RoundEndBehavior = RoundEndBehavior.ShuttleCall;

    /// <summary>
    /// Text for shuttle call if RoundEndBehavior is ShuttleCall.
    /// </summary>
    [DataField]
    public string RoundEndTextSender = "comms-console-announcement-title-centcom";

    /// <summary>
    /// Text for shuttle call if RoundEndBehavior is ShuttleCall.
    /// </summary>
    [DataField]
    public string RoundEndTextShuttleCall = "nuke-ops-no-more-threat-announcement-shuttle-call";

    /// <summary>
    /// Text for announcement if RoundEndBehavior is ShuttleCall. Used if shuttle is already called
    /// </summary>
    [DataField]
    public string RoundEndTextAnnouncement = "nuke-ops-no-more-threat-announcement";

    /// <summary>
    /// Time to emergency shuttle to arrive if RoundEndBehavior is ShuttleCall.
    /// </summary>
    [DataField]
    public TimeSpan EvacShuttleTime = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Whether or not nukie left their outpost
    /// </summary>
    [DataField]
    public bool LeftOutpost;

    /// <summary>
    ///     Enables opportunity to get extra TC for war declaration
    /// </summary>
    [DataField]
    public bool CanEnableWarOps = true;

    /// <summary>
    ///     Indicates time when war has been declared, null if not declared
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? WarDeclaredTime;

    /// <summary>
    ///     This amount of TC will be given to each nukie
    /// </summary>
    [DataField]
    public int WarTcAmountPerNukie = 200;

    // Goobstation start
    /// <summary>
    /// The ratio of players per nuclear operative for war declaration scaling.
    /// Example: A value of 10 means one operative per 10 players.
    /// </summary>
    [DataField]
    public int WarNukiePlayerRatio = 12;

    /// <summary>
    /// Additional telecrystals granted per player on the server during war.
    /// Total bonus is divided by number of operatives.
    /// </summary>
    [DataField]
    public int WarTcPerPlayer = 20;

    /// <summary>
    /// Compensation telecrystals granted per missing nuclear operative.
    /// Total bonus is divided by number of operatives.
    /// </summary>
    [DataField]
    public int WarTcPerNukieMissing = 100;
    // Goobstation end

    /// <summary>
    ///     Delay between war declaration and nuke ops arrival on station map. Gives crew time to prepare
    /// </summary>
    [DataField]
    public TimeSpan WarNukieArriveDelay = TimeSpan.FromMinutes(15);

    /// <summary>
    ///     Time crew can't call emergency shuttle after war declaration.
    /// </summary>
    [DataField]
    public TimeSpan WarEvacShuttleDisabled = TimeSpan.FromMinutes(25);

    /// <summary>
    ///     Minimal operatives count for war declaration
    /// </summary>
    [DataField]
    public int WarDeclarationMinOps = 2;

    [DataField]
    public WinType WinType = WinType.Neutral;

    [DataField]
    public List<WinCondition> WinConditions = new();

    [DataField]
    public EntityUid? TargetStation;

    [DataField]
    public ProtoId<NpcFactionPrototype> Faction = "Syndicate";

    /// <summary>
    ///     Path to antagonist alert sound.
    /// </summary>
    [DataField]
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/Ambience/Antag/nukeops_start.ogg");

    // Goobstation - Honkops
    [DataField]
    public string LocalePrefix = "nukeops-";
}

public enum WinType : byte
{
    /// <summary>
    ///     Operative major win. This means they nuked the station.
    /// </summary>
    OpsMajor,
    /// <summary>
    ///     Minor win. All nukies were alive at the end of the round.
    ///     Alternatively, some nukies were alive, but the disk was left behind.
    /// </summary>
    OpsMinor,
    /// <summary>
    ///     Neutral win. The nuke exploded, but on the wrong station.
    /// </summary>
    Neutral,
    /// <summary>
    ///     Crew minor win. The nuclear authentication disk escaped on the shuttle,
    ///     but some nukies were alive.
    /// </summary>
    CrewMinor,
    /// <summary>
    ///     Crew major win. This means they either killed all nukies,
    ///     or the bomb exploded too far away from the station, or on the nukie moon.
    /// </summary>
    CrewMajor
}

public enum WinCondition : byte
{
    NukeExplodedOnCorrectStation,
    NukeExplodedOnNukieOutpost,
    NukeExplodedOnIncorrectLocation,
    NukeActiveInStation,
    NukeActiveAtCentCom,
    NukeDiskOnCentCom,
    NukeDiskNotOnCentCom,
    NukiesAbandoned,
    AllNukiesDead,
    SomeNukiesAlive,
    AllNukiesAlive
}