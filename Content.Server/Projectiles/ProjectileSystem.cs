// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Projectiles;
using Content.Goobstation.Common.Weapons.Penetration;
using Content.Server.Administration.Logs;
using Content.Server.Destructible;
using Content.Server.Effects;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Camera;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Content.Shared.Damage;

namespace Content.Server.Projectiles;

public sealed class ProjectileSystem : SharedProjectileSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ColorFlashEffectSystem _color = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly DestructibleSystem _destructibleSystem = default!;
    [Dependency] private readonly GunSystem _guns = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileComponent, StartCollideEvent>(OnStartCollide);
    }

    private void OnStartCollide(EntityUid uid, ProjectileComponent component, ref StartCollideEvent args)
    {
        // This is so entities that shouldn't get a collision are ignored.
        if (args.OurFixtureId != ProjectileFixture || !args.OtherFixture.Hard
            || component.ProjectileSpent || component is { Weapon: null, OnlyCollideWhenShot: true })
            return;

        var target = args.OtherEntity;
        // it's here so this check is only done once before possible hit
        var attemptEv = new ProjectileReflectAttemptEvent(uid, component, false);
        RaiseLocalEvent(target, ref attemptEv);
        if (attemptEv.Cancelled)
        {
            SetShooter(uid, component, target);
            _guns.SetTarget(uid, null, out _); // Goob
            component.IgnoredEntities.Clear(); // Goob
            return;
        }

        var ev = new ProjectileHitEvent(component.Damage * _damageableSystem.UniversalProjectileDamageModifier, target, component.Shooter);
        RaiseLocalEvent(uid, ref ev);

        var otherName = ToPrettyString(target);
        var damageRequired = _destructibleSystem.DestroyedAt(target);
        if (TryComp<DamageableComponent>(target, out var damageableComponent))
        {
            damageRequired -= damageableComponent.TotalDamage;
            damageRequired = FixedPoint2.Max(damageRequired, FixedPoint2.Zero);
        }

        // Goob start - shitmed targeting
        TargetBodyPart? targetPart = null;
        if (TryComp(uid, out ProjectileMissTargetPartChanceComponent? missComp) &&
            !missComp.PerfectHitEntities.Contains(target))
            targetPart = TargetBodyPart.Chest;
        // Goob end

        var deleted = Deleted(target);

        if (_damageableSystem.TryChangeDamage((target, damageableComponent), ev.Damage, out var damage, component.IgnoreResistances, origin: component.Shooter, targetPart: targetPart) && Exists(component.Shooter)) // Goob - add shitmed targeting targetPart 
        {
            if (!deleted)
            {
                _color.RaiseEffect(Color.Red, new List<EntityUid> { target }, Filter.Pvs(target, entityManager: EntityManager));
            }

            _adminLogger.Add(LogType.BulletHit,
                LogImpact.Medium,
                $"Projectile {ToPrettyString(uid):projectile} shot by {ToPrettyString(component.Shooter!.Value):user} hit {otherName:target} and dealt {damage:damage} damage");

            /* // Goob start
            // TODO: i don't know why we are moving this to TryPenetrate. logic is reordered and i'm lazy to find out why
            // i think marty did this but i dont know why
            // TODO UPSTREAMER
            // If penetration is to be considered, we need to do some checks to see if the projectile should stop.
            if (component.PenetrationThreshold != 0)
            {
                // If a damage type is required, stop the bullet if the hit entity doesn't have that type.
                if (component.PenetrationDamageTypeRequirement != null)
                {
                    var stopPenetration = false;
                    foreach (var requiredDamageType in component.PenetrationDamageTypeRequirement)
                    {
                        if (!damage.DamageDict.Keys.Contains(requiredDamageType))
                        {
                            stopPenetration = true;
                            break;
                        }
                    }
                    if (stopPenetration)
                        component.ProjectileSpent = true;
                }

                // If the object won't be destroyed, it "tanks" the penetration hit.
                if (damage.GetTotal() < damageRequired)
                {
                    component.ProjectileSpent = true;
                }

                if (!component.ProjectileSpent)
                {
                    component.PenetrationAmount += damageRequired;
                    // The projectile has dealt enough damage to be spent.
                    if (component.PenetrationAmount >= component.PenetrationThreshold)
                    {
                        component.ProjectileSpent = true;
                    }
                }
            }
            else
            {
                component.ProjectileSpent = true;
            }
            */

            // Goob start - see above
            if (damage is not null)
                component.ProjectileSpent = !TryPenetrate((uid, component), damage, damageRequired, target);
            else
                component.ProjectileSpent = true;
            // Goob end
        }
        else
        {
            component.ProjectileSpent = true;
        }

        if (!deleted)
        {
            _guns.PlayImpactSound(target, damage, component.SoundHit, component.ForceSound);

            if (!args.OurBody.LinearVelocity.IsLengthZero())
                _sharedCameraRecoil.KickCamera(target, args.OurBody.LinearVelocity.Normalized());
        }

        if (component.DeleteOnCollide && component.ProjectileSpent || (component.NoPenetrateMask & args.OtherFixture.CollisionLayer) != 0) // Goobstation - Make x-ray arrows not penetrate blob
            QueueDel(uid);

        if (component.ImpactEffect != null && TryComp(uid, out TransformComponent? xform))
        {
            RaiseNetworkEvent(new ImpactEffectEvent(component.ImpactEffect, GetNetCoordinates(xform.Coordinates)), Filter.Pvs(xform.Coordinates, entityMan: EntityManager));
        }
    }

    private bool TryPenetrate(Entity<ProjectileComponent> projectile, DamageSpecifier damage, FixedPoint2 damageRequired,
        EntityUid? target = null) // GOob - this function is goob for some reason. code has been moved. see above
    {
        // If penetration is to be considered, we need to do some checks to see if the projectile should stop.
        if (projectile.Comp.PenetrationThreshold == 0)
            return false;

        // If a damage type is required, stop the bullet if the hit entity doesn't have that type.
        if (projectile.Comp.PenetrationDamageTypeRequirement != null)
        {
            foreach (var requiredDamageType in projectile.Comp.PenetrationDamageTypeRequirement)
            {
                if (damage.DamageDict.Keys.Contains(requiredDamageType))
                    continue;

                return false;
            }
        }
        // Goobstation - Splits penetration change if target have PenetratableComponent
        if (!TryComp<PenetratableComponent>(target, out var penetratable))
        {
            // If the object won't be destroyed, it "tanks" the penetration hit.
            if (damage.GetTotal() < damageRequired)
            {
                return false;
            }

            if (!projectile.Comp.ProjectileSpent)
            {
                projectile.Comp.PenetrationAmount += damageRequired;
                // The projectile has dealt enough damage to be spent.
                if (projectile.Comp.PenetrationAmount >= projectile.Comp.PenetrationThreshold)
                {
                    return false;
                }
            }
        }
        else
        {
            // Goobstation - Here penetration threshold count as "penetration health".
            // If it's lower than damage than penetation damage entity cause it deletes projectile
            if (projectile.Comp.PenetrationThreshold < penetratable.PenetrateDamage)
            {
                projectile.Comp.ProjectileSpent = true;
                return false;
            }

            projectile.Comp.PenetrationThreshold -= FixedPoint2.New(penetratable.PenetrateDamage);
            projectile.Comp.Damage *= (1 - penetratable.DamagePenaltyModifier);
        }

        return true;
    }
}
