// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Handles upgrading enchanted item tier when a player-controlled mob is sacrificed on top of an altar with it.
/// </summary>
public sealed class EnchantSacrificeSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<SoullessComponent> _soullessQuery;

    public override void Initialize()
    {
        base.Initialize();

        _soullessQuery = GetEntityQuery<SoullessComponent>();

        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        // only care about mobs going from alive/crit to dead
        if (args.NewMobState != MobState.Dead)
            return;

        // only care about player-controlled mobs, even if they're mice and shit
        var mob = args.Target;
        if (!_mind.TryGetMind(mob, out var mind, out _))
            return;

        // don't double dip by killing and reviving someone
        if (_soullessQuery.HasComp(mob) || _soullessQuery.HasComp(mind))
            return;

        // only care if the mob dies on an enchanting table
        if (_enchanting.FindTable(mob) is not {} table)
            return;

        var items = _enchanting.FindEnchantedItems(table);
        var upgraded = 0;
        EntityUid? anyItem = null;
        foreach (var item in items)
        {
            if (_enchanting.TryUpgradeTier(item))
            {
                upgraded++;
                anyItem = item;
            }
        }

        // nothing was upgraded L
        if (upgraded == 0 || anyItem is not {} any)
            return;

        // no double dipping
        EnsureComp<SoullessComponent>(mob);
        EnsureComp<SoullessComponent>(mind);

        var identity = Identity.Name(mob, EntityManager);
        var msg = upgraded == 1
            ? Loc.GetString("enchanting-sacrifice-single", ("target", identity), ("item", any))
            : Loc.GetString("enchanting-sacrifice-multiple", ("target", identity));

        _popup.PopupPredicted(msg, mob, null, PopupType.LargeCaution);
    }
}
