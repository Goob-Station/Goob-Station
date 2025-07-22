// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
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

namespace Content.Server._Lavaland.Pressure;

public sealed class PressureEfficiencyChangeSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PressureDamageChangeComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PressureDamageChangeComponent, GetMeleeDamageEvent>(OnGetDamage);
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
            || !ent.Comp.ApplyToMelee
            || !ent.Comp.Enabled)
            return;

        args.Damage *= ent.Comp.AppliedModifier;
    }

    private void OnGunShot(Entity<PressureDamageChangeComponent> ent, ref GunShotEvent args)
    {
        if (!ApplyModifier(ent)
            || !ent.Comp.ApplyToProjectiles
            || !ent.Comp.Enabled)
            return;

        foreach (var (uid, _) in args.Ammo)
            if (TryComp<ProjectileComponent>(uid, out var projectile))
                projectile.Damage *= ent.Comp.AppliedModifier;
    }

    private void OnProjectileShot(Entity<PressureDamageChangeComponent> ent, ref ProjectileShotEvent args)
    {
        if (!ApplyModifier(ent)
            || !TryComp<ProjectileComponent>(args.FiredProjectile, out var projectile)
            || !ent.Comp.ApplyToProjectiles)
            return;

        projectile.Damage *= ent.Comp.AppliedModifier;
    }

    public bool ApplyModifier(Entity<PressureDamageChangeComponent> ent)
    {
        var pressure = _atmos.GetTileMixture((ent.Owner, Transform(ent)))?.Pressure ?? 0f;
        return (pressure >= ent.Comp.LowerBound
                && pressure <= ent.Comp.UpperBound) == ent.Comp.ApplyWhenInRange;
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
