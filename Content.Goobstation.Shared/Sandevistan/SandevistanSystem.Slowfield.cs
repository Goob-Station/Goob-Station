using System.Numerics;
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Goobstation.Shared.ForcedDirectionRotate;
using Content.Shared.Doors.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Shared.Sandevistan;

/// <summary>
/// Slowfield stuff. Slows down time and whatnot.
/// </summary>
public sealed partial class SandevistanSystem
{
    private const string SlowfieldFixtureId = "sandevistan-slowfield";
    private const string TrampleFixtureId = "sandevistan-trample";

    private void InitializeSlowfield()
    {
        SubscribeLocalEvent<SandevistanSlowedComponent, RemoveSandevistanSlowdownEvent>(OnRemoveSlowdown);
        SubscribeLocalEvent<SandevistanSlowedComponent, RefreshMovementSpeedModifiersEvent>(OnSlowedRefreshSpeed);
        SubscribeLocalEvent<SandevistanSlowedComponent, AttemptMeleeEvent>(OnSlowedAttemptMelee);
        SubscribeLocalEvent<SandevistanSlowedComponent, ComponentShutdown>(OnSlowedShutdown);
        SubscribeLocalEvent<SandevistanSlowedComponent, StartCollideEvent>(OnTrampledCollide);
        SubscribeLocalEvent<SandevistanSlowedComponent, StopThrowEvent>(OnTrampledStopThrow);

        SubscribeLocalEvent<ActiveSandevistanUserComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<ActiveSandevistanUserComponent, EndCollideEvent>(OnEndCollide);
        SubscribeLocalEvent<ActiveSandevistanUserComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<ActiveSandevistanUserComponent, AmmoShotUserEvent>(OnAmmoShot);

        SubscribeLocalEvent<MeleeWeaponComponent, MeleeHitEvent>(OnMeleeHit);

        SubscribeLocalEvent<PhysicsUpdateAfterSolveEvent>(OnPhysicsUpdateAfterSolve);
    }

    private void UpdateSlowfield()
    {
        var query = EntityQueryEnumerator<SandevistanSlowedComponent>();
        while (query.MoveNext(out var target, out var slowed))
        {
            if (!slowed.IsSlowed && !slowed.Trampled)
                RemComp(target, slowed);
        }
    }

    private void OnAmmoShot(Entity<ActiveSandevistanUserComponent> ent, ref AmmoShotUserEvent args)
    {
        if (!TryComp<SandevistanUserComponent>(ent, out var comp) || !comp.SlowfieldEnabled)
            return;

        foreach (var projectile in args.FiredProjectiles)
            ApplySlowdown(ent, projectile, comp);
    }

    private void SetFixtures(EntityUid uid, SandevistanUserComponent comp, bool create)
    {
        if (!TryComp<PhysicsComponent>(uid, out var physics))
            return;

        if (comp.SlowfieldEnabled)
            SetFixture(uid, physics, SlowfieldFixtureId, comp.SlowfieldRadius, create);

        if (comp.TrampleEnabled)
            SetFixture(uid, physics, TrampleFixtureId, comp.SlowfieldTrampleRadius, create);
    }

    private void SetFixture(EntityUid uid, PhysicsComponent physics, string id, float radius, bool create)
    {
        if (!create)
        {
            _fixtures.DestroyFixture(uid, id, body: physics);
            return;
        }

        _fixtures.TryCreateFixture(uid, new PhysShapeCircle(radius), id,
            collisionLayer: (int) CollisionGroup.ThrownItem, collisionMask: (int) CollisionGroup.None, hard: false, body: physics);
    }

