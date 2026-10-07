// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Administration.Objectives;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Administration.Systems;

public sealed partial class GoobAdminVerbSystem
{
    [Dependency] private readonly AdminObjectivesSystem _adminObjectives = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;

    private void AddObjectiveVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor) ||
            !_admin.HasAdminFlag(actor.PlayerSession, AdminFlags.Admin))
            return;

        // only entities that have (or had) a mind to hold objectives
        if (!_mindSystem.TryGetMind(args.Target, out var mindId, out _))
            return;

        var session = actor.PlayerSession;
        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("admin-verb-text-edit-objectives"),
            Category = VerbCategory.Admin,
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/sentient.svg.192dpi.png")),
            Act = () => _adminObjectives.OpenPanel(session, mindId),
            Impact = LogImpact.Low,
            Message = Loc.GetString("admin-verb-edit-objectives"),
        });
    }
}
