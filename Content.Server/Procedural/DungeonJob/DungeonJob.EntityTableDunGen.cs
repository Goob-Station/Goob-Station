// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading.Tasks;
using Content.Server.Ghost.Roles.Components;
using Content.Server.NPC.Systems;
using Content.Shared.EntityTable;
using Content.Shared.Physics;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonLayers;
using Robust.Shared.Collections;

namespace Content.Server.Procedural.DungeonJob;

public sealed partial class DungeonJob
{
    private async Task PostGen(
        EntityTableDunGen gen,
        Dungeon dungeon,
        Random random)
    {
        var availableRooms = new ValueList<DungeonRoom>();
        availableRooms.AddRange(dungeon.Rooms);
        var availableTiles = new ValueList<Vector2i>(dungeon.AllTiles);

        var count = random.Next(gen.MinCount, gen.MaxCount + 1);
        var npcs = _entManager.System<NPCSystem>();

        for (var i = 0; i < count; i++)
        {
            while (availableTiles.Count > 0)
            {
                var tile = availableTiles.RemoveSwap(random.Next(availableTiles.Count));

                if (!_anchorable.TileFree(_grid,
                        tile,
                        (int) CollisionGroup.MachineLayer,
                        (int) CollisionGroup.MachineLayer))
                {
                    continue;
                }

                var entities = _entManager.System<EntityTableSystem>().GetSpawns(gen.Table, random).ToList();
                foreach (var ent in entities)
                {
                    var uid = _entManager.SpawnAtPosition(ent, _maps.GridTileToLocal(_gridUid, _grid, tile));
                    _entManager.RemoveComponent<GhostRoleComponent>(uid);
                    _entManager.RemoveComponent<GhostTakeoverAvailableComponent>(uid);
                    npcs.SleepNPC(uid);
                }

                break;
            }

            await SuspendDungeon();

            if (!ValidateResume())
                return;
        }
    }
}