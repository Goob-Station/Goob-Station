// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.Random.Helpers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Adds enchants on mapinit for <see cref="EnchantFillComponent"/>.
/// </summary>
public sealed class EnchantFillSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnchantFillComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RandomEnchantFillComponent, MapInitEvent>(OnRandomMapInit);
    }

    private void OnRandomMapInit(Entity<RandomEnchantFillComponent> ent, ref MapInitEvent args)
    {
        if (_net.IsClient)
            return;

        var tier = ent.Comp.MinMaxTier.Next(_random);
        tier = Math.Min(tier, ent.Comp.Enchants.Count);

        if (tier < 1)
            return;

        _enchanting.SetTier(ent.Owner, tier);
        var weights = ent.Comp.Enchants
            .Select(kvp => new KeyValuePair<EntProtoId<EnchantComponent>, float>(kvp.Key, kvp.Value.Weight))
            .ToDictionary();

        for (var i = 0; i < tier; i++)
        {
            var id = _random.PickAndTake(weights);
            var data = ent.Comp.Enchants[id];
            var level = data.MinMaxLevel.Next(_random);

            if (level < 1)
                continue;

            if (!_enchanting.Enchant(ent, id, level, ent.Comp.Fake))
                Log.Error($"Failed to enchant {ToPrettyString(ent)} with filled {id} {level}");
        }
    }

    private void OnMapInit(Entity<EnchantFillComponent> ent, ref MapInitEvent args)
    {
        _enchanting.SetTier(ent.Owner, ent.Comp.Enchants.Count);
        foreach (var (id, level) in ent.Comp.Enchants)
        {
            if (!_enchanting.Enchant(ent, id, level, ent.Comp.Fake))
                Log.Error($"Failed to enchant {ToPrettyString(ent)} with filled {id} {level}");
        }
    }
}
