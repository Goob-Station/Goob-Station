// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Mining.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Handles events for <see cref="FortuneEnchantComponent"/>.
/// </summary>
public sealed class FortuneEnchantSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;

    private EntityQuery<OreVeinComponent> _oreQuery;

    public override void Initialize()
    {
        base.Initialize();

        _oreQuery = GetEntityQuery<OreVeinComponent>();

        SubscribeLocalEvent<FortuneEnchantComponent, EnchantAddedEvent>(OnAdded);
        SubscribeLocalEvent<FortuneEnchantComponent, EnchantUpgradedEvent>(OnUpgraded);
        SubscribeLocalEvent<FortuneEnchantComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnAdded(Entity<FortuneEnchantComponent> ent, ref EnchantAddedEvent args)
    {
        SetChance(ent, args.Level);
    }

    private void OnUpgraded(Entity<FortuneEnchantComponent> ent, ref EnchantUpgradedEvent args)
    {
        SetChance(ent, args.Level);
    }

    private void SetChance(Entity<FortuneEnchantComponent> ent, int level)
    {
        ent.Comp.Chance = 1f + ent.Comp.BaseChance * (float) level;
    }

    private void OnMeleeHit(Entity<FortuneEnchantComponent> ent, ref MeleeHitEvent args)
    {
        var chance = ent.Comp.Chance;
        foreach (var hit in args.HitEntities)
        {
            if (_oreQuery.TryComp(hit, out var ore))
                ore.Modifier = chance;
        }
    }
}
