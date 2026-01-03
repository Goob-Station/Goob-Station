using Content.Goobstation.Common.Weapons;
using Content.Shared._Goobstation.Weapons.Ranged;
using Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades;

public abstract partial class SharedGunUpgradeSystem
{
    private void InitializeUpgrades()
    {
        SubscribeLocalEvent<GunUpgradeComponentsComponent, EntGotInsertedIntoContainerMessage>(OnCompsUpgradeInsert);
        SubscribeLocalEvent<GunUpgradeComponentsComponent, EntGotRemovedFromContainerMessage>(OnCompsUpgradeEject);

        SubscribeLocalEvent<GunUpgradeFireRateComponent, GunRefreshModifiersEvent>(OnFireRateRefresh);
        SubscribeLocalEvent<GunUpgradeFireRateComponent, RechargeBasicEntityAmmoGetCooldownModifiersEvent>(OnFireRateRefreshRecharge);

        SubscribeLocalEvent<GunUpgradeSpeedComponent, GunRefreshModifiersEvent>(OnSpeedRefresh);

        SubscribeLocalEvent<GunUpgradeProjectileComponentsComponent, GunShotEvent>(OnDamageGunShotComps);

        SubscribeLocalEvent<GunUpgradeVampirismComponent, GunShotEvent>(OnVampirismGunShot);
        SubscribeLocalEvent<ProjectileVampirismComponent, ProjectileHitEvent>(OnVampirismProjectileHit);

        SubscribeLocalEvent<GunUpgradeBayonetComponent, GetRelayMeleeWeaponEvent>(OnGetMeleeRelay);

        SubscribeLocalEvent<WeaponUpgradeDamageComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);

        SubscribeLocalEvent<WeaponUpgradeRangeComponent, GetLightAttackRangeEvent>(OnGetRange);
        SubscribeLocalEvent<WeaponUpgradeSpeedComponent, GetMeleeAttackRateEvent>(OnGetAttackRate);
    }

    private void OnFireRateRefresh(Entity<GunUpgradeFireRateComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.FireRate *= ent.Comp.Coefficient;
        args.BurstFireRate *= ent.Comp.Coefficient;
        args.BurstCooldown /= ent.Comp.Coefficient;
    }

    private void OnFireRateRefreshRecharge(Entity<GunUpgradeFireRateComponent> ent, ref RechargeBasicEntityAmmoGetCooldownModifiersEvent args)
    {
        args.Multiplier /= ent.Comp.Coefficient;
    }

    private void OnCompsUpgradeInsert(Entity<GunUpgradeComponentsComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!_timing.ApplyingState && HasComp<UpgradeableWeaponComponent>(args.Container.Owner))
            EntityManager.AddComponents(args.Container.Owner, ent.Comp.Components);
    }

    private void OnCompsUpgradeEject(Entity<GunUpgradeComponentsComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!_timing.ApplyingState && HasComp<UpgradeableWeaponComponent>(args.Container.Owner))
            EntityManager.RemoveComponents(args.Container.Owner, ent.Comp.Components);
    }

    private void OnSpeedRefresh(Entity<GunUpgradeSpeedComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.ProjectileSpeed *= ent.Comp.Coefficient;
    }

    private void OnDamageGunShotComps(Entity<GunUpgradeProjectileComponentsComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (HasComp<ProjectileComponent>(ammo))
                EntityManager.AddComponents(ammo.Value, ent.Comp.Components);
        }
    }

    private void OnVampirismGunShot(Entity<GunUpgradeVampirismComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            if (!HasComp<ProjectileComponent>(ammo))
                continue;

            var comp = EnsureComp<ProjectileVampirismComponent>(ammo.Value);
            comp.DamageOnHit = ent.Comp.DamageOnHit;
        }
    }

    private void OnVampirismProjectileHit(Entity<ProjectileVampirismComponent> ent, ref ProjectileHitEvent args)
    {
        if (!HasComp<MobStateComponent>(args.Target))
            return;
        _damage.TryChangeDamage(args.Shooter, ent.Comp.DamageOnHit);
    }

    private void OnGetMeleeRelay(Entity<GunUpgradeBayonetComponent> ent, ref GetRelayMeleeWeaponEvent args)
    {
        if (args.Handled)
            return;

        args.Found = ent.Owner;
        args.Handled = true;
    }

    private void OnGetMeleeDamage(Entity<WeaponUpgradeDamageComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.BonusDamage != null)
            args.Damage += ent.Comp.BonusDamage;
        args.Damage *= ent.Comp.Modifier;
    }

    private void OnGetRange(Entity<WeaponUpgradeRangeComponent> ent, ref GetLightAttackRangeEvent args)
    {
        if (ent.Comp.BonusRange != null)
            args.Range += ent.Comp.BonusRange.Value;
        if (ent.Comp.RangeMultiplier != null)
            args.Range *= ent.Comp.RangeMultiplier.Value;
    }

    private void OnGetAttackRate(Entity<WeaponUpgradeSpeedComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        if (ent.Comp.BonusAttackRate != null)
            args.Rate += ent.Comp.BonusAttackRate.Value;
        if (ent.Comp.AttackRateMultiplier != null)
            args.Multipliers *= ent.Comp.AttackRateMultiplier.Value;
    }
}
