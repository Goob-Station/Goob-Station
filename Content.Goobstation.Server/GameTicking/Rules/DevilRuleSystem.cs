// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;
using Content.Goobstation.Server.Devil;
using Content.Goobstation.Server.Devil.Roles;
using Content.Goobstation.Shared;
using Content.Goobstation.Shared.Devil;
using Content.Server.Antag;
using Content.Server.Bible.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.GameTicking.Rules;

public sealed class DevilRuleSystem : GameRuleSystem<DevilRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly ObjectivesSystem _objective = default!;

    private readonly SoundSpecifier _briefingSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/devil_start.ogg");

    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string DevilFaction = "DevilFaction";

    private readonly EntProtoId _devilMindRole = "DevilMindRole";

    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string NanotrasenFaction = "NanoTrasen";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevilRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
        SubscribeLocalEvent<DevilRuleComponent, ObjectivesTextPrependEvent>(OnTextPrepend);
    }

    private void OnSelectAntag(EntityUid uid, DevilRuleComponent comp, ref AfterAntagEntitySelectedEvent args)
    {
        MakeDevil(args.EntityUid, comp);
    }

    private bool MakeDevil(EntityUid target, DevilRuleComponent rule)
    {
        if (!_mind.TryGetMind(target, out var mindId, out var mind))
            return false;

        _role.MindAddRole(mindId, _devilMindRole.Id, mind, true);

        var devilComp = EnsureComp<DevilComponent>(target);

        var meta = MetaData(target);

        var briefing = Loc.GetString("devil-role-greeting", ("trueName", devilComp.TrueName), ("playerName", meta.EntityName));

        _antag.SendBriefing(target, briefing, Color.DarkRed, _briefingSound);

        if (_role.MindHasRole<DevilRoleComponent>(mindId, out var mr))
            AddComp(mr.Value, new RoleBriefingComponent { Briefing = briefing }, overwrite: true);

        _npcFaction.RemoveFaction(target, NanotrasenFaction);
        _npcFaction.AddFaction(target, DevilFaction);

        return true;
    }

    private void OnTextPrepend(EntityUid uid, DevilRuleComponent comp, ref ObjectivesTextPrependEvent args)
    {
        var mostContractsName = string.Empty;
        var mostContracts = 0f;

        foreach (var devil in EntityQuery<DevilComponent>())
        {
            if (!_mind.TryGetMind(devil.Owner, out var mindId, out var mind))
                continue;

            var metaData = MetaData(devil.Owner);

            if (devil.Souls > mostContracts)
            {
                mostContracts = devil.Souls;
                mostContractsName = _objective.GetTitle((mindId, mind), metaData.EntityName);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString($"roundend-prepend-devil-contracts{(!string.IsNullOrWhiteSpace(mostContractsName) ? "-named" : "")}", ("name", mostContractsName), ("number", mostContracts)));

        args.Text = sb.ToString();
    }
}
