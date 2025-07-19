using Content.Shared._Lavaland.Tile.Shapes;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Lavaland.Tile;

public sealed class TileShapeSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public void SpawnTileShape(TileShape shape, EntityUid target, EntProtoId spawnId, out List<EntityUid> spawned)
    {
        SpawnTileShape(shape, Transform(target).Coordinates, spawnId, out spawned);
    }

    public void SpawnTileShape(TileShape shape, EntityCoordinates coords, EntProtoId spawnId, out List<EntityUid> spawned)
    {
        spawned = new List<EntityUid>();
        var grid = _xform.GetGrid(coords);

        if (!TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        var center = _map.CoordinatesToTile(grid.Value, gridComp, coords);

        var result = shape.GetShape(_random.GetRandom(), _protoMan, center);
        foreach (var tile in result)
        {
            var pos = _map.GridTileToLocal(grid.Value, gridComp, tile);
            var ent = PredictedSpawnAtPosition(spawnId, pos);
            spawned.Add(ent);
        }
    }
}
