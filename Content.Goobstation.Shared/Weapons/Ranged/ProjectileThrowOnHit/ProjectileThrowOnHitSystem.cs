// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Weapons.Ranged.ProjectileThrowOnHit;

/// <summary>
/// This handles <see cref="ProjectileThrowOnHitComponent"/>
/// </summary>
public sealed class ProjectileThrowOnHitSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ProjectileThrowOnHitComponent, StartCollideEvent>(OnProjectileCollide);
        SubscribeLocalEvent<ProjectileThrowOnHitComponent, ThrowDoHitEvent>(OnThrowHit);
    }

    private void OnProjectileCollide(Entity<ProjectileThrowOnHitComponent> projectile, ref StartCollideEvent args)
    {
        var projectilePos = _transform.GetWorldPosition(args.OurEntity);

        var targetPos = _transform.GetMapCoordinates(args.OtherEntity).Position;
        var direction = targetPos - projectilePos;
        ThrowOnHitHelper(projectile, args.OurEntity, args.OtherEntity, direction);
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
