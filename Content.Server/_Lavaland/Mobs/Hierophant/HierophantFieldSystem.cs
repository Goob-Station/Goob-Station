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
using Content.Server._Lavaland.Mobs.Hierophant.Components;
using Robust.Shared.Map.Components;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Content.Server._Lavaland.Mobs.Hierophant;

public sealed class HierophantFieldSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantFieldGeneratorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HierophantFieldGeneratorComponent, EntityTerminatingEvent>(OnTerminating);
    }

    #region Event Handling

    private void OnMapInit(Entity<HierophantFieldGeneratorComponent> ent, ref MapInitEvent args)
    {
        var xform = Transform(ent).Coordinates;
        var hierophant = Spawn(ent.Comp.HierophantPrototype, xform);

        if (!TryComp<HierophantBossComponent>(hierophant, out var hieroComp))
            return;

        ent.Comp.ConnectedHierophant = hierophant;
        hieroComp.ConnectedFieldGenerator = ent;
    }

    private void OnTerminating(Entity<HierophantFieldGeneratorComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.ConnectedHierophant != null &&
            TryComp<HierophantBossComponent>(ent.Comp.ConnectedHierophant.Value, out var hieroComp))
            hieroComp.ConnectedFieldGenerator = null;

        DeleteHierophantFieldImmediatly(ent);
    }

    #endregion

    public void ActivateField(Entity<HierophantFieldGeneratorComponent> ent)
    {
        if (ent.Comp.Enabled)
            return; // how?

        SpawnHierophantField(ent);
        ent.Comp.Enabled = true;
    }

    public void DeactivateField(Entity<HierophantFieldGeneratorComponent> ent)
    {
        if (!ent.Comp.Enabled)
            return; // how?

        DeleteHierophantField(ent);
        ent.Comp.Enabled = false;
    }

    public void DeleteHierophantFieldImmediatly(Entity<HierophantFieldGeneratorComponent> ent)
    {
        var walls = ent.Comp.Walls.Where(x => !TerminatingOrDeleted(x));
        foreach (var wall in walls)
        {
            QueueDel(wall);
        }
    }

    private async Task SpawnHierophantField(Entity<HierophantFieldGeneratorComponent> ent)
    {
        var xform = Transform(ent);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gridEnt = (xform.GridUid.Value, grid);
        var range = ent.Comp.Radius;
        var center = xform.Coordinates.Position;

        // get tile position of our entity
        if (!_transform.TryGetGridTilePosition((ent, xform), out var tilePos))
            return;

        var pos = _map.TileCenterToVector(gridEnt, tilePos);
        var confines = new Box2(center, center).Enlarged(ent.Comp.Radius);
        var box = _map.GetLocalTilesIntersecting(ent, grid, confines).ToList();

        var confinesS = new Box2(pos, pos).Enlarged(Math.Max(range - 1, 0));
        var boxS = _map.GetLocalTilesIntersecting(ent, grid, confinesS).ToList();
        box = box.Where(b => !boxS.Contains(b)).ToList();

        // fill the box
        foreach (var tile in box)
        {
            var wall = Spawn(ent.Comp.WallPrototype, _map.GridTileToWorld(xform.GridUid.Value, grid, tile.GridIndices));
            ent.Comp.Walls.Add(wall);
        }
    }

    private async Task DeleteHierophantField(Entity<HierophantFieldGeneratorComponent> ent)
    {
        var walls = ent.Comp.Walls.Where(x => !TerminatingOrDeleted(x));
        foreach (var wall in walls)
        {
            QueueDel(wall);
        }
    }
}