// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.NPC.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Devil.GameTicking.Rules;

[RegisterComponent, Access(typeof(DevilRuleSystem))]
public sealed partial class DevilRuleComponent : Component
{
    [DataField]
    public SoundPathSpecifier BriefingSound = new("/Audio/_Goobstation/Ambience/Antag/devil_start.ogg");

    [DataField]
    public ProtoId<NpcFactionPrototype> DevilFaction = "DevilFaction";

    [DataField]
    public ProtoId<NpcFactionPrototype> NanotrasenFaction = "NanoTrasen";

    [DataField]
    public EntProtoId DevilMindRole = "DevilMindRole";
}
