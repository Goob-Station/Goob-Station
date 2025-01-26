using Content.Server.Administration.Logs;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Server.Effects;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using System.Linq;

namespace Content.Server.Projectiles;

public sealed class ProjectileSystem : SharedProjectileSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ColorFlashEffectSystem _color = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly GunSystem _guns = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ProjectileComponent, StartCollideEvent>(OnStartCollide);
    }

    private void OnStartCollide(EntityUid uid, ProjectileComponent component, ref StartCollideEvent args)
    {
        // This is so entities that shouldn't get a collision are ignored.
        if (args.OurFixtureId != ProjectileFixture || !args.OtherFixture.Hard
            || component.DamagedEntity || component is { Weapon: null, OnlyCollideWhenShot: true })
            return;

        var target = args.OtherEntity;
        // it's here so this check is only done once before possible hit
        var attemptEv = new ProjectileReflectAttemptEvent(uid, component, false);
        RaiseLocalEvent(target, ref attemptEv);
        if (attemptEv.Cancelled)
        {
            SetShooter(uid, component, target);
            _guns.SetTarget(uid, null); // Goobstation
            component.IgnoredEntities.Clear(); // Goobstation
            return;
        }

        var ev = new ProjectileHitEvent(component.Damage, target, component.Shooter);
        RaiseLocalEvent(uid, ref ev);

        var otherName = ToPrettyString(target);
        var modifiedDamage = _damageableSystem.TryChangeDamage(target, ev.Damage, component.IgnoreResistances, origin: component.Shooter);
        var deleted = Deleted(target);

        if (modifiedDamage is not null && EntityManager.EntityExists(component.Shooter))
        {
            if (modifiedDamage.AnyPositive() && !deleted)
            {
                _color.RaiseEffect(Color.Red, new List<EntityUid> { target }, Filter.Pvs(target, entityManager: EntityManager));
            }

            _adminLogger.Add(LogType.BulletHit,
                HasComp<ActorComponent>(target) ? LogImpact.Extreme : LogImpact.High,
                $"Projectile {ToPrettyString(uid):projectile} shot by {ToPrettyString(component.Shooter!.Value):user} hit {otherName:target} and dealt {modifiedDamage.GetTotal():damage} damage");
        }

        if (!deleted)
        {
            _guns.PlayImpactSound(target, modifiedDamage, component.SoundHit, component.ForceSound);

            if (!args.OurBody.LinearVelocity.IsLengthZero())
                _sharedCameraRecoil.KickCamera(target, args.OurBody.LinearVelocity.Normalized());
        }

        // Goobstation start
        if (component.Penetrate)
        {
            component.IgnoredEntities.Add(target);
            // Partial penetration weapons, like Hristov 
            if (component.LosesDamageOnPenetration && modifiedDamage is not null)
            {
                // Retrieve health of piercee, how much less potent the bullet should be now?
                // We're using the least "healthy" trigger, so we go from up
                var destructibleHealth = float.PositiveInfinity;
                if (TryComp<MobThresholdsComponent>(target, out var mob_comp))
                {
                    foreach (var treshold in mob_comp.Thresholds)
                    {
                        if (treshold.Value == MobState.Dead)
                            destructibleHealth = treshold.Key.Float();
                    }

                }
                else if (TryComp<DestructibleComponent>(target, out var destr_comp))
                {
                    // LINQ here would go hard. We're trying to find (perceived) health value of an object
                    // Sadly most have duplicates with extra large values, for nuke I presume, and also
                    // there's a danger of low-damage behaviour being thrown in here, that doesn't have
                    // destruction behaviour attached. So we just sift thru it until we get lowest health
                    // destroy behaviour
                    foreach (var threshold in destr_comp.Thresholds)
                    {
                        if (threshold.Trigger is DamageTrigger damageTrigger &&
                            damageTrigger.Damage < destructibleHealth)
                            foreach (var behavior in threshold.Behaviors)
                            {
                                if (behavior is DoActsBehavior doActsBehavior &&
                                    doActsBehavior.HasAct(ThresholdActs.Destruction))
                                    destructibleHealth = damageTrigger.Damage;
                            }

                    }

                }
                if (float.IsInfinity(destructibleHealth))
                    destructibleHealth = 0;
                // If bullets can't deal structural, they shouldn't be able to pierce, simple as
                component.Damage.TrimZeros();
                component.Damage -= component.DamageLossOnPenetrationBase * destructibleHealth;
                if (component.Damage.DamageDict.Values.Min() < 0.01f)
                {
                    component.DamagedEntity = true;
                    QueueDel(uid);
                }
            }
        }
        else
            component.DamagedEntity = true;
        // Goobstation end

        if (component.DeleteOnCollide || (component.NoPenetrateMask & args.OtherFixture.CollisionLayer) != 0) // Goobstation - Make x-ray arrows not penetrate blob
            QueueDel(uid);

        if (component.ImpactEffect != null && TryComp(uid, out TransformComponent? xform))
        {
            RaiseNetworkEvent(new ImpactEffectEvent(component.ImpactEffect, GetNetCoordinates(xform.Coordinates)), Filter.Pvs(xform.Coordinates, entityMan: EntityManager));
        }
    }
}
