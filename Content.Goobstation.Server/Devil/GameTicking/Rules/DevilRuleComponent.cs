// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
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

    [ValidatePrototypeId<NpcFactionPrototype>, DataField]
    public string DevilFaction = "DevilFaction";

    [ValidatePrototypeId<NpcFactionPrototype>, DataField]
    public string NanotrasenFaction = "NanoTrasen";

    [DataField]
    public EntProtoId DevilMindRole = "DevilMindRole";
}
