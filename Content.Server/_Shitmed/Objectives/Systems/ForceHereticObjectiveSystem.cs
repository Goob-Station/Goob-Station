// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Shitmed.Objectives.Components;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Database;
using Content.Shared.Mind;
using Robust.Shared.Player;
namespace Content.Server._Shitmed.Objectives.Systems;

public sealed class ForceHereticObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindComponent, ObjectiveAddedEvent>(OnObjectiveAdded);
    }

    private void OnObjectiveAdded(EntityUid uid, MindComponent comp, ref ObjectiveAddedEvent args)
    {
        if (!TryComp<ActorComponent>(comp.CurrentEntity, out var actor))
            return;

        if (HasComp<ForceHereticObjectiveComponent>(args.Objective))
        {
            _antag.ForceMakeAntag<HereticRuleComponent>(actor.PlayerSession, "Heretic");

            _adminLogManager.Add(LogType.Mind,
                LogImpact.Medium,
                $"{ToPrettyString(uid)} has been given heretic status by an antag objective.");
        }
    }
}