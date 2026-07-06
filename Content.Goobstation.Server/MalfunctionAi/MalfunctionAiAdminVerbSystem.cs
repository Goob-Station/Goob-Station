// SPDX-FileCopyrightText: 2026 Jonikibaka
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Managers;
using Content.Server.Antag;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.MalfunctionAi;

/// <summary>
/// Adds the admin "Make Malfunction AI" verb. Lives in its own system because the station AI
/// player is the brain held inside the core, so the regular antag verbs (which need the target
/// to be the player entity itself) never show up when right-clicking the core.
/// </summary>
public sealed class MalfunctionAiAdminVerbSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedStationAiSystem _stationAi = default!;

    private static readonly EntProtoId DefaultMalfunctionAiRule = "MalfunctionAi";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnGetVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        if (!_adminManager.HasAdminFlag(actor.PlayerSession, AdminFlags.Fun))
            return;

        // Right-clicking the core/intellicard: resolve the held brain and offer the verb on it.
        if (HasComp<StationAiHolderComponent>(args.Target)
            && _stationAi.TryGetHeld(new Entity<StationAiHolderComponent?>(args.Target, null), out var heldAi)
            && TryComp<ActorComponent>(heldAi, out var heldActor))
        {
            args.Verbs.Add(MakeVerb(heldActor.PlayerSession));
            return;
        }

        // Right-clicking the AI brain entity directly.
        if (HasComp<StationAiHeldComponent>(args.Target)
            && HasComp<MindContainerComponent>(args.Target)
            && TryComp<ActorComponent>(args.Target, out var targetActor))
        {
            args.Verbs.Add(MakeVerb(targetActor.PlayerSession));
        }
    }

    private Verb MakeVerb(ICommonSession targetPlayer)
    {
        var malfAiName = Loc.GetString("admin-verb-text-make-malfunction-ai");
        return new Verb
        {
            Text = malfAiName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "StationAi"),
            Act = () =>
            {
                _antag.ForceMakeAntag<MalfunctionAiRuleComponent>(targetPlayer, DefaultMalfunctionAiRule);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", malfAiName, Loc.GetString("admin-verb-make-malfunction-ai")),
        };
    }
}
