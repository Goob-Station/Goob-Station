using System.Numerics;
using Content.Goobstation.Common.Projectiles;
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared._Lavaland.Weapons.Ranged.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Trauma.Projectiles;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Projectiles;
using Content.Shared.Random.Helpers;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Ranged.Systems;

/// <summary>
/// Trauma - shooting moved out of server so it can be predicted.
/// </summary>
public abstract partial class SharedGunSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    /// <summary>
    /// Get a predicted random instance for an entity, specific to this tick.
    /// </summary>
    protected IRobustRandom GetRandom(EntityUid uid, EntityUid? other = null)
        => SharedRandomExtensions.PredictedRandom(Timing, GetNetEntity(uid), GetNetEntity(other));

    /// <summary>
    /// Goob - mark who fired a projectile or hitscan, see <see cref="PredictedShooterComponent"/>.
    /// </summary>
    private void SetPredictedShooter(EntityUid uid, EntityUid shooter)
    {
        var comp = EnsureComp<PredictedShooterComponent>(uid);
        comp.Shooter = shooter;
        Dirty(uid, comp);
    }

    /// <summary>
    /// Client-overriden function to do camera recoil for a shot.
    /// Shooting is fully predicted so server doesn't need to do anything.
    /// </summary>
    protected virtual void Recoil(EntityUid? user, Vector2 recoil, float recoilScalar)
    {
    }

    /// <summary>
    /// Trauma - moved server version here and predicted it
    /// </summary>
    public void Shoot(
        Entity<GunComponent> gun,
        List<(EntityUid? Entity, IShootable Shootable)> ammo,
        EntityCoordinates fromCoordinates,
        EntityCoordinates toCoordinates,
        out bool userImpulse,
        EntityUid? user = null,
        bool throwItems = false)
    {
        userImpulse = false;

        if (user != null)
        {
            var selfEvent = new SelfBeforeGunShotEvent(user.Value, gun, ammo);
            RaiseLocalEvent(user.Value, selfEvent);
            if (selfEvent.Cancelled)
                return;
        }

        var fromMap = TransformSystem.ToMapCoordinates(fromCoordinates);
        var toMap = TransformSystem.ToMapCoordinates(toCoordinates).Position;
        var mapDirection = toMap - fromMap.Position;
        // prevent shooting with 0,0 direction
        if (mapDirection == Vector2.Zero)
            return;

        var mapAngle = mapDirection.ToAngle();
        var angle = GetRecoilAngle(Timing.CurTime, gun, mapDirection.ToAngle(), user); // Goobstation user

        userImpulse = true;

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var fromEnt = MapSystem.TryFindGridAt(fromMap, out var gridUid, out _)
            ? TransformSystem.WithEntityId(fromCoordinates, gridUid)
            : new EntityCoordinates(MapSystem.GetMapOrInvalid(fromMap.MapId), fromMap.Position);

        var toMapBeforeRecoil = toMap; // Goobstation

        // Update shot based on the recoil
        toMap = fromMap.Position + angle.ToVec() * mapDirection.Length();
        mapDirection = toMap - fromMap.Position;
        var gunVelocity = Physics.GetMapLinearVelocity(fromEnt);

        // camera recoil goes opposite of where you shoot
        var recoilDirection = -mapDirection;

        // I must be high because this was getting tripped even when true.
        // DebugTools.Assert(direction != Vector2.Zero);
        var shotProjectiles = new List<EntityUid>(ammo.Count);
        var shotRandom = GetRandom(gun, user);

        foreach (var (ent, shootable) in ammo)
        {
            // pneumatic cannon doesn't shoot bullets it just throws them, ignore ammo handling
            if (throwItems && ent != null)
            {
                Recoil(user, recoilDirection, gun.Comp.CameraRecoilScalarModified);
                ShootOrThrow(ent.Value, mapDirection, gunVelocity, gun, user,
                    targetCoordinates: toMapBeforeRecoil); // Goobstation
                shotProjectiles.Add(ent.Value); // Goobstation
                continue;
            }

            // TODO: Clean this up in a gun refactor at some point - too much copy pasting
            switch (shootable)
            {
                // Cartridge shoots something else
                case CartridgeAmmoComponent cartridge:
                    if (!cartridge.Spent)
                    {
                        var uid = PredictedSpawnAtPosition(cartridge.Prototype, fromEnt);
                        CreateAndFireProjectiles(uid, cartridge);
                        Recoil(user, recoilDirection, gun.Comp.CameraRecoilScalarModified);

                        RaiseLocalEvent(ent!.Value, new AmmoShotEvent()
                        {
                            FiredProjectiles = shotProjectiles
                        });

                        SetCartridgeSpent(ent.Value, cartridge, true);

                        if (cartridge.DeleteOnSpawn)
                            PredictedDel(ent.Value);
                    }
                    else
                    {
                        userImpulse = false;
                        Audio.PlayPredicted(gun.Comp.SoundEmpty, gun, user);
                    }

                    // Something like ballistic might want to leave it in the container still
                    if (!cartridge.DeleteOnSpawn && !Containers.IsEntityInContainer(ent!.Value))
                        EjectCartridge(shotRandom, user, ent.Value, angle);

                    Dirty(ent!.Value, cartridge);
                    break;
                // Ammo shoots itself
                case AmmoComponent newAmmo:
                    if (ent == null)
                        break;
                    CreateAndFireProjectiles(ent.Value, newAmmo);
                    Recoil(user, recoilDirection, gun.Comp.CameraRecoilScalarModified);

                    break;
                case HitscanAmmoComponent:
                    if (ent == null)
                        break;

                    // Goob - server uses this to not send the shooter effects they already predicted
                    if (user != null)
                        SetPredictedShooter(ent.Value, user.Value);

                    var hitscanEv = new HitscanTraceEvent
                    {
                        FromCoordinates = fromCoordinates,
                        ShotDirection = mapDirection.Normalized(),
                        Gun = gun,
                        Shooter = user,
                        Target = gun.Comp.Target,
                    };
                    RaiseLocalEvent(ent.Value, ref hitscanEv);

                    PredictedDel(ent.Value);

                    Audio.PlayPredicted(gun.Comp.SoundGunshotModified, gun, user);
                    Recoil(user, recoilDirection, gun.Comp.CameraRecoilScalarModified);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        RaiseLocalEvent(gun, new AmmoShotEvent()
        {
            FiredProjectiles = shotProjectiles,
        });

        // Goobstation start
        if (user.HasValue)
            RaiseLocalEvent(user.Value, new AmmoShotUserEvent()
            {
                Gun = gun,
                FiredProjectiles = shotProjectiles,
            });
        // Goobstation end

        void CreateAndFireProjectiles(EntityUid ammoEnt, AmmoComponent ammoComp)
        {
            if (TryComp<ProjectileSpreadComponent>(ammoEnt, out var ammoSpreadComp))
            {
                var spreadEvent = new GunGetAmmoSpreadEvent(ammoSpreadComp.Spread);
                RaiseLocalEvent(gun, ref spreadEvent);

                var angles = LinearSpread(mapAngle - spreadEvent.Spread / 2,
                    mapAngle + spreadEvent.Spread / 2, ammoSpreadComp.Count);

                ShootOrThrow(ammoEnt, angles[0].ToVec(), gunVelocity, gun, user,
                    targetCoordinates: toMapBeforeRecoil); // Goobstation
                shotProjectiles.Add(ammoEnt);

                for (var i = 1; i < ammoSpreadComp.Count; i++)
                {
                    var newuid = PredictedSpawnAtPosition(ammoSpreadComp.Proto, fromEnt);
                    // Lavaland Change: Raise event when a projectile/pellet is fired from a gun.
                    RaiseLocalEvent(gun, new ProjectileShotEvent()
                    {
                        FiredProjectile = newuid
                    });
                    SetProjectilePerfectHitEntities(newuid, user, new MapCoordinates(toMap, fromMap.MapId), shotRandom);
                    // Lavaland end
                    ShootOrThrow(newuid, angles[i].ToVec(), gunVelocity, gun, user,
                        targetCoordinates: toMapBeforeRecoil); // Goob
                    shotProjectiles.Add(newuid);
                }
            }
            else
            {
                ShootOrThrow(ammoEnt, mapDirection, gunVelocity, gun, user,
                    targetCoordinates: toMapBeforeRecoil); // Goobstation
                shotProjectiles.Add(ammoEnt);
            }

            MuzzleFlash(gun, ammoComp, mapDirection.ToAngle(), user);
            Audio.PlayPredicted(gun.Comp.SoundGunshotModified, gun, user);
        }
    }

    // Goobstation start
    public TargetBodyPart? GetTargetPart(Entity<TargetingComponent?>? ent,
        MapCoordinates shootCoords,
        MapCoordinates targetCoords,
        IRobustRandom rand) // Trauma - predicted random
    {
        if (shootCoords.MapId != targetCoords.MapId || ent == null)
            return null;

        var targeting = ent.Value;

        if (!Resolve(targeting, ref targeting.Comp, false))
            return null;

        var dist = (shootCoords.Position - targetCoords.Position).Length();
        var missChance = MathHelper.Lerp(0f, 1f, Math.Clamp(dist / 2f, 0f, 1f));
        return rand.Prob(missChance) ? TargetBodyPart.Chest : targeting.Comp.Target;
    }

    private void SetProjectilePerfectHitEntities(EntityUid projectile,
        Entity<TargetingComponent?>? shooter,
        MapCoordinates coords,
        IRobustRandom rand) // Trauma - predicted random
    {
        if (shooter == null)
            return;

        var ent = shooter.Value;

        if (!Resolve(ent, ref ent.Comp, false))
            return;

        var comp = EnsureComp<ProjectileMissTargetPartChanceComponent>(projectile);
        var look = _lookup.GetEntitiesInRange<BodyComponent>(coords, 2f, LookupFlags.Dynamic);
        foreach (var (uid, body) in look)
        {
            if (body.BodyType != _Shitmed.Body.BodyType.Complex)
                continue;

            var part = GetTargetPart(shooter, coords, TransformSystem.GetMapCoordinates(ent), rand);

            if (part is null or TargetBodyPart.Chest)
                continue;

            comp.PerfectHitEntities.Add(uid);
        }
    }
    // Goobstation end

    private void ShootOrThrow(EntityUid uid, Vector2 mapDirection, Vector2 gunVelocity, Entity<GunComponent> gun, EntityUid? user,
        Vector2? targetCoordinates = null) // Goobstation
    {
        if (gun.Comp.Target is { } target && !TerminatingOrDeleted(target))
        {
            var targeted = EnsureComp<TargetedProjectileComponent>(uid);
            targeted.Target = target;
            Dirty(uid, targeted);
        }

        // Do a throw
        if (!HasComp<ProjectileComponent>(uid))
        {
            RemoveShootable(uid);
            // TODO: Someone can probably yeet this a billion miles so need to pre-validate input somewhere up the call stack.
            ThrowingSystem.TryThrow(uid, mapDirection, gun.Comp.ProjectileSpeedModified, user);
            return;
        }

        // Goob - server uses this to not send the shooter effects they already predicted
        if (user != null)
            SetPredictedShooter(uid, user.Value);

        ShootProjectile(uid, mapDirection, gunVelocity, gun, user, gun.Comp.ProjectileSpeedModified,
            targetCoordinates); // Goobstation
    }

    /// <summary>
    /// Gets a linear spread of angles between start and end.
    /// </summary>
    /// <param name="start">Start angle in degrees</param>
    /// <param name="end">End angle in degrees</param>
    /// <param name="intervals">How many shots there are</param>
    public Angle[] LinearSpread(Angle start, Angle end, int intervals) // Goob edit
    {
        var angles = new Angle[intervals];
        DebugTools.Assert(intervals > 1);

        for (var i = 0; i <= intervals - 1; i++)
        {
            angles[i] = new Angle(start + (end - start) * i / (intervals - 1));
        }

        return angles;
    }

    private Angle GetRecoilAngle(TimeSpan curTime, Entity<GunComponent> gun, Angle direction, EntityUid? user = null) // Goobstation user
    {
        var component = gun.Comp;
        var timeSinceLastFire = (curTime - component.LastFire).TotalSeconds;
        var minTheta = Math.Min(component.MinAngleModified.Theta, component.MaxAngleModified.Theta); // goob edit make min max work properly
        var maxTheta = Math.Max(component.MinAngleModified.Theta, component.MaxAngleModified.Theta); // goob edit reverse recoil direction for funny mechanics
        var newTheta = MathHelper.Clamp(component.CurrentAngle.Theta + component.AngleIncreaseModified.Theta - component.AngleDecayModified.Theta * timeSinceLastFire, minTheta, maxTheta); // goob edit
        component.CurrentAngle = new Angle(newTheta);
        component.LastFire = component.NextFire;

        // Convert it so angle can go either side.
        var random = GetRandom(gun).NextFloat(-0.5f, 0.5f); // Trauma - predicted random

        // Goobstation start
        var angleEv = new GetRecoilModifiersEvent()
        {
            Gun = gun,
            User = user ?? gun
        };
        if (user != null)
            RaiseLocalEvent(user.Value, angleEv);
        RaiseLocalEvent(gun, angleEv);
        random *= angleEv.Modifier;
        // Goobstation end

        var spread = component.CurrentAngle.Theta * random;
        var angle = new Angle(direction.Theta + component.CurrentAngle.Theta * random);
        DebugTools.Assert(Math.Abs(spread) <= maxTheta * Math.Max(1f, Math.Abs(angleEv.Modifier))); // goob edit
        return angle;
    }

    /// <summary>
    /// Trauma - moved server implementation here and predicted it
    /// </summary>
    protected void Cycle(Entity<BallisticAmmoProviderComponent> ent, MapCoordinates coordinates, EntityUid? user = null)
    {
        // TODO: Combine with TakeAmmo
        if (ent.Comp.Entities.Count > 0)
        {
            var existing = ent.Comp.Entities[^1];
            ent.Comp.Entities.RemoveAt(ent.Comp.Entities.Count - 1);
            DirtyField(ent.AsNullable(), nameof(BallisticAmmoProviderComponent.Entities));

            Containers.Remove(existing, ent.Comp.Container);
            EnsureShootable(existing);
        }
        else if (ent.Comp.UnspawnedCount > 0)
        {
            ent.Comp.UnspawnedCount--;
            DirtyField(ent.AsNullable(), nameof(BallisticAmmoProviderComponent.UnspawnedCount));
            var ammoEnt = EntityManager.PredictedSpawn(ent.Comp.Proto, coordinates);
            EnsureShootable(ammoEnt);
            EjectCartridge(GetRandom(ent), user, ammoEnt);
        }

        var cycledEvent = new GunCycledEvent();
        RaiseLocalEvent(ent, ref cycledEvent);
    }

    /// <summary>
    /// Trauma - made concrete and predicted.
    /// Goob - plays like PlayPredicted, server sends it to everyone but <paramref name="predictingUser"/>,
    /// who heard it from predicting the hit.
    /// </summary>
    public void PlayImpactSound(EntityUid otherEntity, DamageSpecifier? modifiedDamage, SoundSpecifier? weaponSound, bool forceWeaponSound, EntityUid? predictingUser = null)
    {
        DebugTools.Assert(!Deleted(otherEntity), "Impact sound entity was deleted");

        // Like projectiles and melee,
        // 1. Entity specific sound
        // 2. Ammo's sound
        // 3. Nothing
        if (!forceWeaponSound && modifiedDamage != null && modifiedDamage.GetTotal() > 0 && TryComp<RangedDamageSoundComponent>(otherEntity, out var rangedSound))
        {
            var type = SharedMeleeWeaponSystem.GetHighestDamageSound(modifiedDamage, ProtoManager);

            if (type != null && rangedSound.SoundTypes?.TryGetValue(type, out var damageSoundType) == true)
            {
                Audio.PlayPredicted(damageSoundType, otherEntity, predictingUser, AudioParams.Default.WithVariation(MeleeSoundSystem.DamagePitchVariation));
                return;
            }

            if (type != null && rangedSound.SoundGroups?.TryGetValue(type, out var damageSoundGroup) == true)
            {
                Audio.PlayPredicted(damageSoundGroup, otherEntity, predictingUser, AudioParams.Default.WithVariation(MeleeSoundSystem.DamagePitchVariation));
                return;
            }
        }

        Audio.PlayPredicted(weaponSound, otherEntity, predictingUser);
    }
}
