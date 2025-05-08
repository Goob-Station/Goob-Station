// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Damage;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Handles damage modifier events for <see cref="DamageModifyEnchantComponent"/>.
/// </summary>
public sealed class DamageModifyEnchantSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageModifyEnchantComponent, EnchantAddedEvent>(OnAdded);
        SubscribeLocalEvent<DamageModifyEnchantComponent, EnchantUpgradedEvent>(OnUpgraded);
        SubscribeLocalEvent<DamageModifyEnchantComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnAdded(Entity<DamageModifyEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        ent.Comp.Modifier = (float) Math.Pow(ent.Comp.Factor, args.Level);
    }

    private void OnUpgraded(Entity<DamageModifyEnchantComponent> ent, ref EnchantUpgradedEvent args)
    {
        ent.Comp.Modifier = (float) Math.Pow(ent.Comp.Factor, args.Level);
    }

    private void OnDamageModify(Entity<DamageModifyEnchantComponent> ent, ref DamageModifyEvent args)
    {
        // no wearing DamageModify III mouse for your protection
        if (!ent.Comp.ProtectWearer && _enchanting.GetEnchantedItem(ent) != args.Target)
            return;

        args.Damage = args.Damage * ent.Comp.Modifier;
    }
}
