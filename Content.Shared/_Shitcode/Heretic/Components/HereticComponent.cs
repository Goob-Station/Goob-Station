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

using Content.Shared.Dataset;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Heretic;

// TODO: Move all of this to mind components, heretics should be safely polymorphable
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HereticComponent : Component
{
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
    public EntityUid MansusGrasp = EntityUid.Invalid;

    [DataField]
    public List<EntityUid> OurBlades = new();

    public int MaxBlades => CurrentPath switch
    {
        "Blade" => 4,
        _ => 2,
    };
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
