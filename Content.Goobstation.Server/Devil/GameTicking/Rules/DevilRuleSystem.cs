// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;
using Content.Goobstation.Server.Devil.Roles;
using Content.Goobstation.Shared.Devil;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Devil.GameTicking.Rules;

public sealed class DevilRuleSystem : GameRuleSystem<DevilRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly ObjectivesSystem _objective = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevilRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        SubscribeLocalEvent<DevilRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
        SubscribeLocalEvent<DevilRoleComponent, GetBriefingEvent>(OnGetBrief);
    }

    private void OnSelectAntag(EntityUid uid, DevilRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeDevil(args.EntityUid, comp);
    }

    private bool MakeDevil(EntityUid target, DevilRuleComponent rule)
    {
        var devilComp = EnsureComp<DevilComponent>(target);

        var briefing = Loc.GetString("devil-role-greeting", ("trueName", devilComp.TrueName), ("playerName", Name(target)));
        _antag.SendBriefing(target, briefing, Color.DarkRed, rule.BriefingSound);

        _npcFaction.RemoveFaction(target, rule.NanotrasenFaction);
        _npcFaction.AddFaction(target, rule.DevilFaction);

        return true;
    }

    private void OnGetBrief(Entity<DevilRoleComponent> role, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;

        if (ent is null)
            return;

        args.Append(MakeBriefing(ent.Value));
    }

    private string MakeBriefing(EntityUid ent)
    {
        return !TryComp<DevilComponent>(ent, out var devilComp)
            ? null!
            : Loc.GetString("devil-role-greeting", ("trueName", devilComp.TrueName), ("playerName", Name(ent)));
    }

    private void OnTextPrepend(EntityUid uid, DevilRuleComponent comp, ref ObjectivesTextPrependEvent args)

    {
        var mostContractsName = string.Empty;
        var mostContracts = 0f;

        var query = EntityQueryEnumerator<DevilComponent>();
        while (query.MoveNext(out var devil, out var devilComp))
        {
            if (!_mind.TryGetMind(devil, out var mindId, out var mind))
                continue;

            var metaData = MetaData(devil);
            if (devilComp.Souls < mostContracts)
                continue;

            mostContracts = devilComp.Souls;
            mostContractsName = _objective.GetTitle((mindId, mind), metaData.EntityName);
        }
        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString($"roundend-prepend-devil-contracts{(!string.IsNullOrWhiteSpace(mostContractsName) ? "-named" : "")}", ("name", mostContractsName), ("number", mostContracts)));
        args.Text = sb.ToString();
    }
}
