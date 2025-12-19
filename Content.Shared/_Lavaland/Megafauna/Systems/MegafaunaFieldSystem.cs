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
using Content.Shared._Lavaland.EntityShapes;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Events;
using Robust.Shared.Threading;

// ReSharper disable EnforceForeachStatementBraces
namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed class MegafaunaFieldSystem : EntitySystem
{
    [Dependency] private readonly EntityShapeSystem _entityShape = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;

    private MegafaunaSpawnFieldJob _job;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, MegafaunaStartupEvent>(OnStartup);
        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, MegafaunaShutdownEvent>(OnShutdown);
        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, MegafaunaKilledEvent>(OnDefeated);
        SubscribeLocalEvent<MegafaunaFieldGeneratorComponent, EntityTerminatingEvent>(OnTerminating);

        _job = new MegafaunaSpawnFieldJob { System = this };
    }

    private void OnStartup(Entity<MegafaunaFieldGeneratorComponent> ent, ref MegafaunaStartupEvent args)
        => ActivateField(ent);

    private void OnShutdown(Entity<MegafaunaFieldGeneratorComponent> ent, ref MegafaunaShutdownEvent args)
        => DeactivateField(ent);

    private void OnDefeated(Entity<MegafaunaFieldGeneratorComponent> ent, ref MegafaunaKilledEvent args)
        => DeactivateField(ent);

    private void OnTerminating(Entity<MegafaunaFieldGeneratorComponent> ent, ref EntityTerminatingEvent args)
        => DeactivateField(ent);

    public void ActivateField(Entity<MegafaunaFieldGeneratorComponent> ent)
    {
        if (ent.Comp.Enabled)
            return;

        _job.Entity = ent;
        _parallel.ProcessNow(_job);
        ent.Comp.Enabled = true;
    }

    private void SpawnField(Entity<MegafaunaFieldGeneratorComponent> ent)
    {
        var comp = ent.Comp;
        _entityShape.SpawnEntityShape(comp.WallShape, ent.Owner, comp.WallId, out comp.Walls, true);
    }

    public void DeactivateField(Entity<MegafaunaFieldGeneratorComponent> ent)
    {
        if (!ent.Comp.Enabled)
            return;

        var walls = ent.Comp.Walls.Where(x => !TerminatingOrDeleted(x));
        foreach (var wall in walls)
            PredictedQueueDel(wall);

        ent.Comp.Enabled = false;
    }

    private record struct MegafaunaSpawnFieldJob : IRobustJob
    {
        public required MegafaunaFieldSystem System;
        public Entity<MegafaunaFieldGeneratorComponent> Entity;

        public void Execute()
        {
            System.SpawnField(Entity);
        }
    }
}

