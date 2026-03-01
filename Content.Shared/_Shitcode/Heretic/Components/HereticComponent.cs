// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 yglop <95057024+yglop@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Heretic;
using Content.Shared.Dataset;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Objectives.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Heretic;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HereticComponent : Component
{
    public override bool SessionSpecific => true;

    [DataField]
    public List<ProtoId<HereticKnowledgePrototype>> BaseKnowledge = new()
    {
        "BreakOfDawn",
        "HeartbeatOfMansus",
        "AmberFocus",
        "LivingHeart",
        "CodexCicatrix",
        "CloakOfShadow",
        "Reminiscence",
        "FeastOfOwls",
    };

    [DataField, AutoNetworkedField]
    public List<ProtoId<HereticRitualPrototype>> KnownRituals = new();

    [DataField]
    public ProtoId<HereticRitualPrototype>? ChosenRitual;

    /// <summary>
    ///     Contains the list of targets that are eligible for sacrifice.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<SacrificeTargetData> SacrificeTargets = new();

    /// <summary>
    ///     How much targets can a heretic have?
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxTargets = 6;

    // hardcoded paths because i hate it
    // "Ash", "Lock", "Flesh", "Void", "Blade", "Rust"
    /// <summary>
    ///     Indicates a path the heretic is on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? CurrentPath;

    /// <summary>
    ///     Indicates a stage of a path the heretic is on. 0 is no path, 10 is ascension
    /// </summary>
    [DataField, AutoNetworkedField]
    public int PathStage;

    [DataField, AutoNetworkedField]
    public bool Ascended;

    [DataField, AutoNetworkedField]
    public bool CanAscend = true;

    [DataField]
    public ProtoId<DatasetPrototype> KnowledgeDataset = "EligibleTags";

    /// <summary>
    ///     Required tags for ritual of knowledge
    /// </summary>
    [DataField(serverOnly: true), NonSerialized]
    public HashSet<ProtoId<TagPrototype>> KnowledgeRequiredTags = new();

    /// <summary>
    ///     Used to prevent double casting mansus grasp.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid MansusGraspAction = EntityUid.Invalid;

    [DataField]
    public Dictionary<ProtoId<HereticRitualPrototype>, List<EntityUid>> LimitedTransmutations = new();

    [DataField]
    public SoundSpecifier? InfluenceGainSound = new SoundCollectionSpecifier("bloodCrawl");

    [DataField]
    public LocId InfluenceGainBaseMessage = "influence-base-message";

    [DataField]
    public int InfluenceGainTextFontSize = 22;

    [DataField]
    public List<LocId> InfluenceGainMessages = new()
    {
        "influence-gain-message-1",
        "influence-gain-message-2",
        "influence-gain-message-3",
        "influence-gain-message-4",
        "influence-gain-message-5",
        "influence-gain-message-6",
        "influence-gain-message-7",
        "influence-gain-message-7",
        "influence-gain-message-8",
        "influence-gain-message-9",
        "influence-gain-message-10",
        "influence-gain-message-11",
        "influence-gain-message-12",
        "influence-gain-message-13",
        "influence-gain-message-14",
        "influence-gain-message-15",
        "influence-gain-message-16",
    };

    [DataField]
    public List<EntProtoId<ObjectiveComponent>> AllObjectives = new()
    {
        "HereticKnowledgeObjective",
        "HereticSacrificeObjective",
        "HereticSacrificeHeadObjective",
    };

    /// <summary>
    /// Events raised when on new body when mind gets transferred to it
    /// </summary>
    [DataField, NonSerialized]
    public List<HereticKnowledgeEvent> KnowledgeEvents = new();

    /// <summary>
    /// Minions summoned by this heretic
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Minions = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class SacrificeTargetData
{
    [DataField]
    public NetEntity Entity;

    [DataField]
    public HumanoidCharacterProfile Profile;

    [DataField]
    public ProtoId<JobPrototype> Job;
}

[Serializable, NetSerializable]
public enum InfusedBladeVisuals
{
    Infused,
}
