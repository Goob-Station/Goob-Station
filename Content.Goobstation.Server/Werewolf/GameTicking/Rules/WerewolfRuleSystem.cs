// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Werewolf.Roles;
using Content.Goobstation.Shared.Werewolf.Components;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.NPC.Systems;

namespace Content.Goobstation.Server.Werewolf.GameTicking.Rules;

public sealed class WerewolfRuleSystem : GameRuleSystem<WerewolfRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WerewolfRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        SubscribeLocalEvent<WerewolfRoleComponent, GetBriefingEvent>(OnGetBrief);
    }

    private void OnSelectAntag(Entity<WerewolfRuleComponent> ent, ref AfterAntagEntitySelectedEvent args) =>
        MakeWerewolf(args.EntityUid, ent.Comp);

    private void OnGetBrief(Entity<WerewolfRoleComponent> ent, ref GetBriefingEvent args) =>
        args.Append(Loc.GetString("werewolf-role-greeting"));

    /// <summary>
    /// Handles making the entity into the werewolf antagonist
    /// </summary>
    /// <param name="target"></param> The entity being made the werewolf
    /// <param name="rule"></param> The gamerule being added
    /// <returns></returns>
    private bool MakeWerewolf(EntityUid target, WerewolfRuleComponent rule)
    {
        if (HasComp<SiliconComponent>(target))
            return false;

        EnsureComp<WerewolfComponent>(target);

        var briefing = Loc.GetString("werewolf-role-greeting");
        _antag.SendBriefing(target, briefing, Color.MediumPurple, rule.BriefingSound);

        _npcFaction.RemoveFaction(target, rule.NanotrasenFaction);
        _npcFaction.AddFaction(target, rule.WerewolfFaction);

        return true;
    }
}
