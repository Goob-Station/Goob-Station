using Content.Server.Atmos.EntitySystems;
using Content.Shared._Lavaland.Weapons.Ranged.Events;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Projectiles;

namespace Content.Server._Lavaland.Pressure;

public sealed partial class PressureEfficiencyChangeSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PressureDamageChangeComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PressureDamageChangeComponent, GetMeleeDamageEvent>(OnGetDamage);
        SubscribeLocalEvent<PressureDamageChangeComponent, GunShotEvent>(OnGunShot);
        SubscribeLocalEvent<PressureDamageChangeComponent, ProjectileShotEvent>(OnProjectileShot);
    }

    public void OnExamined(Entity<PressureDamageChangeComponent> ent, ref ExaminedEvent args)
    {
        var min = ent.Comp.LowerBound;
        var max = Math.Round(ent.Comp.UpperBound, MidpointRounding.ToZero);
        var modifier = Math.Round(ent.Comp.AppliedModifier, 2);

        var localeKey = "lavaland-examine-pressure-";
        localeKey += ent.Comp.ApplyWhenInRange ? "in-range-" : "out-range-";
        localeKey += modifier > 1f ? "buff" : "debuff";

        var markup = Loc.GetString(localeKey,
            ("min", min),
            ("max", max),
            ("modifier", modifier));

        args.PushMarkup(markup);
    }

    private void OnGetDamage(Entity<PressureDamageChangeComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (!ApplyModifier(ent))
            return;

        if (!ent.Comp.ApplyToMelee)
            return;

        args.Damage *= ent.Comp.AppliedModifier;
    }

    private void OnGunShot(Entity<PressureDamageChangeComponent> ent, ref GunShotEvent args)
    {
        if (!ApplyModifier(ent))
            return;

        if (!ent.Comp.ApplyToProjectiles)
            return;

        foreach (var (uid, shootable) in args.Ammo)
        {
            if (shootable is not IShootable shot
                || !TryComp<ProjectileComponent>(uid, out var projectile))
                continue;

            projectile.Damage *= ent.Comp.AppliedModifier;
        }
    }

    private void OnProjectileShot(Entity<PressureDamageChangeComponent> ent, ref ProjectileShotEvent args)
    {
        if (!ApplyModifier(ent)
            || !TryComp<ProjectileComponent>(args.FiredProjectile, out var projectile))
            return;

        if (!ent.Comp.ApplyToProjectiles)
            return;

        projectile.Damage *= ent.Comp.AppliedModifier;
    }

    public bool ApplyModifier(Entity<PressureDamageChangeComponent> ent)
    {
        var mix = _atmos.GetTileMixture((ent.Owner, Transform(ent)));
        var min = ent.Comp.LowerBound;
        var max = ent.Comp.UpperBound;
        var pressure = mix?.Pressure ?? 0f;
        var isInThresholds = pressure >= min && pressure <= max;

        return isInThresholds == ent.Comp.ApplyWhenInRange;
    }
}
