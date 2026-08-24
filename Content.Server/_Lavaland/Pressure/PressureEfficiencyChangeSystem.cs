// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared._Lavaland.Weapons.Ranged.Events;
using Content.Shared.Armor;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Wieldable;

namespace Content.Server._Lavaland.Pressure;

public sealed class PressureEfficiencyChangeSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PressureDamageChangeComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PressureDamageChangeComponent, GetMeleeDamageEvent>(OnGetDamage, after: new []{typeof(SharedWieldableSystem)});
        SubscribeLocalEvent<PressureDamageChangeComponent, GunShotEvent>(OnGunShot);
        SubscribeLocalEvent<PressureDamageChangeComponent, ProjectileShotEvent>(OnProjectileShot);

        SubscribeLocalEvent<PressureArmorChangeComponent, ExaminedEvent>(OnArmorExamined);
        SubscribeLocalEvent<PressureArmorChangeComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnArmorRelayDamageModify, before: [typeof(SharedArmorSystem)]);
    }

    private void OnExamined(Entity<PressureDamageChangeComponent> ent, ref ExaminedEvent args)
    {
        var localeKey = "lavaland-examine-pressure-";
        localeKey += ent.Comp.ApplyWhenInRange ? "in-range-" : "out-range-";

        ExamineHelper(Math.Round(ent.Comp.LowerBound),
            Math.Round(ent.Comp.UpperBound),
            Math.Round(ent.Comp.AppliedModifier, 2),
            localeKey,
            ref args);
    }

    private void OnGetDamage(Entity<PressureDamageChangeComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (!ApplyModifier(ent)
            || !ent.Comp.ApplyToMelee)
            return;

        args.Damage *= ent.Comp.AppliedModifier;
    }

    private void OnGunShot(Entity<PressureDamageChangeComponent> ent, ref GunShotEvent args)
    {
        if (!ApplyModifier(ent)
            || !ent.Comp.ApplyToProjectiles)
            return;

        foreach (var (uid, _) in args.Ammo)
            if (TryComp<ProjectileComponent>(uid, out var projectile))
                projectile.Damage *= ent.Comp.AppliedModifier;
    }

    private void OnProjectileShot(Entity<PressureDamageChangeComponent> ent, ref ProjectileShotEvent args)
    {
        if (!ApplyModifier(ent)
            || !ent.Comp.ApplyToProjectiles
            || !TryComp<ProjectileComponent>(args.FiredProjectile, out var projectile))
            return;

        projectile.Damage *= ent.Comp.AppliedModifier;
    }

    public bool ApplyModifier(Entity<PressureDamageChangeComponent> ent)
    {
        var pressure = _atmos.GetTileMixture((ent.Owner, Transform(ent)))?.Pressure ?? 0f;
        return ent.Comp.Enabled && ((pressure >= ent.Comp.LowerBound
            && pressure <= ent.Comp.UpperBound) == ent.Comp.ApplyWhenInRange);
    }

    private void OnArmorExamined(Entity<PressureArmorChangeComponent> ent, ref ExaminedEvent args)
    {
        var localeKey = "lavaland-examine-pressure-armor-";
        localeKey += ent.Comp.ApplyWhenInRange ? "in-range-" : "out-range-";

        ExamineHelper(Math.Round(ent.Comp.LowerBound),
            Math.Round(ent.Comp.UpperBound),
            Math.Round(ent.Comp.ExtraPenetrationModifier * 100),
            localeKey,
            ref args);
    }

    private void OnArmorRelayDamageModify(Entity<PressureArmorChangeComponent> ent, ref InventoryRelayedEvent<DamageModifyEvent> args)
    {
        var pressure = _atmos.GetTileMixture((ent.Owner, Transform(ent)))?.Pressure ?? 0f;
        if ((pressure >= ent.Comp.LowerBound && pressure <= ent.Comp.UpperBound) != ent.Comp.ApplyWhenInRange
            || args.Args.TargetPart == null
            || !TryComp<ArmorComponent>(ent, out var armor))
            return;

        var (partType, _) = _body.ConvertTargetBodyPart(args.Args.TargetPart); // Woundmed stuff
        var coverage = armor.ArmorCoverage;
        if (!coverage.Contains(partType))
            return;

        args.Args.Damage.ArmorPenetration += ent.Comp.ExtraPenetrationModifier;
    }

    private void ExamineHelper(double min, double max, double modifier, string localeKey, ref ExaminedEvent args)
    {
        localeKey += modifier > 0f ? "debuff" : "buff";
        modifier = Math.Abs(modifier);
        args.PushMarkup(Loc.GetString(localeKey, ("min", min), ("max", max), ("modifier", modifier)));
    }
}
