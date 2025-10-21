// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar; //Goobstation - Crawling
using Content.Goobstation.Common.Projectiles;
using Content.Shared._DV.Abilities;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Standing;
using Robust.Shared.Physics.Events;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Configuration; //Goobstation - Crawling

namespace Content.Shared.Damage.Components;

public sealed class RequireProjectileTargetSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    // Goobstation
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private float _crawlHitzoneSize; //Goobstation

    public override void Initialize()
    {
        SubscribeLocalEvent<RequireProjectileTargetComponent, PreventCollideEvent>(PreventCollide);
        SubscribeLocalEvent<RequireProjectileTargetComponent, StoodEvent>(StandingBulletHit);
        SubscribeLocalEvent<RequireProjectileTargetComponent, DownedEvent>(LayingBulletPass);
        _cfg.OnValueChanged(GoobCVars.CrawlHitzoneSize, value => _crawlHitzoneSize = value, true); //Goobstation - Crawling
    }

    private void PreventCollide(Entity<RequireProjectileTargetComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Active)
            return;

        var other = args.OtherEntity;
        // Goob edit start
        if (TryComp(other, out TargetedProjectileComponent? targeted))
        {
            if (targeted.Target == null || targeted.Target == ent)
                return;

            var ev = new ShouldTargetedProjectileCollideEvent(targeted.Target.Value);
            RaiseLocalEvent(ent, ev);
            if (ev.Handled)
                return;
        }

        if (TryComp(other, out ProjectileComponent? projectile))
        {
            // Goob edit end

            // Prevents shooting out of while inside of crates
            var shooter = projectile.Shooter;
            if (!shooter.HasValue)
                return;

            // Goobstation - Crawling
            if (TryComp<CrawlUnderObjectsComponent>(shooter, out var crawl) && crawl.Enabled)
                return;

            if (TryComp(ent, out PhysicsComponent? physics) && physics.LinearVelocity.Length() > 2.5f) // Goobstation
                return;

            // ProjectileGrenades delete the entity that's shooting the projectile,
            // so it's impossible to check if the entity is in a container
            if (TerminatingOrDeleted(shooter.Value))
                return;

            if ((_transform.GetMapCoordinates(ent).Position - projectile.TargetCoordinates).Length() <= _crawlHitzoneSize) //Goobstation
                return;

            if (!_container.IsEntityOrParentInContainer(shooter.Value))
                args.Cancelled = true;
        }
    }

    private void SetActive(Entity<RequireProjectileTargetComponent> ent, bool value)
    {
        if (ent.Comp.Active == value)
            return;

        ent.Comp.Active = value;
        Dirty(ent);
    }

    private void StandingBulletHit(Entity<RequireProjectileTargetComponent> ent, ref StoodEvent args)
    {
        SetActive(ent, false);
    }

    private void LayingBulletPass(Entity<RequireProjectileTargetComponent> ent, ref DownedEvent args)
    {
        SetActive(ent, true);
    }
}
