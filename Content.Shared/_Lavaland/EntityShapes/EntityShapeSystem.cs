using Content.Shared._Lavaland.EntityShapes.Components;
using Content.Shared._Lavaland.EntityShapes.Shapes;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.EntityShapes;

public sealed class EntityShapeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShapeSpawnerComponent, MapInitEvent>(OnSpawnerInit);
        SubscribeLocalEvent<ShapeSpawnerCounterComponent, MapInitEvent>(OnCounterInit);

        SubscribeLocalEvent<ExpandingShapeSpawnerComponent, SpawnCounterEntityShapeEvent>(OnExpandingShapeTrigger);
        SubscribeLocalEvent<SequenceShapeSpawnerComponent, SpawnCounterEntityShapeEvent>(OnSequenceShapeTrigger);
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

    public void SpawnTileShape(EntityShape shape, EntityUid target, EntProtoId spawnId, out List<EntityUid> spawned)
        => SpawnTileShape(shape, Transform(target).Coordinates, spawnId, out spawned);

    /// <remarks>
    /// Use this only if you need to get all spawned entities by this shape,
    /// otherwise it's better to spawn an entity with ShapeSpawnerComponent.
    /// </remarks>
    public void SpawnTileShape(EntityShape shape, EntityCoordinates coords, EntProtoId spawnId, out List<EntityUid> spawned)
    {
        spawned = new List<EntityUid>();

        var center = coords.Position;
        var result = shape.GetShape(_random.GetRandom(), _protoMan, center);

        foreach (var pos in result)
        {
            var coord = new EntityCoordinates(coords.EntityId, pos);
            var ent = PredictedSpawnAtPosition(spawnId, coord);
            spawned.Add(ent);
        }
    }

    private void OnSpawnerInit(Entity<ShapeSpawnerComponent> ent, ref MapInitEvent args)
        => SpawnTileShape(ent.Comp.Shape, ent.Owner, ent.Comp.Spawn, out _);

    private void OnCounterInit(Entity<ShapeSpawnerCounterComponent> ent, ref MapInitEvent args)
        => ent.Comp.NextSpawn = _timing.CurTime + ent.Comp.SpawnPeriod;

    private void OnExpandingShapeTrigger(Entity<ExpandingShapeSpawnerComponent> ent, ref SpawnCounterEntityShapeEvent args)
    {
        var comp = ent.Comp;

        if (comp.CounterOffset != null)
            comp.Shape.DefaultOffset = comp.CounterOffset.Value * args.Counter;

        if (comp.CounterSize != null)
            comp.Shape.DefaultSize = (int) Math.Round(comp.CounterSize.Value * args.Counter);

        if (comp.CounterStepSize != null)
            comp.Shape.DefaultStepSize = (int) Math.Round(comp.CounterStepSize.Value * args.Counter);

        SpawnTileShape(comp.Shape, Transform(ent).Coordinates, comp.Spawn, out _);
    }

    private void OnSequenceShapeTrigger(Entity<SequenceShapeSpawnerComponent> ent, ref SpawnCounterEntityShapeEvent args)
    {
        if (!ent.Comp.Scheduler.TryGetValue(args.Counter, out var shape))
            return;

        SpawnTileShape(shape, Transform(ent).Coordinates, ent.Comp.Spawn, out _);
    }
}
