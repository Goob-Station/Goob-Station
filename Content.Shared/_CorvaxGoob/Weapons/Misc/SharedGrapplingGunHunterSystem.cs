using System;
using System.Numerics;
using Content.Shared._CorvaxGoob.Weapons.Ranged.Components;
using Content.Shared.CombatMode;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Misc;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics.Joints;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CorvaxGoob.Weapons.Misc;

public abstract class SharedGrapplingGunHunterSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;

    public const string GrapplingJoint = "corvax-goob-hunter-grappling";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrapplingGunHunterComponent, GunShotEvent>(OnGunShot);
        SubscribeLocalEvent<GrapplingGunHunterComponent, ActivateInWorldEvent>(OnGunActivate);
        SubscribeLocalEvent<GrapplingGunHunterComponent, HandDeselectedEvent>(OnGunDeselected);
        SubscribeLocalEvent<GrapplingGunHunterComponent, GotUnequippedHandEvent>(OnGunGotUnequipped);
        SubscribeLocalEvent<GrapplingGunHunterComponent, ItemUnwieldedEvent>(OnGunUnwielded);
        SubscribeLocalEvent<GrapplingGunHunterComponent, JointRemovedEvent>(OnGunJointRemoved);
        SubscribeLocalEvent<GrapplingGunHunterComponent, ComponentShutdown>(OnGunShutdown);
        
        SubscribeLocalEvent<GrapplingHookHunterComponent, ProjectileEmbedEvent>(OnHookEmbed);
        SubscribeLocalEvent<GrapplingHookHunterComponent, ComponentShutdown>(OnHookShutdown);

        SubscribeLocalEvent<GrapplingHookedHunterComponent, DidEquipHandEvent>(OnHookedEquipHand);

        SubscribeAllEvent<RequestGrapplingHunterReelMessage>(OnGrapplingReel);

        SubscribeLocalEvent<PhysicsComponent, StartCollideEvent>(OnPhysicsStartCollide);
    }

    private const string PortalFixtureId = "portalFixture";

    private void OnPhysicsStartCollide(EntityUid uid, PhysicsComponent component, ref StartCollideEvent args)
    {
        if (HasComp<PortalComponent>(uid))
            return;

        if (!args.OtherEntity.Valid || !HasComp<PortalComponent>(args.OtherEntity))
            return;

        if (args.OtherFixtureId != PortalFixtureId)
            return;

        HandleEntityEnteredPortal(uid);
    }

    private void OnGunShot(Entity<GrapplingGunHunterComponent> entity, ref GunShotEvent args)
    {
        var (uid, component) = entity;

        foreach (var (shotUid, _) in args.Ammo)
        {
            if (shotUid is null || !TryComp<GrapplingHookHunterComponent>(shotUid, out var hook))
                continue;

            component.Projectile = shotUid.Value;
            component.HookedTarget = null;
            component.Reeling = false;
            Dirty(uid, component);

            hook.Gun = uid;
            hook.Shooter = args.User;
            hook.Target = null;
            Dirty(shotUid.Value, hook);

            var visuals = EnsureComp<JointVisualsComponent>(shotUid.Value);
            visuals.Sprite = component.RopeSprite;
            visuals.OffsetA = new Vector2(0f, 0.5f);
            visuals.Target = GetNetEntity(uid);
            Dirty(shotUid.Value, visuals);
        }

        component.Stream = _audio.Stop(component.Stream);
        if (TryComp<AppearanceComponent>(uid, out var appearance))
            _appearance.SetData(uid, SharedTetherGunSystem.TetherVisualsStatus.Key, false, appearance);
        else
            _appearance.SetData(uid, SharedTetherGunSystem.TetherVisualsStatus.Key, false);
        Dirty(uid, component);
    }

    private void OnGunActivate(Entity<GrapplingGunHunterComponent> entity, ref ActivateInWorldEvent args)
    {
        var (uid, component) = entity;

        if (!Timing.IsFirstTimePredicted || args.Handled || !args.Complex || component.Projectile is not { })
            return;

        _audio.PlayPredicted(component.CycleSound, uid, args.User);
        ReturnHook(entity, args.User);
        args.Handled = true;
    }

    private void OnGunDeselected(Entity<GrapplingGunHunterComponent> entity, ref HandDeselectedEvent args)
    {
        SetReeling(entity, false, args.User);
    }

    private void OnGunGotUnequipped(Entity<GrapplingGunHunterComponent> entity, ref GotUnequippedHandEvent args)
    {
        var (uid, component) = entity;

        if (component.Projectile == null && component.HookedTarget == null)
            return;

        ReturnHook(entity, args.User);
    }

    private void OnGunUnwielded(Entity<GrapplingGunHunterComponent> entity, ref ItemUnwieldedEvent args)
    {
        var (uid, component) = entity;

        if (!component.RequireWieldedHands)
            return;

        if (component.Projectile == null && component.HookedTarget == null)
            return;

        ReturnHook(entity, args.User);
    }

    private void OnGunJointRemoved(Entity<GrapplingGunHunterComponent> entity, ref JointRemovedEvent args)
    {
        var (uid, component) = entity;

        if (args.Joint.ID != GrapplingJoint)
            return;

        if (component.Projectile == null && component.HookedTarget == null)
            return;

        ReturnHook(entity, null);
    }

    private void OnGunShutdown(Entity<GrapplingGunHunterComponent> entity, ref ComponentShutdown args)
    {
        ReturnHook(entity, null, false);
    }

    private void OnHookEmbed(Entity<GrapplingHookHunterComponent> entity, ref ProjectileEmbedEvent args)
    {
        var (uid, component) = entity;

        if (component.Gun is not { } gun || !TryComp(gun, out GrapplingGunHunterComponent? gunComp))
            return;

        if (component.Target != null)
            return;

        if (!EntityManager.EntityExists(args.Embedded) || args.Embedded == gun)
            return;

        if (!HasComp<MobStateComponent>(args.Embedded))
        {
            ReturnHook((gun, gunComp), null);
            return;
        }

        if (TryComp<GrapplingHookedHunterComponent>(args.Embedded, out var existingHooked))
        {
            if (existingHooked.Hook is not { } existingHook || !EntityManager.EntityExists(existingHook))
            {
                RemCompDeferred<GrapplingHookedHunterComponent>(args.Embedded);
            }
            else if (existingHooked.Gun is { } existingGun && existingGun != gun)
            {
                ReturnHook((gun, gunComp), null);
                return;
            }
        }

        if (!TryComp<PhysicsComponent>(args.Embedded, out var physics) ||
            (physics.BodyType & (BodyType.Dynamic | BodyType.KinematicController)) == 0x0)
        {
            ReturnHook((gun, gunComp), null);
            return;
        }

        component.Target = args.Embedded;
        Dirty(uid, component);

        gunComp.Projectile = uid;
        gunComp.HookedTarget = args.Embedded;
        gunComp.Reeling = false;
        Dirty(gun, gunComp);

        var hooked = EnsureComp<GrapplingHookedHunterComponent>(args.Embedded);
        hooked.Gun = gun;
        hooked.Hook = uid;
        Dirty(args.Embedded, hooked);

        if (gunComp.ApplyStunOnAttach && gunComp.StunDuration > TimeSpan.Zero)
        {
            var stunEvent = new GrapplingHookHunterStunEvent(gun, component.Shooter, gunComp.StunDuration);
            RaiseLocalEvent(args.Embedded, stunEvent);
        }

        var gunXform = Transform(gun);
        var targetXform = Transform(args.Embedded);

        if (gunXform.MapID != targetXform.MapID)
        {
            ReturnHook((gun, gunComp), null);
            return;
        }

        var distance = Vector2.Distance(
            TransformSystem.GetMapCoordinates(gun, gunXform).Position,
            TransformSystem.GetMapCoordinates(args.Embedded, targetXform).Position);

        if (distance > gunComp.MaxRange || distance < gunComp.MinRange)
        {
            ReturnHook((gun, gunComp), null);
            return;
        }
        
        var joint = _joints.CreateDistanceJoint(gun, args.Embedded, anchorA: new Vector2(0f, 0.5f), id: GrapplingJoint);
        joint.MinLength = gunComp.JointMinLength;
        var maxLength = MathF.Min(gunComp.MaxRange, MathF.Max(gunComp.JointMinLength, distance + gunComp.JointSlack));
        joint.MaxLength = maxLength;
        joint.Length = maxLength;
        joint.Stiffness = 1f;

        if (TryComp<JointComponent>(gun, out var jointComp))
            Dirty(gun, jointComp);
    }

    private void OnHookShutdown(Entity<GrapplingHookHunterComponent> entity, ref ComponentShutdown args)
    {
        var (uid, component) = entity;

        if (component.Gun is not { } gun || !TryComp(gun, out GrapplingGunHunterComponent? gunComp))
            return;

        if (gunComp.Projectile != uid)
            return;

        ReturnHook((gun, gunComp), null);
    }

    private void OnHookedEquipHand(Entity<GrapplingHookedHunterComponent> entity, ref DidEquipHandEvent args)
    {
        var (uid, component) = entity;

        if (component.Gun is not { } gun)
            return;

        if (!HasComp<GrapplingGunHunterComponent>(args.Equipped))
            return;

        if (!TryComp(gun, out GrapplingGunHunterComponent? gunComp))
            return;

        if (args.Equipped != gun)
            return;

        ReturnHook((gun, gunComp), args.User);
    }

    private void OnGrapplingReel(RequestGrapplingHunterReelMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player)
            return;

        if (!_hands.TryGetActiveItem(player, out var activeItem) ||
            !TryComp<GrapplingGunHunterComponent>(activeItem, out var grappling))
        {
            return;
        }

        if (grappling.Projectile == null || grappling.HookedTarget == null)
            return;

        if (msg.Reeling &&
            (!TryComp<CombatModeComponent>(player, out var combatMode) ||
             !combatMode.IsInCombatMode))
        {
            return;
        }

        if (msg.Reeling)
        {
            var gunXform = Transform(activeItem.Value);
            var targetXform = Transform(grappling.HookedTarget.Value);

            if (targetXform.MapID != gunXform.MapID)
            {
                return;
            }

            var stopLength = MathF.Max(grappling.JointMinLength, grappling.ReelStopDistance);
            var currentDistance = Vector2.Distance(
                TransformSystem.GetMapCoordinates(activeItem.Value, gunXform).Position,
                TransformSystem.GetMapCoordinates(grappling.HookedTarget.Value, targetXform).Position);

            if (currentDistance <= stopLength + grappling.PullStopTolerance)
            {
                return;
            }
        }

        SetReeling((activeItem.Value, grappling), msg.Reeling, player);
    }

    public void HandleEntityEnteredPortal(EntityUid subject)
    {
        if (!EntityManager.EntityExists(subject))
            return;

        if (TryComp<GrapplingHookedHunterComponent>(subject, out var hooked) &&
            hooked.Gun is { } hookedGun &&
            TryComp(hookedGun, out GrapplingGunHunterComponent? hookedGunComp))
        {
            ReturnHook((hookedGun, hookedGunComp), null);
        }

        if (TryComp(subject, out HandsComponent? hands))
        {
            foreach (var held in _hands.EnumerateHeld((subject, hands)))
            {
                if (!TryComp<GrapplingGunHunterComponent>(held, out var heldGunComp))
                    continue;

                if (heldGunComp.Projectile == null && heldGunComp.HookedTarget == null)
                    continue;

                ReturnHook((held, heldGunComp), null);
            }
        }

        if (TryComp<GrapplingHookHunterComponent>(subject, out var hook) &&
            hook.Gun is { } gun &&
            TryComp(gun, out GrapplingGunHunterComponent? ownerGunComp))
        {
            ReturnHook((gun, ownerGunComp), null);
        }
    }

    private void SetReeling(Entity<GrapplingGunHunterComponent> entity, bool value, EntityUid? user)
    {
        var (uid, component) = entity;

        if (component.Reeling == value)
            return;

        if (value)
        {
            if (Timing.IsFirstTimePredicted)
                component.Stream = _audio.PlayPredicted(component.ReelSound, uid, user)?.Entity;
        }
        else
        {
            if (Timing.IsFirstTimePredicted)
                component.Stream = _audio.Stop(component.Stream);
        }

        component.Reeling = value;
        Dirty(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GrapplingGunHunterComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.Reeling && Timing.IsFirstTimePredicted && component.Stream != null)
            {
                component.Stream = _audio.Stop(component.Stream);
            }

            if (component.Projectile == null)
            {
                if (component.Reeling)
                    SetReeling((uid, component), false, null);
                continue;
            }

            if (!EntityManager.EntityExists(component.Projectile.Value))
            {
                ReturnHook((uid, component), null);
                continue;
            }

            if (component.HookedTarget == null)
            {
                if (component.Reeling)
                    SetReeling((uid, component), false, null);

                var gunXform = Transform(uid);
                var projectileXform = Transform(component.Projectile.Value);

                if (projectileXform.MapID != gunXform.MapID)
                {
                    ReturnHook((uid, component), null);
                    continue;
                }

                var distance = Vector2.Distance(
                    TransformSystem.GetMapCoordinates(uid, gunXform).Position,
                    TransformSystem.GetMapCoordinates(component.Projectile.Value, projectileXform).Position);

                if (distance > component.MaxRange)
                    ReturnHook((uid, component), null);

                continue;
            }

            if (!EntityManager.EntityExists(component.HookedTarget.Value))
            {
                ReturnHook((uid, component), null);
                continue;
            }

            if (!TryComp<JointComponent>(uid, out var jointComp) ||
                !jointComp.GetJoints.TryGetValue(GrapplingJoint, out var joint) ||
                joint is not DistanceJoint distanceJoint)
            {
                SetReeling((uid, component), false, null);
                continue;
            }

            if (!component.Reeling)
                continue;

            var reelGunXform = Transform(uid);
            var targetXform = Transform(component.HookedTarget.Value);

            if (targetXform.MapID != reelGunXform.MapID)
            {
                ReturnHook((uid, component), null);
                continue;
            }

            var stopLength = MathF.Max(component.JointMinLength, component.ReelStopDistance);
            var currentDistance = Vector2.Distance(
                TransformSystem.GetMapCoordinates(uid, reelGunXform).Position,
                TransformSystem.GetMapCoordinates(component.HookedTarget.Value, targetXform).Position);

            if (currentDistance <= component.ReelStopDistance)
            {
                SetReeling((uid, component), false, null);
                continue;
            }

            distanceJoint.MaxLength = MathF.Max(stopLength, distanceJoint.MaxLength - component.ReelRate * frameTime);
            distanceJoint.Length = MathF.Min(distanceJoint.MaxLength, distanceJoint.Length);

            _physics.WakeBody(joint.BodyAUid);
            _physics.WakeBody(joint.BodyBUid);

            Dirty(uid, jointComp);

            if (distanceJoint.MaxLength <= stopLength + component.PullStopTolerance)
            {
                SetReeling((uid, component), false, null);
            }
        }
    }

    private void ReturnHook(Entity<GrapplingGunHunterComponent> entity, EntityUid? user, bool restoreAmmo = true)
    {
        var (uid, component) = entity;
        var projectile = component.Projectile;
        var target = component.HookedTarget;
        var hadProjectile = projectile is not null;
        var showLoaded = restoreAmmo && hadProjectile;

        SetReeling(entity, false, user);

        component.Projectile = null;
        component.HookedTarget = null;
        Dirty(uid, component);

        if (TryComp<JointComponent>(uid, out var jointComp) && jointComp.GetJoints.ContainsKey(GrapplingJoint))
        {
            _joints.RemoveJoint(uid, GrapplingJoint);
        }

        if (target is { } targetUid && EntityManager.EntityExists(targetUid) &&
            TryComp<GrapplingHookedHunterComponent>(targetUid, out var hooked) &&
            hooked.Gun == uid)
        {
            RemCompDeferred<GrapplingHookedHunterComponent>(targetUid);
        }

        if (TryComp<AppearanceComponent>(uid, out var appearance))
            _appearance.SetData(uid, SharedTetherGunSystem.TetherVisualsStatus.Key, showLoaded, appearance);
        else
            _appearance.SetData(uid, SharedTetherGunSystem.TetherVisualsStatus.Key, showLoaded);

        if (_netManager.IsServer && projectile is { } proj && EntityManager.EntityExists(proj))
        {
            QueueDel(proj);
        }

        if (restoreAmmo && hadProjectile)
        {
            _gun.ChangeBasicEntityAmmoCount(uid, 1);

            var updateAmmoEvent = new UpdateClientAmmoEvent();
            RaiseLocalEvent(uid, ref updateAmmoEvent);
        }
    }

    [Serializable, NetSerializable]
    protected sealed class RequestGrapplingHunterReelMessage : EntityEventArgs
    {
        public bool Reeling;

        public RequestGrapplingHunterReelMessage(bool reeling)
        {
            Reeling = reeling;
        }
    }
}

public sealed class GrapplingHookHunterStunEvent : HandledEntityEventArgs
{
    public EntityUid Gun;
    public EntityUid? Shooter;
    public TimeSpan Duration;

    public GrapplingHookHunterStunEvent(EntityUid gun, EntityUid? shooter, TimeSpan duration)
    {
        Gun = gun;
        Shooter = shooter;
        Duration = duration;
    }
}