    /// <summary>
    /// Melee hits landed while active can hit for increased damage, knock back the target, and disable the sandevistan.
    /// </summary>
    private void OnMeleeHit(Entity<MeleeWeaponComponent> weapon, ref MeleeHitEvent args)
    {
        if (!TryComp<SandevistanUserComponent>(args.User, out var comp)
            || !comp.Active
            || comp.DashActive
            || !args.IsHit
            || args.HitEntities.Count == 0
            || !_timing.IsFirstTimePredicted)
            return;

        var user = args.User;
        var userPos = _transform.GetWorldPosition(user);
        var userVel = TryComp<PhysicsComponent>(user, out var userBody) ? userBody.LinearVelocity : Vector2.Zero;

        args.BonusDamage += args.BaseDamage * (comp.HitDamageMultiplier - 1f);

        if (comp.HitKnockbackStrength > 0f)
        {
            foreach (var target in args.HitEntities)
            {
                if (!HasComp<PhysicsComponent>(target))
                    continue;

                var dir = _transform.GetWorldPosition(target) - userPos;

                if (dir.LengthSquared() < 0.01f)
                    dir = args.Direction ?? (userVel.LengthSquared() > 0.01f ? userVel : new Vector2(0f, -1f));

                LaunchTarget(user, comp, target, dir, userVel, comp.HitKnockbackStrength);
            }
        }

        if (!comp.HitDisables)
            return;

        PlayToggleSound((user, comp), comp.EndSound);
        Disable(user, comp);
    }

    private void LaunchTarget(EntityUid user, SandevistanUserComponent comp, EntityUid target, Vector2 dir, Vector2 userVel, float distance)
    {
        if (dir.LengthSquared() < 0.01f)
            return;

        var dirNorm = Vector2.Normalize(dir);
        var momentum = MathF.Max(0f, Vector2.Dot(userVel, dirNorm));
        var launch = dirNorm * (distance + momentum * comp.SlowfieldMomentumScale);

        if (!comp.HitKnockbackDelayed)
        {
            _grabThrown.Throw(target, user, launch, comp.SlowfieldKnockbackSpeed);
            return;
        }

        comp.PendingKnockback.TryGetValue(target, out var pending);
        comp.PendingKnockback[target] = pending + launch;
    }

    private void OnStartCollide(Entity<ActiveSandevistanUserComponent> ent, ref StartCollideEvent args)
    {
        if (!TryComp<SandevistanUserComponent>(ent, out var comp)
            || !comp.SlowfieldEnabled && !comp.TrampleEnabled)
            return;

        var target = args.OtherEntity;

        if (target == ent.Owner)
            return;

        switch (args.OurFixtureId)
        {
            case SlowfieldFixtureId:
                ApplySlowdown(ent, target, comp);
                break;

            case TrampleFixtureId:
                TryTrample(ent, comp, target);
                break;
        }
    }

    /// <summary>
    /// Shoves a living mob the active sandevistan user runs into, scaled by how fast they're moving.
    /// </summary>
    private void TryTrample(EntityUid user, SandevistanUserComponent comp, EntityUid target)
    {
        // The dash attack moves too fast for the normal colllision based trample to actually register.
        if (comp.DashActive && target == comp.DashTarget)
            return;

        if (HasComp<ActiveSandevistanUserComponent>(target)
            || !_mobState.IsAlive(target)
            || !TryComp<PhysicsComponent>(user, out var userBody))
            return;

        var userVel = userBody.LinearVelocity;
        if (userVel.Length() < comp.SlowfieldTrampleMinSpeed)
            return;

        Trample(user, comp, target, userVel);
    }

    /// <summary>
    /// Shoves the target.
    /// </summary>
    private void Trample(EntityUid user, SandevistanUserComponent comp, EntityUid target, Vector2 velocity)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        LaunchTarget(user, comp, target, velocity, velocity, comp.SlowfieldKnockbackDistance);
        ApplyTrampleAirSlowdown(user, target, comp);

