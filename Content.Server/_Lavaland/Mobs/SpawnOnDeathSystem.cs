// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityTable;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;

// ReSharper disable EnforceForeachStatementBraces
// ReSharper disable EnforceIfStatementBraces
namespace Content.Server._Lavaland.Mobs;

public sealed class SpawnOnDeathSystem : EntitySystem
{
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
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
        if (comp.DoSpecialLoot && comp.SpecialTable != null)
        {
            var specialLoot = _entityTable.GetSpawns(comp.SpecialTable);
            foreach (var item in specialLoot)
                Spawn(item, coords);

            droppedSpecial = true;
        }

        if (comp.Table == null)
            return;

        var loot = _entityTable.GetSpawns(comp.Table);
        if (droppedSpecial)
        {
            if (comp.DropBoth)
                foreach (var item in loot)
                    Spawn(item, coords);
        }
        else
            foreach (var item in loot)
                Spawn(item, coords);
    }
}
