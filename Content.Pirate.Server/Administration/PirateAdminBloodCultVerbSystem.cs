// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Managers;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Pirate.Server.Administration.Systems;

public sealed class PirateAdminBloodCultVerbSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IAdminManager _admin = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnGetVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;
        if (!_admin.HasAdminFlag(player, AdminFlags.Fun))
            return;

        if (!HasComp<MindContainerComponent>(args.Target) || !TryComp<ActorComponent>(args.Target, out var targetActor))
            return;

        Verb cultist = new()
        {
            Text = Loc.GetString("admin-verb-text-make-cultist"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Pirate/BloodCult/spells_cultist.rsi"), "summon_dagger"),
            Act = () =>
            {
                _antag.ForceMakeAntag<BloodCultRuleComponent>(targetActor.PlayerSession, "BloodCult");
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-cultist"),
        };
        args.Verbs.Add(cultist);
    }
}
