// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileThrowOnHit;

/// <summary>
/// This handles <see cref="ProjectileThrowOnHitComponent"/>
/// </summary>
public sealed class ProjectileThrowOnHitSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ProjectileThrowOnHitComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<ProjectileThrowOnHitComponent, ThrowDoHitEvent>(OnThrowHit);
    }

    private void OnProjectileHit(Entity<ProjectileThrowOnHitComponent> projectile, ref ProjectileHitEvent args)
    {
        if (!TryComp<PhysicsComponent>(projectile, out var physics))
            return;

        ThrowOnHitHelper(projectile, projectile, args.Target, physics.LinearVelocity);
    }

    private void OnThrowHit(Entity<ProjectileThrowOnHitComponent> projectile, ref ThrowDoHitEvent args)
    {
        if (!TryComp<PhysicsComponent>(args.Thrown, out var weaponPhysics))
            return;

        ThrowOnHitHelper(projectile, args.Component.Thrower, args.Target, weaponPhysics.LinearVelocity);
    }

    private void ThrowOnHitHelper(Entity<ProjectileThrowOnHitComponent> ent, EntityUid? user, EntityUid target, Vector2 direction)
    {
        var attemptEvent = new AttemptProjectileThrowOnHitEvent(target, user);
        RaiseLocalEvent(ent.Owner, ref attemptEvent);

        if (attemptEvent.Cancelled)
            return;

        var startEvent = new ProjectileThrowOnHitStartEvent(ent.Owner, user);
        RaiseLocalEvent(target, ref startEvent);

        if (ent.Comp.StunTime != null)
            _stun.TryParalyze(target, ent.Comp.StunTime.Value, false);

        if (direction == Vector2.Zero)
            return;

        _throwing.TryThrow(target, direction.Normalized() * ent.Comp.Distance, ent.Comp.Speed, user, unanchor: ent.Comp.UnanchorOnHit);
    }
}
