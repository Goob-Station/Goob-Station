using Content.Shared._Lavaland.EntityShapes.Shapes;
using Content.Shared._Lavaland.Tile.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Lavaland.EntityShapes;

public sealed class EntityShapeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShapeSpawnerComponent, MapInitEvent>(OnSpawnerInit);
    }

    private void OnSpawnerInit(Entity<ShapeSpawnerComponent> ent, ref MapInitEvent args)
    {
        SpawnTileShape(ent.Comp.Shape, ent.Owner, ent.Comp.Spawn, out _);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ExpandingShapeSpawnerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var spawnerComp, out var xform))
        {
            spawnerComp.Accumulator += frameTime;
            if (spawnerComp.Accumulator < spawnerComp.SpawnPeriod)
                continue;
            spawnerComp.Accumulator = 0f;

            spawnerComp.Counter++;
            var counter = spawnerComp.Counter;

            if (spawnerComp.CounterOffset != null)
                spawnerComp.Shape.DefaultOffset = spawnerComp.CounterOffset.Value * counter;

            if (spawnerComp.CounterSize != null)
                spawnerComp.Shape.DefaultSize = (int) Math.Round(spawnerComp.CounterSize.Value * counter);

            if (spawnerComp.CounterStepSize != null)
                spawnerComp.Shape.DefaultStepSize = (int) Math.Round(spawnerComp.CounterStepSize.Value * counter);

            SpawnTileShape(spawnerComp.Shape, xform.Coordinates, spawnerComp.Spawn, out _);

            if (counter == spawnerComp.MaxCounter)
                QueueDel(uid);
        }
    }

    public void SpawnTileShape(EntityShape shape, EntityUid target, EntProtoId spawnId, out List<EntityUid> spawned)
    {
        SpawnTileShape(shape, Transform(target).Coordinates, spawnId, out spawned);
    }

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
}
