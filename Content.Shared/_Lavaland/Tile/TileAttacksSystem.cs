using System.Linq;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Threading;

namespace Content.Shared._Lavaland.Tile;

public sealed class TileShapeSpawnerSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private SpawnJob _job;

    public override void Initialize()
    {
        base.Initialize();
        _job = new SpawnJob { System = this };
    }

    public void SpawnTileShapeAtPosition(EntityCoordinates coords, EntProtoId spawnId, ProtoId<TileShapePrototype> shapeId)
    {
        var grid = _xform.GetGrid(coords);

        if (!TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        SpawnTileShape((grid.Value, gridComp), shapeId, coords, spawnId);
    }

    public void SpawnTileShapeAtTarget(EntityUid uid, EntProtoId spawnId, ProtoId<TileShapePrototype> shapeId)
    {
        var xform = Transform(uid);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var gridComp))
            return;

        SpawnTileShape((xform.GridUid.Value, gridComp), shapeId, xform.Coordinates, spawnId);
    }

    private void SpawnTileShape(Entity<MapGridComponent> grid, ProtoId<TileShapePrototype> shapeId, EntityCoordinates coords, EntProtoId spawnId)
    {
        var center = _map.CoordinatesToTile(grid.Owner, grid.Comp, coords);
        _job.ResultShape = GetShapeById(shapeId, center);
        _job.Grid = grid;
        _job.SpawnId = spawnId;
        _parallel.ProcessNow(_job);
    }

    public List<Vector2i> GetShapeById(ProtoId<TileShapePrototype> shapeId, Vector2i center)
    {
        var result = new List<Vector2i>();
        var shapes = _protoMan.Index(shapeId);
        foreach (var shape in shapes.Shapes)
        {
            result.AddRange(shape.GetShape(center));
        }

        return result.ToHashSet().ToList();
    }

    private void SpawnShape(Entity<MapGridComponent> grid, List<Vector2i> coords, EntProtoId spawnId)
    {
        foreach (var tile in coords)
        {
            var pos = _map.GridTileToLocal(grid.Owner, grid.Comp, tile);
            PredictedSpawnAtPosition(spawnId, pos);
        }
    }

    private record struct SpawnJob : IRobustJob
    {
        public required TileShapeSpawnerSystem System;
        public Entity<MapGridComponent> Grid;
        public List<Vector2i> ResultShape;
        public EntProtoId SpawnId;

        public void Execute()
        {
            System.SpawnShape(Grid, ResultShape, SpawnId);
        }
    }
}
