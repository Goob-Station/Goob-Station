// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Projectiles;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileForceTarget;

public sealed class ProjectileForceTargetSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileForceTargetComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(Entity<ProjectileForceTargetComponent> ent, ref ProjectileHitEvent args)
    {
        if (ent.Comp.Part != null)
            args.TargetPart = ent.Comp.Part;

        if (ent.Comp.MakeUnableToMiss)
            args.CanMiss = false;
    }
}
