// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.NPC.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Werewolf.GameTicking.Rules;

[RegisterComponent, Access(typeof(WerewolfRuleSystem))]
public sealed partial class WerewolfRuleComponent : Component
{
    /// <summary>
    ///  The briefing sound that plays once an entity becomes the werewolf
    /// </summary>
    [DataField]
    public SoundPathSpecifier BriefingSound = new("/Audio/_Goobstation/Ambience/Antag/devil_start.ogg"); // todo: replace

    /// <summary>
    ///  The faction the werewolf resides in
    /// </summary>
    [ValidatePrototypeId<NpcFactionPrototype>, DataField]
    public string WerewolfFaction = "WerewolfFaction";

    /// <summary>
    ///  The faction that will be removed from the antagonist
    /// </summary>
    [ValidatePrototypeId<NpcFactionPrototype>, DataField]
    public string NanotrasenFaction = "NanoTrasen";

    /// <summary>
    ///  The mindrole of the antagonist
    /// </summary>
    [DataField]
    public EntProtoId WerewolfMindRole = "MindRoleWerewolf";
}
