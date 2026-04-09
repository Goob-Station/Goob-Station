using Content.Shared._Shitmed.DoAfter;
using Content.Shared.Abilities;
using Content.Shared.Alert;
using Content.Shared.Damage.Events;
using Content.Shared.Doors.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Goobstation.Common.Weapons.Ranged;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Sandevistan;

public sealed class SandevistanSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private const string SlowfieldFixtureId = "sandevistan-slowfield";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanUserComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SandevistanUserComponent, ToggleSandevistanEvent>(OnToggle);
        SubscribeLocalEvent<SandevistanUserComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<SandevistanUserComponent, MeleeAttackEvent>(OnMeleeAttack);
        SubscribeLocalEvent<SandevistanUserComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<SandevistanUserComponent, GetDoAfterDelayMultiplierEvent>(OnModifyDoAfterDelay);
        SubscribeLocalEvent<SandevistanUserComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);

        SubscribeLocalEvent<SandevistanSlowedComponent, RemoveSandevistanSlowdownEvent>(OnRemoveSlowdown);
        SubscribeLocalEvent<SandevistanSlowedComponent, RefreshMovementSpeedModifiersEvent>(OnSlowedRefreshSpeed);

        SubscribeLocalEvent<ActiveSandevistanUserComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<ActiveSandevistanUserComponent, EndCollideEvent>(OnEndCollide);
        SubscribeLocalEvent<ActiveSandevistanUserComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<ActiveSandevistanUserComponent, AmmoShotUserEvent>(OnAmmoShot);

        SubscribeLocalEvent<PhysicsUpdateAfterSolveEvent>(OnPhysicsUpdateAfterSolve);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var cleanupQuery = EntityQueryEnumerator<SandevistanSlowedComponent>();
        while (cleanupQuery.MoveNext(out var target, out var slowed))
        {
            if (!slowed.IsSlowed)
                RemComp(target, slowed);
        }

        if (_netManager.IsServer)
        {
            var glitchQuery = EntityQueryEnumerator<SandevistanGlitchComponent>();
            while (glitchQuery.MoveNext(out var glitchUid, out var glitchComp))
            {
                if (_timing.CurTime >= glitchComp.ExpiresAt)
                    RemCompDeferred<SandevistanGlitchComponent>(glitchUid);
            }
        }

        if (_netManager.IsServer)
        {
            var inactiveQuery = EntityQueryEnumerator<SandevistanUserComponent>();
            while (inactiveQuery.MoveNext(out var inactiveUid, out var inactiveComp))
            {
                if (inactiveComp.Active || inactiveComp.CurrentLoad <= 0f)
                    continue;

                inactiveComp.CurrentLoad = MathF.Max(0f, inactiveComp.CurrentLoad + inactiveComp.LoadPerInactiveSecond * frameTime);
                Dirty(inactiveUid, inactiveComp);
            }
        }

        var query = EntityQueryEnumerator<ActiveSandevistanUserComponent, SandevistanUserComponent>();
        while (query.MoveNext(out var uid, out _, out var comp))
        {
            UpdateAfterimages(uid, comp);

            if (_netManager.IsServer)
            {
                comp.CurrentLoad += comp.LoadPerActiveSecond * frameTime;
                Dirty(uid, comp);
            }

            var filteredStates = new List<int>();
            foreach (var stateThreshold in comp.Thresholds)
                if (comp.CurrentLoad >= stateThreshold.Value)
                    filteredStates.Add((int) stateThreshold.Key);

            filteredStates.Sort((a, b) => b.CompareTo(a));
            foreach (var state in filteredStates)
            {
                if (!comp.Effects.TryGetValue((SandevistanState) state, out var effects))
                    continue;

                foreach (var effect in effects)
                    effect.Effect(uid, comp, EntityManager, frameTime);
            }

            if (comp.NextPopupTime > _timing.CurTime)
            {
                Dirty(uid, comp);
                continue;
            }

            var popup = -1;
            foreach (var state in filteredStates)
                if (state > popup && state < 4) // Goida
                    popup = state;

            if (popup == -1)
                continue;

            if (_netManager.IsServer)
                _popup.PopupEntity(Loc.GetString("sandevistan-overload-" + popup), uid, uid);

            comp.NextPopupTime = _timing.CurTime + comp.PopupDelay;
            Dirty(uid, comp);
        }
    }

    private void OnInit(Entity<SandevistanUserComponent> ent, ref ComponentInit args)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.LoadAlert);
        Dirty(ent);
    }

    private void OnToggle(Entity<SandevistanUserComponent> ent, ref ToggleSandevistanEvent args)
    {
        args.Handled = true;

        if (ent.Comp.Active)
        {
            _audio.PlayPredicted(ent.Comp.EndSound, ent, ent);
            Disable(ent, ent.Comp);
            Dirty(ent);
            return;
        }

        ent.Comp.Active = true;
        EnsureComp<ActiveSandevistanUserComponent>(ent);

        if (TryComp<SandevistanSlowedComponent>(ent, out var slowed))
        {
            var ev = new RemoveSandevistanSlowdownEvent(slowed.Source);
            RaiseLocalEvent(ent, ref ev);
        }

        _speed.RefreshMovementSpeedModifiers(ent);

        EnsureComp<DogVisionComponent>(ent);

        if (ent.Comp.SlowfieldEnabled)
            CreateSlowfieldFixture(ent, ent.Comp);

        _audio.PlayPredicted(ent.Comp.StartSound, ent, ent);
        Dirty(ent);
        PlayLoopedAudio(ent, ent.Comp);
    }

    private void OnRefreshSpeed(Entity<SandevistanUserComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Active)
            args.ModifySpeed(ent.Comp.MovementSpeedModifier, ent.Comp.MovementSpeedModifier);
    }

    private void OnMeleeAttack(Entity<SandevistanUserComponent> ent, ref MeleeAttackEvent args)
    {
        if (!ent.Comp.Active
            || !TryComp<MeleeWeaponComponent>(args.Weapon, out var weapon))
            return;

        var rate = weapon.NextAttack - _timing.CurTime; //weapon.AttackRate; breaks things when multiple systems modify NextAttack
        weapon.NextAttack -= rate - rate / ent.Comp.AttackSpeedModifier;
    }

    private void OnModifyDoAfterDelay(Entity<SandevistanUserComponent> ent, ref GetDoAfterDelayMultiplierEvent args)
    {
        if (ent.Comp.Active && ent.Comp.DoAfterSpeedEnabled)
            args.Multiplier *= 0.5f;
    }

    private void OnBeforeStaminaDamage(Entity<SandevistanUserComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (ent.Comp.Active
            && args.Source == ent.Owner)
            args.Cancelled = true;
    }

    private void OnMobStateChanged(Entity<SandevistanUserComponent> ent, ref MobStateChangedEvent args) =>
        Disable(ent, ent.Comp);

    private void OnShutdown(Entity<SandevistanUserComponent> ent, ref ComponentShutdown args)
    {
        Disable(ent, ent.Comp);
        _alerts.ClearAlert(ent.Owner, ent.Comp.LoadAlert);
    }

    public void Disable(EntityUid uid, SandevistanUserComponent comp)
    {
        var wasActive = comp.Active;
        if (comp.Active)
        {
            if (comp.SlowfieldEnabled)
            {
                DestroySlowfieldFixture(uid, comp);

                // Remove slowdown from all affected entities
                var query = EntityQueryEnumerator<SandevistanSlowedComponent>();
                while (query.MoveNext(out var target, out var slowed))
                {
                    if (slowed.Source != uid)
                        continue;

                    var ev = new RemoveSandevistanSlowdownEvent(uid);
                    RaiseLocalEvent(target, ref ev);
                }
            }

            RemCompDeferred<ActiveSandevistanUserComponent>(uid);
            comp.Active = false;
        }

        comp.LastEnabled = _timing.CurTime;
        comp.ColorAccumulator = 0;
        _speed.RefreshMovementSpeedModifiers(uid);
        DeleteAfterimages(uid);
        StopLoopedAudio(comp);

        RemCompDeferred<DogVisionComponent>(uid);

        if (wasActive)
            Dirty(uid, comp);
    }

    #region Afterimage Methods
    /// <summary>
    /// Update afterimages for sandevistan user
    /// </summary>
    public void UpdateAfterimages(EntityUid uid, SandevistanUserComponent comp)
    {
        if (_timing.CurTime >= comp.NextAfterimageTime)
        {
            SpawnAfterimage(uid, comp);

            comp.NextAfterimageTime = _timing.CurTime + TimeSpan.FromSeconds(comp.AfterimageInterval);
        }

        comp.ColorAccumulator++;
    }

    /// <summary>
    /// Spawn afterimage for sandevistan user
    /// </summary>
    private void SpawnAfterimage(EntityUid uid, SandevistanUserComponent comp)
    {
        var xform = Transform(uid);
        var coordinates = xform.Coordinates;
        var afterimage = Spawn(null, coordinates);

        var afterimageComp = EnsureComp<SandevistanAfterimageComponent>(afterimage);
        afterimageComp.SourceEntity = uid;
        afterimageComp.Hue = comp.ColorAccumulator % 100f / 100f;
        afterimageComp.DirectionOverride = xform.LocalRotation.GetCardinalDir();
        Dirty(afterimage, afterimageComp);
    }

    /// <summary>
    /// Deletes all afterimages for a given source entity
    /// </summary>
    public void DeleteAfterimages(EntityUid sourceUid)
    {
        // Sometimes it doesn't capture the last afterimage. This just makes sure the timing isn't off.
        Timer.Spawn(TimeSpan.FromSeconds(1), () =>
        {
            var query = EntityQueryEnumerator<SandevistanAfterimageComponent>();
            while (query.MoveNext(out var afterimageUid, out var afterimageComp))
            {
                if (afterimageComp.SourceEntity != sourceUid)
                    continue;

                var despawn = EnsureComp<TimedDespawnComponent>(afterimageUid);
                despawn.Lifetime = 3f;
            }
        });
    }

    #endregion

    #region Audio Methods
    /// <summary>
    /// Play looped audio for sandevistan user
    /// </summary>
    public void PlayLoopedAudio(EntityUid uid, SandevistanUserComponent comp)
    {
        if (!_netManager.IsServer || comp.LoopSound == null || comp.PlayingStream != null)
            return;

        Timer.Spawn(TimeSpan.FromSeconds(comp.LoopSoundDelay), () =>
        {
            if (!Deleted(uid) && comp.Active && comp.PlayingStream == null)
            {
                var stream = _audio.PlayPvs(comp.LoopSound, uid);
                if (stream?.Entity is { } entity)
                    comp.PlayingStream = entity;
            }
        });
    }
    /// <summary>
    /// Stop looped audio for sandevistan user
    /// </summary>
    private void StopLoopedAudio(SandevistanUserComponent comp)
    {
        if (comp.PlayingStream != null)
        {
            _audio.Stop(comp.PlayingStream);
            comp.PlayingStream = null;
        }
    }
    #endregion

    #region Slowfield Methods

    private void OnAmmoShot(Entity<ActiveSandevistanUserComponent> ent, ref AmmoShotUserEvent args)
    {
        if (!TryComp<SandevistanUserComponent>(ent, out var comp) || !comp.SlowfieldEnabled)
            return;

        foreach (var projectile in args.FiredProjectiles)
            ApplySlowdown(ent, projectile, comp);
    }

    private void CreateSlowfieldFixture(EntityUid uid, SandevistanUserComponent comp)
    {
        if (!TryComp<PhysicsComponent>(uid, out var physics))
            return;

        var shape = new PhysShapeCircle(comp.SlowfieldRadius);

        _fixtures.TryCreateFixture(
            uid,
            shape,
            SlowfieldFixtureId,
            collisionLayer: (int) CollisionGroup.ThrownItem,
            collisionMask: (int) (CollisionGroup.MobMask | CollisionGroup.BulletImpassable | CollisionGroup.ThrownItem),
            hard: false,
            body: physics);
    }

    private void DestroySlowfieldFixture(EntityUid uid, SandevistanUserComponent comp)
    {
        if (!TryComp<PhysicsComponent>(uid, out var physics))
            return;

        _fixtures.DestroyFixture(uid, SlowfieldFixtureId, body: physics);
    }

    private void OnStartCollide(Entity<ActiveSandevistanUserComponent> ent, ref StartCollideEvent args)
    {
        if (!TryComp<SandevistanUserComponent>(ent, out var comp) || !comp.SlowfieldEnabled)
            return;

        var target = args.OtherEntity;

        if (args.OurFixtureId != SlowfieldFixtureId
            || target == ent.Owner)
            return;

        ApplySlowdown(ent, target, comp);
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
        if (TryComp<SandevistanSlowedComponent>(target, out var existing) && existing.IsSlowed)
            return;

        if (HasComp<ActiveSandevistanUserComponent>(target))
            return;

        var slowed = EnsureComp<SandevistanSlowedComponent>(target);
        slowed.IsSlowed = true;
        slowed.Source = source;

        // Mobs
        if (HasComp<MobStateComponent>(target))
        {
            slowed.SpeedMultiplier = comp.MobSpeedMultiplier;
            _speed.RefreshMovementSpeedModifiers(target);
            EnsureComp<DogVisionComponent>(target);
        }

        // Bullets
        else if (TryComp<ProjectileComponent>(target, out _))
        {
            slowed.SpeedMultiplier = comp.ProjectileSpeedMultiplier;
            ApplyProjectileSlowdown(target, slowed);
        }

        // Thrown items
        else if (TryComp<ThrownItemComponent>(target, out var thrown))
        {
            slowed.SpeedMultiplier = comp.ThrownItemSpeedMultiplier;
            ApplyThrownItemSlowdown(target, slowed, thrown);
        }

        Dirty(target, slowed);
    }

    private void ApplyProjectileSlowdown(EntityUid target, SandevistanSlowedComponent slowed)
    {
        if (!TryComp<PhysicsComponent>(target, out var physics))
            return;

        slowed.OriginalLinearVelocity = physics.LinearVelocity;
        _physics.SetLinearVelocity(target, physics.LinearVelocity * slowed.SpeedMultiplier, body: physics);
    }

    private void ApplyThrownItemSlowdown(EntityUid target, SandevistanSlowedComponent slowed, ThrownItemComponent thrown)
    {
        if (!TryComp<PhysicsComponent>(target, out var physics))
            return;

        slowed.OriginalLinearVelocity = physics.LinearVelocity;
        _physics.SetLinearVelocity(target, slowed.OriginalLinearVelocity * slowed.SpeedMultiplier, body: physics);

        // Extend LandTime
        // e.g. 95% slower (multiplier 0.05) = 20x longer to land
        if (thrown.LandTime != null && slowed.SpeedMultiplier > 0)
        {
            var remaining = thrown.LandTime.Value - _timing.CurTime;
            thrown.LandTime = _timing.CurTime + remaining / slowed.SpeedMultiplier;
        }
    }

    private void OnRemoveSlowdown(Entity<SandevistanSlowedComponent> ent, ref RemoveSandevistanSlowdownEvent args)
    {
        if (ent.Comp.Source != args.Source)
            return;

        if (!ent.Comp.IsSlowed)
            return;

        ent.Comp.IsSlowed = false;

        // Mobs
        if (HasComp<MobStateComponent>(ent))
        {
            _speed.RefreshMovementSpeedModifiers(ent);
            if (HasComp<DogVisionComponent>(ent))
                RemCompDeferred<DogVisionComponent>(ent);
        }

        // Bullets
        else if (TryComp<PhysicsComponent>(ent, out var physics)
            && ent.Comp.OriginalLinearVelocity.LengthSquared() > 0.01f)
            _physics.SetLinearVelocity(ent, ent.Comp.OriginalLinearVelocity, body: physics);

        // Thrown items
        if (TryComp<ThrownItemComponent>(ent, out var thrown)
            && thrown.LandTime != null
            && ent.Comp.SpeedMultiplier > 0)
        {
            // Convert remaining landtime back to normal speed
            // e.g. item was slowed 95% (mult=0.05), 10s remain in slowfield time = 0.5s at normal speed
            var remainingSlowed = thrown.LandTime.Value - _timing.CurTime;
            thrown.LandTime = _timing.CurTime + remainingSlowed * ent.Comp.SpeedMultiplier;
        }
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

            var targetVelocity = slowed.OriginalLinearVelocity * slowed.SpeedMultiplier;
            if (TryComp<PhysicsComponent>(uid, out var physics)
                && (physics.LinearVelocity - targetVelocity).LengthSquared() > 0.01f)
                _physics.SetLinearVelocity(uid, targetVelocity, body: physics);
        }
    }

    #endregion
}
