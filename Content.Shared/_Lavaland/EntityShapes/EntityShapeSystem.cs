﻿using Content.Shared._Lavaland.Anger.Systems;
using Content.Shared._Lavaland.EntityShapes.Components;
using Content.Shared._Lavaland.EntityShapes.Shapes;
using Content.Shared._Lavaland.Megafauna.Events;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.EntityShapes;

public sealed class EntityShapeSystem : EntitySystem
{
    [Dependency] private readonly AngerSystem _anger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly INetManager _net = default!;

    private EntityQuery<ShapeSpawnerCounterComponent> _counterQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShapeSpawnerComponent, MapInitEvent>(OnSpawnerInit);
        SubscribeLocalEvent<ShapeSpawnerCounterComponent, MapInitEvent>(OnCounterInit);

        SubscribeLocalEvent<AngerShapeSpawnerComponent, SpawnedByActionEvent>(OnActionSpawned);

        SubscribeLocalEvent<ExpandingShapeSpawnerComponent, SpawnCounterEntityShapeEvent>(OnExpandingShapeTrigger);

        _counterQuery = GetEntityQuery<ShapeSpawnerCounterComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShapeSpawnerCounterComponent>();
        while (query.MoveNext(out var uid, out var counterComp))
        {
            if (counterComp.NextSpawn < _timing.CurTime)
                continue;

            counterComp.NextSpawn = _timing.CurTime + counterComp.SpawnPeriod;

            counterComp.Counter++;

            var ev = new SpawnCounterEntityShapeEvent(counterComp.Counter);
            RaiseLocalEvent(uid, ref ev);

            if (counterComp.Counter == counterComp.MaxCounter)
                QueueDel(uid);
        }
    }

    public void SpawnEntityShape(EntityShape shape, EntityUid target, EntProtoId spawnId, out List<EntityUid> spawned)
        => SpawnEntityShape(shape, Transform(target).Coordinates, spawnId, out spawned);

    /// <remarks>
    /// Use this only if you need to get all spawned entities by this shape,
    /// otherwise it's better to spawn an entity with ShapeSpawnerComponent.
    /// </remarks>
    public void SpawnEntityShape(EntityShape shape, EntityCoordinates coords, EntProtoId spawnId, out List<EntityUid> spawned)
    {
        spawned = new List<EntityUid>();

        // Sadly we still don't have shared random.
        // It also crashes the spawn menu.
        if (_net.IsClient)
            return;

        var center = coords.Position;
        var result = shape.GetShape(GetRandom(), _protoMan, center);

        foreach (var pos in result)
        {
            var coord = new EntityCoordinates(coords.EntityId, pos);
            var ent = PredictedSpawnAtPosition(spawnId, coord);
            spawned.Add(ent);
        }
    }

    private void OnSpawnerInit(Entity<ShapeSpawnerComponent> ent, ref MapInitEvent args)
        => SpawnEntityShape(ent.Comp.Shape, ent.Owner, ent.Comp.Spawn, out _);

    private void OnCounterInit(Entity<ShapeSpawnerCounterComponent> ent, ref MapInitEvent args)
        => ent.Comp.NextSpawn = _timing.CurTime + ent.Comp.SpawnPeriod;

    private void OnActionSpawned(Entity<AngerShapeSpawnerComponent> ent, ref SpawnedByActionEvent args)
    {
        if (!_counterQuery.TryComp(ent, out var counterComp))
            return;

        var anger = ent.Comp;

        if (anger.MaxCounterRange != null)
        {
            counterComp.MaxCounter = _anger.GetAngerScale(args.User,
                anger.MaxCounterRange.Value.X,
                anger.MaxCounterRange.Value.Y,
                anger.InverseCounter);
        }

        if (anger.SpawnPeriodRange != null)
        {
            counterComp.SpawnPeriod = _anger.GetAngerScale(args.User,
                TimeSpan.FromSeconds(anger.SpawnPeriodRange.Value.X),
                TimeSpan.FromSeconds(anger.SpawnPeriodRange.Value.Y),
                anger.InverseCounter);
        }
    }

    private void OnExpandingShapeTrigger(Entity<ExpandingShapeSpawnerComponent> ent, ref SpawnCounterEntityShapeEvent args)
    {
        var comp = ent.Comp;

        if (comp.CounterOffset != null)
            comp.Shape.DefaultOffset = comp.CounterOffset.Value * args.Counter;

        if (comp.CounterSize != null)
            comp.Shape.DefaultSize = (int) Math.Round(comp.CounterSize.Value * args.Counter);

        if (comp.CounterStepSize != null)
            comp.Shape.DefaultStepSize = (int) Math.Round(comp.CounterStepSize.Value * args.Counter);

        SpawnEntityShape(comp.Shape, Transform(ent).Coordinates, comp.Spawn, out _);
    }

    private System.Random GetRandom()
    {
        return new System.Random((int) _timing.CurTick.Value);
    }
}
