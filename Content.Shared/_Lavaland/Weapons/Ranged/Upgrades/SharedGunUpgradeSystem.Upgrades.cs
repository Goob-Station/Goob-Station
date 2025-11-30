using Content.Shared._Goobstation.Weapons.Ranged;
using Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades;

public abstract partial class SharedGunUpgradeSystem
{
    private void InitializeUpgrades()
    {
        SubscribeLocalEvent<GunComponentUpgradeComponent, EntInsertedIntoContainerMessage>(OnCompsUpgradeInsert);
        SubscribeLocalEvent<GunComponentUpgradeComponent, EntRemovedFromContainerMessage>(OnCompsUpgradeEject);

        SubscribeLocalEvent<GunUpgradeFireRateComponent, GunRefreshModifiersEvent>(OnFireRateRefresh);
        SubscribeLocalEvent<GunUpgradeFireRateComponent, RechargeBasicEntityAmmoGetCooldownModifiersEvent>(OnFireRateRefreshRecharge);

        SubscribeLocalEvent<GunUpgradeSpeedComponent, GunRefreshModifiersEvent>(OnSpeedRefresh);

        SubscribeLocalEvent<GunUpgradeComponentsComponent, GunShotEvent>(OnDamageGunShotComps);

        SubscribeLocalEvent<GunUpgradeVampirismComponent, GunShotEvent>(OnVampirismGunShot);
        SubscribeLocalEvent<ProjectileVampirismComponent, ProjectileHitEvent>(OnVampirismProjectileHit);

        SubscribeLocalEvent<GunUpgradeBayonetComponent, GetRelayMeleeWeaponEvent>(OnGetMeleeRelay);
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

    private void OnCompsUpgradeInsert(Entity<GunComponentUpgradeComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        EntityManager.AddComponents(args.Entity, ent.Comp.ToAdd);
        EntityManager.RemoveComponents(args.Entity, ent.Comp.ToRemove);
    }

    private void OnCompsUpgradeEject(Entity<GunComponentUpgradeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        EntityManager.AddComponents(args.Entity, ent.Comp.ToAdd);
        EntityManager.RemoveComponents(args.Entity, ent.Comp.ToRemove);
    }

    private void OnSpeedRefresh(Entity<GunUpgradeSpeedComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.ProjectileSpeed *= ent.Comp.Coefficient;
    }

    private void OnDamageGunShotComps(Entity<GunUpgradeComponentsComponent> ent, ref GunShotEvent args)
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
}
