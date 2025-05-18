// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using Content.Goobstation.Shared.MisandryBox.Smites;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Administration.Systems;

public sealed partial class GoobAdminVerbSystem
{
    private void AddSmiteVerbs(GetVerbsEvent<Verb> args)
    {
        if (!SmitesAllowed(args))
            return;

        var thunderstrike = Loc.GetString("admin-smite-thunderstrike-name").ToLowerInvariant();
        Verb thunder = new()
        {
            Text = thunderstrike,
            Category = VerbCategory.Smite,
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/smite.svg.192dpi.png")),
            Act = () =>
            {
                var ogun = EntityManager.System<ThunderstrikeSystem>();
                ogun.Smite(args.Target, kill: true);
            },
            Impact = LogImpact.Extreme,
            Message = Loc.GetString("admin-smite-thunderstrike-desc"),
        };
        args.Verbs.Add(thunder);
    }

    private bool SmitesAllowed(GetVerbsEvent<Verb> args)
    {
        if (!TryComp(args.User, out ActorComponent? actor))
            return false;

        var player = actor.PlayerSession;

        if (!_admin.HasAdminFlag(player, AdminFlags.Fun))
            return false;

        // 1984.
        if (HasComp<MapComponent>(args.Target) || HasComp<MapGridComponent>(args.Target))
            return false;

        return true;
    }
}
