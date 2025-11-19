// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Adds enchants on mapinit for <see cref="EnchantFillComponent"/>.
/// </summary>
public sealed class EnchantFillSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnchantFillComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<EnchantFillComponent> ent, ref MapInitEvent args)
    {
        _enchanting.SetTier(ent, ent.Comp.Enchants.Count);
        foreach (var (id, level) in ent.Comp.Enchants)
        {
            if (!_enchanting.Enchant(ent, id, level))
                Log.Error($"Failed to enchant {ToPrettyString(ent)} with filled {id} {level}");
        }
    }
}
