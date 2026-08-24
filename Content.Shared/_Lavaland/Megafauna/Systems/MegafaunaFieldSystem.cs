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

