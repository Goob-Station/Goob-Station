using Content.Shared._Lavaland.Anger.Systems;
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
    [Dependency] private readonly IMapManager _mapMan = default!;

    private EntityQuery<ShapeSpawnerComponent> _spawnerQuery;
    private EntityQuery<ShapeSpawnerCounterComponent> _counterQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShapeSpawnerComponent, MapInitEvent>(OnSpawnerInit);
        SubscribeLocalEvent<ShapeSpawnerCounterComponent, MapInitEvent>(OnCounterInit);
        SubscribeLocalEvent<AngerShapeSpawnerComponent, SpawnedByActionEvent>(OnActionSpawned);
        SubscribeLocalEvent<ExpandingShapeSpawnerComponent, SpawnCounterEntityShapeEvent>(OnExpandingShapeTrigger);

        _spawnerQuery = GetEntityQuery<ShapeSpawnerComponent>();
        _counterQuery = GetEntityQuery<ShapeSpawnerCounterComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ShapeSpawnerCounterComponent>();
        while (query.MoveNext(out var uid, out var counterComp))
        {
            if (counterComp.NextSpawn > curTime)
                continue;

            if (counterComp.Counter == counterComp.MaxCounter)
            {
                PredictedQueueDel(uid);
                continue;
            }

            counterComp.NextSpawn = curTime + counterComp.SpawnPeriod;
            counterComp.Counter++;

            var ev = new SpawnCounterEntityShapeEvent(counterComp.Counter);
            RaiseLocalEvent(uid, ref ev);
        }
    }

    public void SpawnEntityShape(EntityShape shape, EntityUid target, EntProtoId spawnId, out List<EntityUid> spawned, bool alignTile = false)
    {
        var coords = alignTile
            ? Transform(target).Coordinates.AlignWithClosestGridTile(1.5f, EntityManager, _mapMan)
            : Transform(target).Coordinates;

        SpawnEntityShape(shape, coords, spawnId, out spawned);
    }

    /// <remarks>
    /// Use this only if you need to get all spawned entities by this shape,
    /// otherwise it's better to spawn an entity with ShapeSpawnerComponent.
    /// </remarks>
    public void SpawnEntityShape(EntityShape shape, EntityCoordinates coords, EntProtoId spawnId, out List<EntityUid> spawned)
    {
        spawned = new List<EntityUid>();

        // Sadly we still don't have proper shared random.
        // It also crashes the spawn menu.
        if (_net.IsClient)
            return;

        var result = shape.GetShape(GetRandom(), _protoMan);
        for (int i = 0; i < result.Count; i++)
        {
            result[i] += coords.Position;
        }

        foreach (var pos in result)
        {
            var coord = new EntityCoordinates(coords.EntityId, pos);
            var ent = PredictedSpawnAtPosition(spawnId, coord);
            spawned.Add(ent);
        }
    }

    private void OnSpawnerInit(Entity<ShapeSpawnerComponent> ent, ref MapInitEvent args)
        => SpawnEntityShape(ent.Comp.Shape, ent.Owner, ent.Comp.Spawn, out _, ent.Comp.AlignCoords);

    private void OnCounterInit(Entity<ShapeSpawnerCounterComponent> ent, ref MapInitEvent args)
        => ent.Comp.NextSpawn = _timing.CurTime + ent.Comp.SpawnPeriod; // First shape is spawned by an event right above this one, so delay it

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
        var (uid, comp) = ent;

        if (!_spawnerQuery.TryComp(uid, out var spawner))
            return;

        if (comp.CounterOffset != null)
            spawner.Shape.DefaultOffset = comp.CounterOffset.Value * args.Counter;

        if (comp.CounterSize != null)
            spawner.Shape.DefaultSize = (int) Math.Round(comp.CounterSize.Value * args.Counter);

        if (comp.CounterStepSize != null)
            spawner.Shape.DefaultStepSize = (int) Math.Round(comp.CounterStepSize.Value * args.Counter);

        SpawnEntityShape(spawner.Shape, uid, spawner.Spawn, out _, spawner.AlignCoords);
    }

    private System.Random GetRandom()
    {
        return new System.Random((int) _timing.CurTick.Value);
    }
}
