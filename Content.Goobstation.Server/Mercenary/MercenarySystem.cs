// SPDX-FileCopyrightText: 2025 IrisTheAmped <iristheamped@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Mercenary;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Server.Roles;
using Robust.Shared.Player;
using Content.Server.Antag;

namespace Content.Goobstation.Server.Mercenary;

public sealed partial class MercenarySystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    private ISawmill _sawmill = null!;
    public override void Initialize()
    {
        base.Initialize();

        InitializeOrder();

        SubscribeLocalEvent<MercenaryRequesterComponent, PlayerAttachedEvent>(OnPlayerAttached);

        _sawmill = Logger.GetSawmill("mercenary-system");
    }

    private void OnPlayerAttached(EntityUid uid, MercenaryRequesterComponent comp, PlayerAttachedEvent args)
    {
        if (comp.Requester is not { } requester || !TerminatingOrDeleted(requester))
        {
            _sawmill.Error($"Could not resolve requester entity : {comp.Requester}");
            return;
        }

        AddMercenaryRole(uid, requester, comp);
    }

    private void AddMercenaryRole(EntityUid merc, EntityUid requester, MercenaryRequesterComponent comp) //This is like mostly mind control code, thank whoever the fuck invented it.
    {
        if (!_mind.TryGetMind(merc, out var mindId, out var mind))
            return;

        var metadata = MetaData(requester);
        _roles.MindAddRole(mindId, comp.MindRole.Id);

        if (_roles.MindHasRole<MercenaryRoleComponent>(mindId, out var mindrole))
            AddComp(mindrole.Value, new RoleBriefingComponent { Briefing = MakeBriefing(requester) }, true);

        if (mind.Session == null
            || comp.BriefingSent)
            return;

        var popup = Loc.GetString("mercenary-briefing");
        var briefing = Loc.GetString("mercenary-briefing", ("employer", (MetaData(requester).EntityName)));

        _popup.PopupEntity(popup, merc, PopupType.LargeCaution);
        _antag.SendBriefing(mind.Session, briefing, Color.Red, comp.MercenaryStartSound);
        comp.BriefingSent = true;
    }

    private string MakeBriefing(EntityUid requester)
    {
        var metadata = MetaData(requester);
        var briefing = Loc.GetString("mercenary-briefing", ("employer", (MetaData(requester).EntityName)));

        return briefing + "\n " + Loc.GetString("mercenary-briefing", ("employer", metadata.EntityName)) + "\n";
    }
}
