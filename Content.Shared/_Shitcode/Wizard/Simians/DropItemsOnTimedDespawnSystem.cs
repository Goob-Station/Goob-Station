// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Spawners;

namespace Content.Shared._Goobstation.Wizard.Simians;

public sealed class DropItemsOnTimedDespawnSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DropItemsOnTimedDespawnComponent, TimedDespawnEvent>(OnDespawn);
    }

    private void OnDespawn(Entity<DropItemsOnTimedDespawnComponent> ent, ref TimedDespawnEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp(uid, out HandsComponent? hands))
            return;

        var despawnQuery = GetEntityQuery<TimedDespawnComponent>();
        var fadingQuery = GetEntityQuery<FadingTimedDespawnComponent>();

        foreach (var hand in _hands.EnumerateHands(uid, hands))
        {
            if (hand.HeldEntity == null)
                continue;

            var held = hand.HeldEntity.Value;

            if (!comp.DropDespawningItems && (fadingQuery.HasComp(held) || despawnQuery.HasComp(held)))
                continue;

            _hands.TryDrop(uid, hand, handsComp: hands);
        }
    }
}