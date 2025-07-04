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
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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