        _damageable.TryChangeDamage(target, comp.SlowfieldSlamDamage, origin: user);
        _audio.PlayPredicted(comp.SlowfieldSlamSound, target, user);
    }

    private void OnEndCollide(Entity<ActiveSandevistanUserComponent> ent, ref EndCollideEvent args)
    {
        var target = args.OtherEntity;

        if (!TryComp<SandevistanSlowedComponent>(target, out var slowed) || slowed.Source != ent.Owner)
            return;

        if (args.OurFixtureId != SlowfieldFixtureId)
            return;

        var ev = new RemoveSandevistanSlowdownEvent(ent.Owner);
        RaiseLocalEvent(target, ref ev);
    }

    private void OnPreventCollide(Entity<ActiveSandevistanUserComponent> ent, ref PreventCollideEvent args)
    {
        if (!TryComp<FixturesComponent>(ent, out var fixtures)
            || !fixtures.Fixtures.TryGetValue(SlowfieldFixtureId, out var slowfieldFixture)
            || args.OurFixture != slowfieldFixture)
            return;

        if (HasComp<DoorComponent>(args.OtherEntity))
            args.Cancelled = true;
    }

    private void ApplySlowdown(EntityUid source, EntityUid target, SandevistanUserComponent comp)
    {
        if (TryComp<SandevistanSlowedComponent>(target, out var existing) && existing.IsSlowed
            || HasComp<ActiveSandevistanUserComponent>(target))
            return;

        if (!HasComp<MobStateComponent>(target) && !HasComp<ProjectileComponent>(target) && !HasComp<ThrownItemComponent>(target))
            return;

        var slowed = EnsureComp<SandevistanSlowedComponent>(target);
        slowed.IsSlowed = true;
        slowed.Source = source;

        // Mobs
        if (HasComp<MobStateComponent>(target))
        {
            slowed.SpeedMultiplier = comp.MobSpeedMultiplier;
            _speed.RefreshMovementSpeedModifiers(target);
            EntityManager.AddComponents(target, comp.VisionComponents);
        }

        // Bullets
        else if (HasComp<ProjectileComponent>(target))
        {
            slowed.SpeedMultiplier = comp.ProjectileSpeedMultiplier;
            ApplyVelocitySlowdown(target, slowed);
        }

        // Thrown items
        else if (TryComp<ThrownItemComponent>(target, out var thrown))
        {
            slowed.SpeedMultiplier = comp.ThrownItemSpeedMultiplier;
            ApplyVelocitySlowdown(target, slowed, thrown);
        }

        Dirty(target, slowed);
    }

    /// <summary>
    /// Scales a moving body's velocity down, remembering the original so it can be restored later. A thrown
    /// item also gets its landing pushed back to match (e.g. 95% slower = 20x longer to land).
    /// </summary>
    private void ApplyVelocitySlowdown(EntityUid target, SandevistanSlowedComponent slowed, ThrownItemComponent? thrown = null)
    {
        if (!TryComp<PhysicsComponent>(target, out var physics))
            return;

        slowed.OriginalLinearVelocity = physics.LinearVelocity;
        _physics.SetLinearVelocity(target, physics.LinearVelocity * slowed.SpeedMultiplier, body: physics);

        if (thrown?.LandTime is { } landTime && slowed.SpeedMultiplier > 0)
            thrown.LandTime = _timing.CurTime + (landTime - _timing.CurTime) / slowed.SpeedMultiplier;
    }

    private void ApplyTrampleAirSlowdown(EntityUid source, EntityUid target, SandevistanUserComponent comp)
    {
        if (!TryComp<ThrownItemComponent>(target, out var thrown))
            return;

        var slowed = EnsureComp<SandevistanSlowedComponent>(target);
        slowed.IsSlowed = true;
        slowed.Source = source;
        slowed.SpeedMultiplier = comp.ThrownItemSpeedMultiplier;
        ApplyVelocitySlowdown(target, slowed, thrown);

        if (thrown.LandTime is { } landTime)
            _stun.TryKnockdown(target, landTime - _timing.CurTime, refresh: true, force: true, drop: false);

        slowed.Trampled = true;

        EnsureComp<ForcedDirectionRotateComponent>(target);
        Dirty(target, slowed);
    }

    private void OnTrampledCollide(Entity<SandevistanSlowedComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.Trampled
            || !args.OtherFixture.Hard
            || args.OtherBody.BodyType != BodyType.Static
            || !TryComp<ThrownItemComponent>(ent, out var thrown))
            return;

        _thrownItem.StopThrow(ent, thrown);
    }

    private void OnTrampledStopThrow(Entity<SandevistanSlowedComponent> ent, ref StopThrowEvent args)
    {
        if (!ent.Comp.Trampled)
            return;

        _stun.SetKnockdownTime(ent.Owner, TimeSpan.Zero);
        ent.Comp.Trampled = false;
        RemCompDeferred<ForcedDirectionRotateComponent>(ent);

        if (!TryComp<SandevistanUserComponent>(ent.Comp.Source, out var source) || !source.SlowfieldEnabled)
        {
            ent.Comp.OriginalLinearVelocity = Vector2.Zero;
            var ev = new RemoveSandevistanSlowdownEvent(ent.Comp.Source);
            RaiseLocalEvent(ent, ref ev);
        }
    }

    private void OnRemoveSlowdown(Entity<SandevistanSlowedComponent> ent, ref RemoveSandevistanSlowdownEvent args)
    {
        if (ent.Comp.Source != args.Source
            || !ent.Comp.IsSlowed)
            return;

        ent.Comp.IsSlowed = false;

        var isMob = HasComp<MobStateComponent>(ent);
        var isThrown = TryComp<ThrownItemComponent>(ent, out var thrown);

        if (isMob)
        {
            _speed.RefreshMovementSpeedModifiers(ent);
            if (TryComp<SandevistanUserComponent>(args.Source, out var source))
                EntityManager.RemoveComponents(ent, source.VisionComponents);
            RestoreSlowedWeapon(ent);
        }

        // Bullets and thrown items always carry slowed velocity; a mob only does while flying from a trample.
        if ((!isMob || isThrown)
            && ent.Comp.OriginalLinearVelocity.LengthSquared() > 0.01f
            && TryComp<PhysicsComponent>(ent, out var physics))
            _physics.SetLinearVelocity(ent, ent.Comp.OriginalLinearVelocity, body: physics);

        // Convert the remaining slowed landing time back to normal speed
        // e.g. slowed 95% (mult=0.05), 10s remain in slowfield time = 0.5s at normal speed
        if (thrown?.LandTime is { } landTime && ent.Comp.SpeedMultiplier > 0)
            thrown.LandTime = _timing.CurTime + (landTime - _timing.CurTime) * ent.Comp.SpeedMultiplier;
    }

    private void OnSlowedAttemptMelee(Entity<SandevistanSlowedComponent> ent, ref AttemptMeleeEvent args)
    {
        if (!ent.Comp.IsSlowed || ent.Comp.SpeedMultiplier <= 0f || ent.Comp.SlowedWeapon == args.Weapon)
            return;

        if (ent.Comp.SlowedWeapon != null)
            RestoreSlowedWeapon(ent);

        ent.Comp.SlowedWeapon = args.Weapon;
        ent.Comp.SlowedWeaponOriginalAttackRate = args.WeaponComponent.AttackRate;
        args.WeaponComponent.AttackRate *= ent.Comp.SpeedMultiplier;
        DirtyField(args.Weapon, args.WeaponComponent, nameof(MeleeWeaponComponent.AttackRate));
    }

    private void OnSlowedShutdown(Entity<SandevistanSlowedComponent> ent, ref ComponentShutdown args) =>
        RestoreSlowedWeapon(ent);

    /// <summary>
    /// Restores the attack rate of the weapon we slowed for this entity, if any.
    /// </summary>
    private void RestoreSlowedWeapon(Entity<SandevistanSlowedComponent> ent)
    {
        if (ent.Comp.SlowedWeapon is not { } weapon)
            return;

        if (TryComp<MeleeWeaponComponent>(weapon, out var melee))
        {
            melee.AttackRate = ent.Comp.SlowedWeaponOriginalAttackRate;
            DirtyField(weapon, melee, nameof(MeleeWeaponComponent.AttackRate));
        }

        ent.Comp.SlowedWeapon = null;
    }

    private void OnSlowedRefreshSpeed(Entity<SandevistanSlowedComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (HasComp<MobStateComponent>(ent) && ent.Comp.IsSlowed)
            args.ModifySpeed(ent.Comp.SpeedMultiplier, ent.Comp.SpeedMultiplier);
    }

    /// <summary>
    /// Used to continuously enforce slowdown on thrown items, otherwise they would ignore it.
    /// </summary>
    private void OnPhysicsUpdateAfterSolve(ref PhysicsUpdateAfterSolveEvent args)
    {
        var query = EntityQueryEnumerator<SandevistanSlowedComponent>();
        while (query.MoveNext(out var uid, out var slowed))
        {
            if (!slowed.IsSlowed || !HasComp<ThrownItemComponent>(uid) || slowed.OriginalLinearVelocity.LengthSquared() <= 0.01f)
                continue;

            if (!_netManager.IsServer && HasComp<MobStateComponent>(uid))
                continue;

            var targetVelocity = slowed.OriginalLinearVelocity * slowed.SpeedMultiplier;
            if (TryComp<PhysicsComponent>(uid, out var physics)
                && (physics.LinearVelocity - targetVelocity).LengthSquared() > 0.01f)
                _physics.SetLinearVelocity(uid, targetVelocity, body: physics);
        }
    }
}
