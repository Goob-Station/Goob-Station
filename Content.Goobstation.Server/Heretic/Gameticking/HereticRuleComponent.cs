// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking.Rules;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Store;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Heretic.GameTicking;

[RegisterComponent, Access(typeof(HereticRuleSystem))]
public sealed partial class HereticRuleComponent : Component
{
    [DataField]
    public MinMax RealityShiftPerHeretic = new(3, 4);

    [DataField]
    public MinMax StartingPoints = new(2, 3);

    [DataField]
    public EntProtoId RealityShift = "EldritchInfluence";

    [DataField]
    public ProtoId<NpcFactionPrototype> HereticFactionId = "Heretic";

    [DataField]
    public ProtoId<NpcFactionPrototype> NanotrasenFactionId = "NanoTrasen";

    [DataField]
    public ProtoId<CurrencyPrototype> Currency = "KnowledgePoint";

    [DataField]
    public EntProtoId MindRole = "MindRoleHeretic";

    [DataField]
    public SoundSpecifier BriefingSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/heretic_gain.ogg");

    [ViewVariables(VVAccess.ReadOnly)]
    public readonly List<EntityUid> Minds = [];

    [ViewVariables(VVAccess.ReadOnly)]
    public readonly List<ProtoId<StoreCategoryPrototype>> StoreCategories =
    [
        "HereticPathAsh",
        //"HereticPathLock",
        "HereticPathFlesh",
        "HereticPathBlade",
        "HereticPathVoid",
        "HereticPathRust",
        "HereticPathSide",
    ];
}
