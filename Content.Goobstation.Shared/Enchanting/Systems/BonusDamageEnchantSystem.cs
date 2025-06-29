// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Handles fire + temperature events for <see cref="BonusDamageEnchantComponent"/>.
/// </summary>
public sealed class BonusDamageEnchantSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BonusDamageEnchantComponent, EnchantAddedEvent>(OnAdded);
        SubscribeLocalEvent<BonusDamageEnchantComponent, EnchantUpgradedEvent>(OnUpgraded);
        SubscribeLocalEvent<BonusDamageEnchantComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnAdded(Entity<BonusDamageEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        Modify(ent, (float) args.Level);
    }

    private void OnUpgraded(Entity<BonusDamageEnchantComponent> ent, ref EnchantUpgradedEvent args)
    {
        Modify(ent, (float) args.Level / (float) args.OldLevel);
    }

    private void Modify(Entity<BonusDamageEnchantComponent> ent, float factor)
    {
        ent.Comp.Damage *= factor;
        Dirty(ent);
    }

    private void OnMeleeHit(Entity<BonusDamageEnchantComponent> ent, ref MeleeHitEvent args)
    {
        args.BonusDamage += ent.Comp.Damage;
    }
}
