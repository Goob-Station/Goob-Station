// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.Projectiles; // Goobstation
using Content.Shared.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Cargo.Systems;
using Content.Server.Power.EntitySystems;
using Content.Server.Weapons.Ranged.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Reflect;
using Content.Shared.Damage.Components;
using Robust.Shared.Audio;
using Robust.Shared.Configuration; // Goobstation
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Containers;
using Content.Server.PowerCell;
using Content.Shared._Lavaland.Weapons.Ranged.Events; // Lavaland Change
using Robust.Server.GameObjects; // Goobstation
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Robust.Shared.Random; // Lavaland Change

namespace Content.Server.Weapons.Ranged.Systems;

public sealed partial class GunSystem : SharedGunSystem
{
    [Dependency] private readonly DamageExamineSystem _damageExamine = default!;
    [Dependency] private readonly PricingSystem _pricing = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    // Goobstation
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private const float DamagePitchVariation = 0.05f;
    private float _crawlHitzoneSize; // Goobstation

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BallisticAmmoProviderComponent, PriceCalculationEvent>(OnBallisticPrice);
        _cfg.OnValueChanged(GoobCVars.CrawlHitzoneSize, value => _crawlHitzoneSize = value, true); // Goobstation
    }

    private void OnBallisticPrice(EntityUid uid, BallisticAmmoProviderComponent component, ref PriceCalculationEvent args)
    {
        if (string.IsNullOrEmpty(component.Proto) || component.UnspawnedCount == 0)
            return;

        if (!ProtoManager.TryIndex<EntityPrototype>(component.Proto, out var proto))
        {
            Log.Error($"Unable to find fill prototype for price on {component.Proto} on {ToPrettyString(uid)}");
            return;
        }

        // Probably good enough for most.
        var price = _pricing.GetEstimatedPrice(proto);
        args.Price += price * component.UnspawnedCount;
    }

    public override void Shoot(EntityUid gunUid, GunComponent gun, List<(EntityUid? Entity, IShootable Shootable)> ammo,
        EntityCoordinates fromCoordinates, EntityCoordinates toCoordinates, out bool userImpulse, EntityUid? user = null, bool throwItems = false)
    {
        userImpulse = true;

        if (user != null)
        {
            var selfEvent = new SelfBeforeGunShotEvent(user.Value, (gunUid, gun), ammo);
            RaiseLocalEvent(user.Value, selfEvent);
            if (selfEvent.Cancelled)
            {
                userImpulse = false;
                return;
            }
        }

        var fromMap = TransformSystem.ToMapCoordinates(fromCoordinates);
        var toMap = TransformSystem.ToMapCoordinates(toCoordinates).Position;
        var mapDirection = toMap - fromMap.Position;
        var mapAngle = mapDirection.ToAngle();
        var angle = GetRecoilAngle(Timing.CurTime, gun, mapDirection.ToAngle(), user);  // Goobstation user

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var fromEnt = MapManager.TryFindGridAt(fromMap, out var gridUid, out _)
            ? TransformSystem.WithEntityId(fromCoordinates, gridUid)
            : new EntityCoordinates(_map.GetMapOrInvalid(fromMap.MapId), fromMap.Position);

        var toMapBeforeRecoil = toMap; // Goobstation

        // Update shot based on the recoil
        toMap = fromMap.Position + angle.ToVec() * mapDirection.Length();
        mapDirection = toMap - fromMap.Position;
        var gunVelocity = Physics.GetMapLinearVelocity(fromEnt);

        // I must be high because this was getting tripped even when true.
        // DebugTools.Assert(direction != Vector2.Zero);
        var shotProjectiles = new List<EntityUid>(ammo.Count);

        foreach (var (ent, shootable) in ammo)
        {
            // pneumatic cannon doesn't shoot bullets it just throws them, ignore ammo handling
            if (throwItems && ent != null)
            {
                ShootOrThrow(ent.Value, mapDirection, gunVelocity, gun, gunUid, user, targetCoordinates: toMapBeforeRecoil);
                shotProjectiles.Add(ent.Value); // Goobstation
                continue;
            }

            switch (shootable)
            {
                // Cartridge shoots something else
                case CartridgeAmmoComponent cartridge:
                    if (!cartridge.Spent)
                    {
                        var uid = Spawn(cartridge.Prototype, fromEnt);
                        CreateAndFireProjectiles(uid, cartridge);

                        RaiseLocalEvent(ent!.Value, new AmmoShotEvent()
                        {
                            FiredProjectiles = shotProjectiles
                        });

                        SetCartridgeSpent(ent.Value, cartridge, true);

                        if (cartridge.DeleteOnSpawn)
                            Del(ent.Value);
                    }
                    else
                    {
                        userImpulse = false;
                        Audio.PlayPredicted(gun.SoundEmpty, gunUid, user);
                    }

                    // Something like ballistic might want to leave it in the container still
                    if (!cartridge.DeleteOnSpawn && !Containers.IsEntityInContainer(ent!.Value))
                        EjectCartridge(ent.Value, angle);

                    Dirty(ent!.Value, cartridge);
                    break;
                // Ammo shoots itself
                case AmmoComponent newAmmo:
                    if (ent == null)
                        break;
                    CreateAndFireProjectiles(ent.Value, newAmmo);

                    break;
                case HitscanPrototype hitscan:

                    EntityUid? lastHit = null;

                    var from = fromMap;
                    // can't use map coords above because funny FireEffects
                    var fromEffect = fromCoordinates;
                    var dir = mapDirection.Normalized();

                    //in the situation when user == null, means that the cannon fires on its own (via signals). And we need the gun to not fire by itself in this case
                    var lastUser = user ?? gunUid;

                    if (hitscan.Reflective != ReflectType.None)
                    {
                        for (var reflectAttempt = 0; reflectAttempt < 3; reflectAttempt++)
                        {
                            var ray = new CollisionRay(from.Position, dir, hitscan.CollisionMask);
                            var rayCastResults =
                                Physics.IntersectRay(from.MapId, ray, hitscan.MaxLength, lastUser, false).ToList();
                            if (!rayCastResults.Any())
                                break;

                            var result = rayCastResults[0];

                            // Check if laser is shot from in a container
                            if (!_container.IsEntityOrParentInContainer(lastUser))
                            {
                                // Checks if the laser should pass over unless targeted by its user
                                foreach (var collide in rayCastResults)
                                {
                                    if (collide.HitEntity != gun.Target &&
                                        CompOrNull<RequireProjectileTargetComponent>(collide.HitEntity)?.Active == true &&
                                        (_transform.GetMapCoordinates(collide.HitEntity).Position - toMapBeforeRecoil).Length() > _crawlHitzoneSize)
                                    {
                                        continue;
                                    }

                                    result = collide;
                                    break;
                                }
                            }

                            var hit = result.HitEntity;
                            lastHit = hit;

                            FireEffects(fromEffect, result.Distance, dir.Normalized().ToAngle(), hitscan, hit);

                            var ev = new HitScanReflectAttemptEvent(user, gunUid, hitscan.Reflective, dir, false, hitscan.Damage); // WD EDIT
                            RaiseLocalEvent(hit, ref ev);

                            if (!ev.Reflected)
                                break;

                            fromEffect = Transform(hit).Coordinates;
                            from = TransformSystem.ToMapCoordinates(fromEffect);
                            dir = ev.Direction;
                            lastUser = hit;
                        }
                    }

                    if (lastHit != null)
                    {
                        var hitEntity = lastHit.Value;
                        if (hitscan.StaminaDamage > 0f)
                            _stamina.TakeStaminaDamage(hitEntity, hitscan.StaminaDamage, source: user, applyResistances: true); // Goob edit

                        if (hitscan.FireStacks > 0f && TryComp(hitEntity, out FlammableComponent? flammable)) // Goobstation
                            _flammable.AdjustFireStacks(hitEntity, hitscan.FireStacks, flammable, true);

                        var dmg = hitscan.Damage;

                        var hitName = ToPrettyString(hitEntity);
                        // Goob edit start
                        if (dmg != null)
                        {
                            dmg = Damageable.TryChangeDamage(hitEntity,
                                dmg * Damageable.UniversalHitscanDamageModifier,
                                origin: user,
                                targetPart: GetTargetPart(lastUser,
                                    new MapCoordinates(toMap, fromMap.MapId),
                                    _transform.GetMapCoordinates(hitEntity)),
                                canBeCancelled: true); // Shitmed Change
                        }
                        // Goob edit end

                        // check null again, as TryChangeDamage returns modified damage values
                        if (dmg != null)
                        {
                            if (!Deleted(hitEntity))
                            {
                                if (dmg.AnyPositive())
                                {
                                    _color.RaiseEffect(Color.Red, new List<EntityUid>() { hitEntity }, Filter.Pvs(hitEntity, entityManager: EntityManager));
                                }

                                // TODO get fallback position for playing hit sound.
                                PlayImpactSound(hitEntity, dmg, hitscan.Sound, hitscan.ForceSound);
                            }

                            if (user != null)
                            {
                                Logs.Add(LogType.HitScanHit,
                                    $"{ToPrettyString(user.Value):user} hit {hitName:target} using hitscan and dealt {dmg.GetTotal():damage} damage");
                            }
                            else
                            {
                                Logs.Add(LogType.HitScanHit,
                                    $"{hitName:target} hit by hitscan dealing {dmg.GetTotal():damage} damage");
                            }
                        }
                    }
                    else
                    {
                        FireEffects(fromEffect, hitscan.MaxLength, dir.ToAngle(), hitscan);
                    }

                    Audio.PlayPredicted(gun.SoundGunshotModified, gunUid, user);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        RaiseLocalEvent(gunUid, new AmmoShotEvent()
        {
            FiredProjectiles = shotProjectiles,
        });

        // Goobstation start
        if (user.HasValue)
            RaiseLocalEvent(user.Value, new AmmoShotUserEvent()
            {
                Gun = gunUid,
                FiredProjectiles = shotProjectiles,
            });
        // Goobstation end

        void CreateAndFireProjectiles(EntityUid ammoEnt, AmmoComponent ammoComp)
        {
            if (TryComp<ProjectileSpreadComponent>(ammoEnt, out var ammoSpreadComp))
            {
                var spreadEvent = new GunGetAmmoSpreadEvent(ammoSpreadComp.Spread);
                RaiseLocalEvent(gunUid, ref spreadEvent);

                var angles = LinearSpread(mapAngle - spreadEvent.Spread / 2,
                    mapAngle + spreadEvent.Spread / 2, ammoSpreadComp.Count);

                ShootOrThrow(ammoEnt, angles[0].ToVec(), gunVelocity, gun, gunUid, user, targetCoordinates: toMapBeforeRecoil); // Goobstation
                shotProjectiles.Add(ammoEnt);

                for (var i = 1; i < ammoSpreadComp.Count; i++)
                {
                    var newuid = Spawn(ammoSpreadComp.Proto, fromEnt);
                    // Lavaland Change: Raise event when a projectile/pellet is fired from a gun.
                    RaiseLocalEvent(gunUid, new ProjectileShotEvent()
                    {
                        FiredProjectile = newuid
                    });
                    SetProjectilePerfectHitEntities(newuid, user, new MapCoordinates(toMap, fromMap.MapId));
                    ShootOrThrow(newuid, angles[i].ToVec(), gunVelocity, gun, gunUid, user, targetCoordinates: toMapBeforeRecoil); // Goobstation
                    shotProjectiles.Add(newuid);
                }
            }
            else
            {
                ShootOrThrow(ammoEnt, mapDirection, gunVelocity, gun, gunUid, user, targetCoordinates: toMapBeforeRecoil); // Goobstation
                shotProjectiles.Add(ammoEnt);
            }

            MuzzleFlash(gunUid, ammoComp, mapDirection.ToAngle(), user);
            Audio.PlayPredicted(gun.SoundGunshotModified, gunUid, user);
        }
    }

    // Goobstation start
    public TargetBodyPart? GetTargetPart(Entity<TargetingComponent?>? ent,
        MapCoordinates shootCoords,
        MapCoordinates targetCoords)
    {
        if (shootCoords.MapId != targetCoords.MapId || ent == null)
            return null;

        var targeting = ent.Value;

        if (!Resolve(targeting, ref targeting.Comp, false))
            return null;

        var dist = (shootCoords.Position - targetCoords.Position).Length();
        var missChance = MathHelper.Lerp(0f, 1f, Math.Clamp(dist / 2f, 0f, 1f));
        return Random.Prob(missChance) ? TargetBodyPart.Chest : targeting.Comp.Target;
    }

    private void SetProjectilePerfectHitEntities(EntityUid projectile,
        Entity<TargetingComponent?>? shooter,
        MapCoordinates coords)
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
            if (body.BodyType != Shared._Shitmed.Body.BodyType.Complex)
                continue;

            var part = GetTargetPart(shooter, coords, _transform.GetMapCoordinates(ent));

            if (part is null or TargetBodyPart.Chest)
                continue;

            comp.PerfectHitEntities.Add(uid);
        }
    }
    // Goobstation end

    private void ShootOrThrow(EntityUid uid, Vector2 mapDirection, Vector2 gunVelocity, GunComponent gun, EntityUid gunUid, EntityUid? user, Vector2? targetCoordinates = null) // Goobstation
    {
        if (gun.Target is { } target && !TerminatingOrDeleted(target))
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
            ThrowingSystem.TryThrow(uid, mapDirection, gun.ProjectileSpeedModified, user);
            return;
        }

        ShootProjectile(uid, mapDirection, gunVelocity, gunUid, user, gun.ProjectileSpeedModified, targetCoordinates); // Goobstation
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

    private Angle GetRecoilAngle(TimeSpan curTime, GunComponent component, Angle direction, EntityUid? user = null) // Goobstation user
    {
        var timeSinceLastFire = (curTime - component.LastFire).TotalSeconds;
        var minTheta = Math.Min(component.MinAngleModified.Theta, component.MaxAngleModified.Theta); // goob edit make min max work properly
        var maxTheta = Math.Max(component.MinAngleModified.Theta, component.MaxAngleModified.Theta); // goob edit reverse recoil direction for funny mechanics
        var newTheta = MathHelper.Clamp(component.CurrentAngle.Theta + component.AngleIncreaseModified.Theta - component.AngleDecayModified.Theta * timeSinceLastFire, minTheta, maxTheta); // goob edit
        component.CurrentAngle = new Angle(newTheta);
        component.LastFire = component.NextFire;

        // Convert it so angle can go either side.
        var random = Random.NextFloat(-0.5f, 0.5f);

        // Goobstation start
        var angleEv = new GetRecoilModifiersEvent()
        {
            Gun = component.Owner,
            User = user ?? component.Owner
        };
        if (user != null)
            RaiseLocalEvent(user.Value, angleEv);
        RaiseLocalEvent(component.Owner, angleEv);
        random *= angleEv.Modifier;
        // Goobstation end

        var spread = component.CurrentAngle.Theta * random;
        var angle = new Angle(direction.Theta + component.CurrentAngle.Theta * random);
        DebugTools.Assert(Math.Abs(spread) <= maxTheta); // goob edit
        return angle;
    }

    protected override void Popup(string message, EntityUid? uid, EntityUid? user) { }

    protected override void CreateEffect(EntityUid gunUid, MuzzleFlashEvent message, EntityUid? user = null)
    {
        var filter = Filter.Pvs(gunUid, entityManager: EntityManager);

        if (TryComp<ActorComponent>(user, out var actor))
            filter.RemovePlayer(actor.PlayerSession);

        RaiseNetworkEvent(message, filter);
    }

    public void PlayImpactSound(EntityUid otherEntity, DamageSpecifier? modifiedDamage, SoundSpecifier? weaponSound, bool forceWeaponSound)
    {
        DebugTools.Assert(!Deleted(otherEntity), "Impact sound entity was deleted");

        // Like projectiles and melee,
        // 1. Entity specific sound
        // 2. Ammo's sound
        // 3. Nothing
        var playedSound = false;

        if (!forceWeaponSound && modifiedDamage != null && modifiedDamage.GetTotal() > 0 && TryComp<RangedDamageSoundComponent>(otherEntity, out var rangedSound))
        {
            var type = SharedMeleeWeaponSystem.GetHighestDamageSound(modifiedDamage, ProtoManager);

            if (type != null && rangedSound.SoundTypes?.TryGetValue(type, out var damageSoundType) == true)
            {
                Audio.PlayPvs(damageSoundType, otherEntity, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
            else if (type != null && rangedSound.SoundGroups?.TryGetValue(type, out var damageSoundGroup) == true)
            {
                Audio.PlayPvs(damageSoundGroup, otherEntity, AudioParams.Default.WithVariation(DamagePitchVariation));
                playedSound = true;
            }
        }

        if (!playedSound && weaponSound != null)
        {
            Audio.PlayPvs(weaponSound, otherEntity);
        }
    }

    // TODO: Pseudo RNG so the client can predict these.
    #region Hitscan effects

    private void FireEffects(EntityCoordinates fromCoordinates, float distance, Angle angle, HitscanPrototype hitscan, EntityUid? hitEntity = null)
    {
        // Lord
        // Forgive me for the shitcode I am about to do
        // Effects tempt me not
        var sprites = new List<(NetCoordinates coordinates, Angle angle, SpriteSpecifier sprite, float scale)>();
        var fromXform = Transform(fromCoordinates.EntityId);

        // We'll get the effects relative to the grid / map of the firer
        // Look you could probably optimise this a bit with redundant transforms at this point.

        var gridUid = fromXform.GridUid;
        if (gridUid != fromCoordinates.EntityId && TryComp(gridUid, out TransformComponent? gridXform))
        {
            var (_, gridRot, gridInvMatrix) = TransformSystem.GetWorldPositionRotationInvMatrix(gridXform);
            var map = TransformSystem.ToMapCoordinates(fromCoordinates);
            fromCoordinates = new EntityCoordinates(gridUid.Value, Vector2.Transform(map.Position, gridInvMatrix));
            angle -= gridRot;
        }
        else
        {
            angle -= TransformSystem.GetWorldRotation(fromXform);
        }

        if (distance >= 1f)
        {
            if (hitscan.MuzzleFlash != null)
            {
                var coords = fromCoordinates.Offset(angle.ToVec().Normalized() / 2);
                var netCoords = GetNetCoordinates(coords);

                sprites.Add((netCoords, angle, hitscan.MuzzleFlash, 1f));
            }

            if (hitscan.TravelFlash != null)
            {
                var coords = fromCoordinates.Offset(angle.ToVec() * (distance + 0.5f) / 2);
                var netCoords = GetNetCoordinates(coords);

                sprites.Add((netCoords, angle, hitscan.TravelFlash, distance - 1.5f));
            }
        }

        if (hitscan.ImpactFlash != null)
        {
            var coords = fromCoordinates.Offset(angle.ToVec() * distance);
            var netCoords = GetNetCoordinates(coords);

            sprites.Add((netCoords, angle.FlipPositive(), hitscan.ImpactFlash, 1f));
        }

        if (sprites.Count > 0)
        {
            RaiseNetworkEvent(new HitscanEvent
            {
                Sprites = sprites,
            }, Filter.Pvs(fromCoordinates, entityMan: EntityManager));
        }
    }

    #endregion
}
