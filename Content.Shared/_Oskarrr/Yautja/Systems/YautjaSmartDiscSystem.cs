using System.Numerics;
using Content.Shared.Damage;
using Content.Shared.Follower;
using Content.Shared.Follower.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

using Content.Shared._Oskarrr.Yautja.Components;

namespace Content.Shared._Oskarrr.Yautja.Systems;

public sealed class YautjaSmartDiscSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly FollowerSystem _follow = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly ThrownItemSystem _thrownItem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<YautjaSmartDiscComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<YautjaSmartDiscComponent, ThrowDoHitEvent>(OnThrowHit);
        SubscribeLocalEvent<YautjaSmartDiscComponent, LandEvent>(OnLand);
    }

    private void OnUseInHand(Entity<YautjaSmartDiscComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.State != YautjaSmartDiscState.Idle)
        {
            args.Handled = true;
            return;
        }

        var user = args.User;
        var target = GetNearestLiving(user, ent.Comp.Range);

        if (target == null)
        {
            _popup.PopupPredicted(Loc.GetString("yautja-smart-disc-no-target"), user, user);
            args.Handled = true;
            return;
        }

        if (!_hands.TryDrop(user, ent.Owner, checkActionBlocker: false))
        {
            args.Handled = true;
            return;
        }

        LaunchAtTarget(ent, user, target.Value);
        args.Handled = true;
    }

    private void OnThrowHit(Entity<YautjaSmartDiscComponent> ent, ref ThrowDoHitEvent args)
    {
        if (ent.Comp.State != YautjaSmartDiscState.Flying)
            return;

        if (ent.Comp.Target != args.Target)
            return;

        BeginOrbit(ent, args.Target);
    }

    private void OnLand(Entity<YautjaSmartDiscComponent> ent, ref LandEvent args)
    {
        if (ent.Comp.State == YautjaSmartDiscState.Flying && ent.Comp.Target is { } target
            && !TerminatingOrDeleted(target) && _mobState.IsAlive(target)
            && IsNear(ent, target, ent.Comp.ArrivalDistance))
        {
            BeginOrbit(ent, target);
            return;
        }

        if (ent.Comp.State == YautjaSmartDiscState.Returning && ent.Comp.Thrower is { } owner
            && !TerminatingOrDeleted(owner) && IsNear(ent, owner, ent.Comp.PickupDistance))
        {
            TryReturnToOwner(ent, owner);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<YautjaSmartDiscComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var ent = (uid, comp);

            switch (comp.State)
            {
                case YautjaSmartDiscState.Flying:
                    UpdateFlying(ent, frameTime);
                    break;
                case YautjaSmartDiscState.Orbiting:
                    UpdateOrbiting(ent, frameTime);
                    break;
                case YautjaSmartDiscState.Returning:
                    UpdateReturning(ent, frameTime);
                    break;
            }
        }
    }

    private void LaunchAtTarget(Entity<YautjaSmartDiscComponent> ent, EntityUid owner, EntityUid target)
    {
        ent.Comp.Thrower = owner;
        ent.Comp.Target = target;
        ent.Comp.State = YautjaSmartDiscState.Flying;
        ent.Comp.CompletedOrbits = 0;
        ent.Comp.OrbitAccumulator = 0f;
        ent.Comp.HomingTimer = 0f;
        Dirty(ent);

        ThrowToward(ent, target, ent.Comp.ThrowSpeed);
    }

    private void UpdateFlying(Entity<YautjaSmartDiscComponent> ent, float frameTime)
    {
        if (ent.Comp.Target is not { } target || TerminatingOrDeleted(target) || !_mobState.IsAlive(target))
        {
            BeginReturn(ent);
            return;
        }

        if (IsNear(ent, target, ent.Comp.ArrivalDistance))
        {
            BeginOrbit(ent, target);
            return;
        }

        ent.Comp.HomingTimer += frameTime;
        if (ent.Comp.HomingTimer < ent.Comp.HomingRetargetInterval)
            return;

        ent.Comp.HomingTimer = 0f;
        ThrowToward(ent, target, ent.Comp.ThrowSpeed);
    }

    private void BeginOrbit(Entity<YautjaSmartDiscComponent> ent, EntityUid target)
    {
        if (ent.Comp.State == YautjaSmartDiscState.Orbiting && ent.Comp.Target == target)
            return;

        if (TryComp<ThrownItemComponent>(ent, out var thrown))
            _thrownItem.StopThrow(ent, thrown);

        if (TryComp<PhysicsComponent>(ent, out var physics))
            _physics.SetLinearVelocity(ent, Vector2.Zero, body: physics);

        ent.Comp.Target = target;
        ent.Comp.State = YautjaSmartDiscState.Orbiting;
        ent.Comp.OrbitAccumulator = 0f;
        Dirty(ent);

        _follow.StartFollowingEntity(ent, target);

        if (TryComp<OrbitVisualsComponent>(ent, out var orbit))
        {
            orbit.OrbitLength = ent.Comp.OrbitPeriod;
            orbit.OrbitDistance = ent.Comp.OrbitDistance;
        }
    }

    private void UpdateOrbiting(Entity<YautjaSmartDiscComponent> ent, float frameTime)
    {
        if (ent.Comp.Target is not { } target || TerminatingOrDeleted(target) || !_mobState.IsAlive(target))
        {
            BeginReturn(ent);
            return;
        }

        ent.Comp.OrbitAccumulator += frameTime;
        if (ent.Comp.OrbitAccumulator < ent.Comp.OrbitPeriod)
            return;

        ent.Comp.OrbitAccumulator -= ent.Comp.OrbitPeriod;
        ApplyOrbitDamage(ent, target);
        ent.Comp.CompletedOrbits++;
        Dirty(ent);

        if (ent.Comp.CompletedOrbits >= ent.Comp.MaxOrbits)
            BeginReturn(ent);
    }

    private void BeginReturn(Entity<YautjaSmartDiscComponent> ent)
    {
        if (ent.Comp.State == YautjaSmartDiscState.Returning || ent.Comp.State == YautjaSmartDiscState.Idle)
            return;

        if (ent.Comp.Target is { } oldTarget && !TerminatingOrDeleted(oldTarget)
            && TryComp<FollowerComponent>(ent, out var follower) && follower.Following == oldTarget)
        {
            _follow.StopFollowingEntity(ent, oldTarget);
        }
        else if (TryComp<FollowerComponent>(ent, out var anyFollower))
        {
            _follow.StopFollowingEntity(ent, anyFollower.Following);
        }

        RemComp<OrbitVisualsComponent>(ent);

        ent.Comp.Target = null;
        ent.Comp.State = YautjaSmartDiscState.Returning;
        ent.Comp.HomingTimer = 0f;
        Dirty(ent);

        if (ent.Comp.Thrower is { } owner && !TerminatingOrDeleted(owner))
            ThrowToward(ent, owner, ent.Comp.ReturnSpeed);
        else
            ResetIdle(ent);
    }

    private void UpdateReturning(Entity<YautjaSmartDiscComponent> ent, float frameTime)
    {
        if (ent.Comp.Thrower is not { } owner || TerminatingOrDeleted(owner))
        {
            ResetIdle(ent);
            return;
        }

        if (IsNear(ent, owner, ent.Comp.PickupDistance))
        {
            TryReturnToOwner(ent, owner);
            return;
        }

        ent.Comp.HomingTimer += frameTime;
        if (ent.Comp.HomingTimer < ent.Comp.HomingRetargetInterval)
            return;

        ent.Comp.HomingTimer = 0f;
        ThrowToward(ent, owner, ent.Comp.ReturnSpeed);
    }

    private void TryReturnToOwner(Entity<YautjaSmartDiscComponent> ent, EntityUid owner)
    {
        if (TryComp<ThrownItemComponent>(ent, out var thrown))
            _thrownItem.StopThrow(ent, thrown);

        if (!_hands.TryPickupAnyHand(owner, ent, checkActionBlocker: false))
        {
            // Немає вільної руки — лишаємо біля власника і скидаємо стан.
            _transform.SetCoordinates(ent, Transform(owner).Coordinates);
        }

        ResetIdle(ent);
    }

    private void ResetIdle(Entity<YautjaSmartDiscComponent> ent)
    {
        ent.Comp.State = YautjaSmartDiscState.Idle;
        ent.Comp.Thrower = null;
        ent.Comp.Target = null;
        ent.Comp.CompletedOrbits = 0;
        ent.Comp.OrbitAccumulator = 0f;
        ent.Comp.HomingTimer = 0f;
        Dirty(ent);
    }

    private void ApplyOrbitDamage(Entity<YautjaSmartDiscComponent> ent, EntityUid target)
    {
        _damageable.TryChangeDamage(target, ent.Comp.OrbitDamage, origin: ent.Comp.Thrower, interruptsDoAfters: false);
    }

    private void ThrowToward(EntityUid disc, EntityUid destination, float speed)
    {
        if (TryComp<ThrownItemComponent>(disc, out var thrown))
            _thrownItem.StopThrow(disc, thrown);

        var from = _transform.GetMapCoordinates(disc);
        var to = _transform.GetMapCoordinates(destination);
        if (from.MapId != to.MapId)
            return;

        var direction = to.Position - from.Position;
        if (direction.LengthSquared() < 0.01f)
            return;

        _throwing.TryThrow(disc, direction, speed, user: null, recoil: false, playSound: false, doSpin: true);
    }

    private bool IsNear(EntityUid a, EntityUid b, float distance)
    {
        var mapA = _transform.GetMapCoordinates(a);
        var mapB = _transform.GetMapCoordinates(b);
        if (mapA.MapId != mapB.MapId)
            return false;

        return (mapA.Position - mapB.Position).Length() <= distance;
    }

    private EntityUid? GetNearestLiving(EntityUid origin, float range)
    {
        var originPos = _transform.GetWorldPosition(origin);
        EntityUid? nearest = null;
        var nearestDist = float.MaxValue;

        foreach (var uid in _lookup.GetEntitiesInRange(origin, range, flags: LookupFlags.Dynamic | LookupFlags.Approximate))
        {
            if (uid == origin || !HasComp<MobStateComponent>(uid) || !_mobState.IsAlive(uid))
                continue;

            if (!_interaction.InRangeUnobstructed(origin, uid, range))
                continue;

            var dist = (originPos - _transform.GetWorldPosition(uid)).Length();
            if (dist >= nearestDist)
                continue;

            nearestDist = dist;
            nearest = uid;
        }

        return nearest;
    }
}
