using Content.Server._Lavaland.Pressure;
using Content.Shared._Lavaland.Weapons.Ranged.Events;
using Content.Shared._Lavaland.Weapons.Ranged.Upgrades;
using Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Server._Lavaland.Weapons.Ranged.Upgrades;

public sealed class GunUpgradeSystem : SharedGunUpgradeSystem
{
    [Dependency] private readonly PressureEfficiencyChangeSystem _pressure = default!;
    private const float PelletPenalty = 0.5f; // How much of the damage boost is penalized by virtue of being a multi-projectile weapon.
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunUpgradeDamageComponent, GunShotEvent>(OnDamageGunShot);
        SubscribeLocalEvent<GunUpgradeDamageComponent, ProjectileShotEvent>(OnProjectileShot);
    }

    private void OnDamageGunShot(Entity<GunUpgradeDamageComponent> ent, ref GunShotEvent args)
    {
        foreach (var (ammo, _) in args.Ammo)
        {
            var multiplier = 1f;

            if (TryComp<PressureDamageChangeComponent>(Transform(ent).ParentUid, out var pressure)
                && _pressure.ApplyModifier((Transform(ent).ParentUid, pressure)))
                multiplier = pressure.AppliedModifier;

            if (TryComp<ProjectileComponent>(ammo, out var proj))
                proj.Damage += ent.Comp.Damage * multiplier;
        }
    }

    private void OnProjectileShot(Entity<GunUpgradeDamageComponent> ent, ref ProjectileShotEvent args)
    {
        if (!TryComp<ProjectileComponent>(args.FiredProjectile, out var projectile))
            return;

        var multiplier = 1f;

        if (TryComp<PressureDamageChangeComponent>(Transform(ent).ParentUid, out var pressure)
            && _pressure.ApplyModifier((Transform(ent).ParentUid, pressure)))
            multiplier = pressure.AppliedModifier;

        projectile.Damage += ent.Comp.Damage * multiplier * PelletPenalty;
    }

}
