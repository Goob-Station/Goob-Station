// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading.Tasks;
using Content.Server._Lavaland.Megafauna.Components;
using Robust.Shared.Map.Components;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed class MegafaunaFieldSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, MegafaunaStartupEvent>(OnStartup);
        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, MegafaunaShutdownEvent>(OnShutdown);
        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, EntityTerminatingEvent>(OnTerminating);
    }

    private void OnStartup(Entity<MegafaunaFieldGeneratorComponent> ent, ref MegafaunaStartupEvent args)
        => ActivateField(ent);

    private void OnShutdown(Entity<MegafaunaFieldGeneratorComponent> ent, ref MegafaunaShutdownEvent args)
        => DeactivateField(ent);

    private void OnTerminating(Entity<MegafaunaFieldGeneratorComponent> ent, ref EntityTerminatingEvent args)
        => DeactivateField(ent);

    public void ActivateField(Entity<MegafaunaFieldGeneratorComponent> ent)
    {
        if (ent.Comp.Enabled)
            return;

        SpawnField(ent);
        ent.Comp.Enabled = true;
    }

    private async Task SpawnField(Entity<MegafaunaFieldGeneratorComponent> ent)
    {
        var xform = Transform(ent);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        // get tile position of our entity
        if (!_transform.TryGetGridTilePosition((ent, xform), out var tilePos))
            return;

        var tiles = TileHelperMethods.MakeBoxHollow(tilePos, ent.Comp.Radius);

        // fill the box
        foreach (var tile in tiles)
        {
            var wall = Spawn(ent.Comp.WallPrototype, _map.GridTileToWorld(xform.GridUid.Value, grid, tile));
            ent.Comp.Walls.Add(wall);
        }
    }

    public void DeactivateField(Entity<MegafaunaFieldGeneratorComponent> ent)
    {
        if (!ent.Comp.Enabled)
            return;

        var walls = ent.Comp.Walls.Where(x => !TerminatingOrDeleted(x));
        foreach (var wall in walls)
        {
            QueueDel(wall);
        }

        ent.Comp.Enabled = false;
    }
}
