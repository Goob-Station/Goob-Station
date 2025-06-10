// SPDX-FileCopyrightText: 2025 IrisTheAmped <iristheamped@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Mercenary;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.Antag;
using Content.Server.Ghost.Roles.Components;

namespace Content.Goobstation.Server.Mercenary;

public sealed partial class MercenarySystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    private ISawmill _sawmill = null!;
    public override void Initialize()
    {
        base.Initialize();

        InitializeOrder();

        SubscribeLocalEvent<MercenaryRequesterComponent, TakeGhostRoleEvent>(OnPlayerAttached);

        _sawmill = Logger.GetSawmill("mercenary-system");
    }

    private void OnPlayerAttached(Entity<MercenaryRequesterComponent> merc, ref TakeGhostRoleEvent args)
    {
        if (merc.Comp.Requester is not { } requester
            || TerminatingOrDeleted(requester))
        {
            _sawmill.Error($"Could not resolve requester entity : {merc.Comp.Requester}");
            return;
        }

        AddMercenaryRole(merc, requester);
    }

    private void AddMercenaryRole(Entity<MercenaryRequesterComponent> merc, EntityUid requester)
    {
        if (!_mind.TryGetMind(merc, out var mindId, out var mind))
            return;

        _roles.MindAddRole(mindId, merc.Comp.MindRole.Id);

        var briefing = Loc.GetString("mercenary-briefing", ("employer", Name(requester) ));

        if (_roles.MindHasRole<MercenaryRoleComponent>(mindId, out var mindrole))
            AddComp(mindrole.Value, new RoleBriefingComponent { Briefing = briefing }, true);

        if (mind.Session == null
            || merc.Comp.BriefingSent)
            return;

        _antag.SendBriefing(mind.Session, briefing, Color.Red, null);
        merc.Comp.BriefingSent = true;
    }
}
