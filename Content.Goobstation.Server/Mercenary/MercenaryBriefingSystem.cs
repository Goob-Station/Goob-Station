// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 IrisTheAmped <iristheamped@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Mercenary;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Server.Roles;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Server.Antag;

namespace Content.Goobstation.Server.Mercenary;

public sealed class MercenaryBriefingSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    [ValidatePrototypeId<EntityPrototype>] static EntProtoId mindRole = "MindRoleMercenary";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MercenaryRequesterComponent, PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerAttached(EntityUid uid, MercenaryRequesterComponent comp, PlayerAttachedEvent args)
    {
        if (comp.Requester == null || !TerminatingOrDeleted(comp.Requester.Value))
        {
            Log.Error($"No requester entity(Merc Shit)");
            return;
        }

        AddMercenaryRole(uid, comp.Requester.Value, comp);
    }

    private void AddMercenaryRole(EntityUid merc, EntityUid requester, MercenaryRequesterComponent comp) //This is like mostly mind control code, thank whoever the fuck invented it.
    {
        var metadata = MetaData(requester);

        if (!_mind.TryGetMind(merc, out var mindId, out var mind))
            return;

        _roles.MindAddRole(mindId, mindRole.Id);

        if (_roles.MindHasRole<MercenaryRoleComponent>(mindId, out var mr))
            AddComp(mr.Value, new RoleBriefingComponent { Briefing = MakeBriefing(requester) }, true);

        if (mind?.Session != null && !comp.BriefingSent)
        {
            _popup.PopupEntity(Loc.GetString("mercenary-briefing"), merc, PopupType.LargeCaution);
            _antag.SendBriefing(mind.Session, Loc.GetString("mercenary-briefing", ("employer", (MetaData(requester).EntityName))), Color.Red, comp.MindcontrolStartSound);
            comp.BriefingSent = true;
        }
    }

    private string MakeBriefing(EntityUid requester)
    {
        var metadata = MetaData(requester);
        var briefing = Loc.GetString("mercenary-briefing");
        if (requester != null)
        {
            if (metadata != null)
                briefing += "\n " + Loc.GetString("mercenary-briefing", ("employer", metadata.EntityName)) + "\n";
        }
        return briefing;
    }
}
