// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <43253663+DogZeroX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <le0nel_1van@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    protected override void InitializeBattery()
    {
        base.InitializeBattery();

        // Hitscan
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, ComponentStartup>(OnBatteryStartup);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, ChargeChangedEvent>(OnBatteryChargeChange);
        SubscribeLocalEvent<HitscanBatteryAmmoProviderComponent, DamageExamineEvent>(OnBatteryDamageExamine);

        // Projectile
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, ComponentStartup>(OnBatteryStartup);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, ChargeChangedEvent>(OnBatteryChargeChange);
        SubscribeLocalEvent<ProjectileBatteryAmmoProviderComponent, DamageExamineEvent>(OnBatteryDamageExamine);
    }

    private void OnBatteryStartup(EntityUid uid, BatteryAmmoProviderComponent component, ComponentStartup args)
    {
        UpdateShots(uid, component);
    }

    private void OnBatteryChargeChange(EntityUid uid, BatteryAmmoProviderComponent component, ref ChargeChangedEvent args)
    {
        UpdateShots(uid, component, args.Charge, args.MaxCharge);
    }

    private void UpdateShots(EntityUid uid, BatteryAmmoProviderComponent component)
    {
        if (!TryComp<BatteryComponent>(uid, out var battery))
            return;

        UpdateShots(uid, component, battery.CurrentCharge, battery.MaxCharge);
    }

    private void UpdateShots(EntityUid uid, BatteryAmmoProviderComponent component, float charge, float maxCharge)
    {
        var shots = (int) (charge / component.FireCost);
        var maxShots = (int) (maxCharge / component.FireCost);

        if (component.Shots != shots || component.Capacity != maxShots)
        {
            Dirty(uid, component);
        }

        component.Shots = shots;
        component.Capacity = maxShots;
        UpdateBatteryAppearance(uid, component);
    }

    private void OnBatteryDamageExamine(EntityUid uid, BatteryAmmoProviderComponent component, ref DamageExamineEvent args)
    {
        var damageSpec = GetDamage(component);

        if (damageSpec == null)
            return;

        var damageType = component switch
        {
            HitscanBatteryAmmoProviderComponent => Loc.GetString("damage-hitscan"),
            ProjectileBatteryAmmoProviderComponent => Loc.GetString("damage-projectile"),
            _ => throw new ArgumentOutOfRangeException(),
        };

        _damageExamine.AddDamageExamine(args.Message, Damageable.ApplyUniversalAllModifiers(damageSpec), damageType);
    }

    private DamageSpecifier? GetDamage(BatteryAmmoProviderComponent component)
    {
        if (component is ProjectileBatteryAmmoProviderComponent battery)
        {
            if (ProtoManager.Index<EntityPrototype>(battery.Prototype).Components
                .TryGetValue(_factory.GetComponentName(typeof(ProjectileComponent)), out var projectile))
            {
                var p = (ProjectileComponent) projectile.Component;

                if (!p.Damage.Empty)
                {
                    return p.Damage * Damageable.UniversalProjectileDamageModifier;
                }
            }

            return null;
        }

        if (component is HitscanBatteryAmmoProviderComponent hitscan)
        {
            var dmg = ProtoManager.Index<HitscanPrototype>(hitscan.Prototype).Damage;
            return dmg == null ? dmg : dmg * Damageable.UniversalHitscanDamageModifier;
        }

        return null;
    }

    protected override void TakeCharge(EntityUid uid, BatteryAmmoProviderComponent component)
    {
        // Will raise ChargeChangedEvent
        _battery.UseCharge(uid, component.FireCost);
    }
}