// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Weapons.Melee.Components;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Controls <see cref="MeleeThrowOnHitComponent"/> values with the enchant level.
/// </summary>
public sealed class KnockbackEnchantSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KnockbackEnchantComponent, EnchantAddedEvent>(OnAdded);
        SubscribeLocalEvent<KnockbackEnchantComponent, EnchantUpgradedEvent>(OnUpgraded);
    }

    private void OnAdded(Entity<KnockbackEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        Modify(ent, (float) args.Level);
    }

    private void OnUpgraded(Entity<KnockbackEnchantComponent> ent, ref EnchantUpgradedEvent args)
    {
        Modify(ent, (float) args.Level / (float) args.OldLevel);
    }

    private void Modify(EntityUid uid, float factor)
    {
        var comp = EnsureComp<MeleeThrowOnHitComponent>(uid);
        comp.Speed *= factor;
        comp.Distance *= factor;
        Dirty(uid, comp);
    }
}
