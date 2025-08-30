// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;

namespace Content.Server._Lavaland.Mobs;

public sealed class SpawnOnDeathSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpawnLootOnDeathComponent, AttackedEvent>(OnDropAttacked);
        SubscribeLocalEvent<SpawnLootOnDeathComponent, MobStateChangedEvent>(OnDropKilled);
    }

    private void OnDropAttacked(EntityUid uid, SpawnLootOnDeathComponent comp, ref AttackedEvent args)
    {
        comp.DoSpecialLoot = _whitelist.IsWhitelistPassOrNull(comp.SpecialWeaponWhitelist, args.Used);
    }

    private void OnDropKilled(EntityUid uid, SpawnLootOnDeathComponent comp, ref MobStateChangedEvent args)
    {
        if (!_mobState.IsDead(uid))
            return;

        var coords = Transform(uid).Coordinates;

        if (comp.DeleteOnDeath)
            QueueDel(uid);

        var droppedSpecial = false;
        if (comp.DoSpecialLoot && comp.SpecialLoot != null)
        {
            Spawn(comp.SpecialLoot, coords);
            droppedSpecial = true;
        }

        if (comp.Loot == null)
            return;

        if (droppedSpecial)
        {
            if (comp.DropBoth)
                Spawn(comp.Loot, coords);
        }
        else
            Spawn(comp.Loot, coords);
    }
}
