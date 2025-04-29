// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Teleportation;
using Content.Shared.Projectiles;
using Content.Shared.Whitelist;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class RandomTeleportOnProjectileHitSystem : EntitySystem
{
    [Dependency] private readonly TeleportSystem _teleport = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomTeleportOnProjectileHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<RandomTeleportOnProjectileHitComponent> ent, ref ProjectileHitEvent args)
    {
        var (uid, comp) = ent;
        if (TryComp(uid, out RandomTeleportComponent? tele) && _whitelist.IsValid(comp.Whitelist, args.Target))
            _teleport.RandomTeleport(args.Target, tele);
    }
}