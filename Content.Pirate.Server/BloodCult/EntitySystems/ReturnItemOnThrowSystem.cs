// SPDX-FileCopyrightText: 2024 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server.BloodCult.Components;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared.IdentityManagement;
using Content.Shared.Item;
using Content.Shared.Mobs.Systems;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class ReturnItemOnThrowSystem : EntitySystem
{
    private const float FinishMovementDistance = 0.2f;

    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReturnItemOnThrowComponent, ThrowDoHitEvent>(OnThrowHit);
        SubscribeLocalEvent<ReturnItemOnThrowComponent, GettingPickedUpAttemptEvent>(OnPickupAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ReturnItemOnThrowComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.ReturningTo is not { } returningTo || !Exists(returningTo))
            {
                CancelReturn(uid, component);
                continue;
            }

            var currentPosition = _transform.GetWorldPosition(uid);
            var targetPosition = _transform.GetWorldPosition(returningTo);
            var direction = targetPosition - currentPosition;
            var distance = direction.Length();

            if (distance < FinishMovementDistance)
            {
                FinishReturn(uid, component);
                continue;
            }

            direction = Vector2.Normalize(direction);
            var movement = direction * component.ReturnSpeed * frameTime;
            if (movement.Length() >= distance)
            {
                FinishReturn(uid, component);
                continue;
            }

            _transform.SetWorldPosition(uid, currentPosition + movement);
            _transform.SetWorldRotation(uid, direction.ToWorldAngle());
        }
    }

    private void OnThrowHit(Entity<ReturnItemOnThrowComponent> item, ref ThrowDoHitEvent args)
    {
        var thrower = args.Component.Thrower;
        if (thrower is not { } throwerUid ||
            (item.Comp.ThrowerWhitelist is { } throwerWhitelist && !_whitelist.IsValid(throwerWhitelist, throwerUid)) ||
            (item.Comp.TargetWhitelist is { } targetWhitelist && !_whitelist.IsValid(targetWhitelist, args.Target)) ||
            (item.Comp.TargetBlacklist is { } targetBlacklist && _whitelist.IsValid(targetBlacklist, args.Target)) ||
            _mobState.IsDead(throwerUid))
            return;

        item.Comp.ReturningTo = throwerUid;
        if (TryComp<PhysicsComponent>(item, out var physics))
            _physics.SetCanCollide(item.Owner, false, body: physics);
    }

    private void OnPickupAttempt(Entity<ReturnItemOnThrowComponent> item, ref GettingPickedUpAttemptEvent args)
    {
        if (item.Comp.ReturningTo != null)
            args.Cancel();
    }

    private void FinishReturn(EntityUid uid, ReturnItemOnThrowComponent component)
    {
        if (component.ReturningTo is not { } returningTo || !Exists(returningTo))
        {
            CancelReturn(uid, component);
            return;
        }

        if (TryComp<PhysicsComponent>(uid, out var physics))
            _physics.SetCanCollide(uid, true, body: physics);

        component.ReturningTo = null;

        var message = Loc.GetString("return-item-to-hands", ("item", Identity.Entity(uid, EntityManager)));
        var messageToOthers = Loc.GetString(
            "return-item-to-hands-other",
            ("item", Identity.Entity(uid, EntityManager)),
            ("user", Identity.Entity(returningTo, EntityManager)));

        if (!_hands.TryPickupAnyHand(returningTo, uid))
        {
            message = Loc.GetString("return-item-to-feet", ("item", Identity.Entity(uid, EntityManager)));
            messageToOthers = Loc.GetString(
                "return-item-to-feet-other",
                ("item", Identity.Entity(uid, EntityManager)),
                ("user", Identity.Entity(returningTo, EntityManager)));
            _transform.SetWorldPosition(uid, _transform.GetWorldPosition(returningTo));
        }

        _popup.PopupEntity(message, uid, returningTo);
        _popup.PopupEntity(
            messageToOthers,
            uid,
            Filter.PvsExcept(returningTo, entityManager: EntityManager),
            true);
    }

    private void CancelReturn(EntityUid uid, ReturnItemOnThrowComponent component)
    {
        component.ReturningTo = null;
        if (TryComp<PhysicsComponent>(uid, out var physics))
            _physics.SetCanCollide(uid, true, body: physics);
    }
}
