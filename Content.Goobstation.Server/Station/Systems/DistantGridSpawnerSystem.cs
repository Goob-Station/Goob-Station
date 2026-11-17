using Content.Goobstation.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using System.Numerics;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Station.Systems;

public sealed partial class DistantGridSpawnerSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MapLoaderSystem _mapLoader = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private StationSystem _station = default!;

    private const int MaxAttempts = 10; // No reason for this to live in the comp don't @ me

    private List<Entity<MapGridComponent>> _grids = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<DistantGridSpawnerComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnStationPostInit(Entity<DistantGridSpawnerComponent> ent, ref StationPostInitEvent args)
    {
        var stationGrid = _station.GetLargestGrid(args.Station.AsNullable());
        if (stationGrid is null)
            return;

        var xform = Transform(stationGrid.Value);
        if (xform.MapUid is null)
            return;

        var mapId = xform.MapID;
        var center = _transform.GetWorldPosition(xform);

        foreach (var entry in ent.Comp.Entries)
        {
            if (!_random.Prob(entry.Chance))
                continue;

            TrySpawnEntry(entry, mapId, center);
        }
    }

    private void TrySpawnEntry(DistantGridEntry entry, MapId mapId, Vector2 center)
    {
        for (var i = 0; i < MaxAttempts; i++)
        {
            var angle = _random.NextAngle();
            var distance = _random.NextFloat(entry.MinDistance, entry.MaxDistance);
            var position = center + angle.ToVec() * distance;

            if (HasCollisions(mapId, Box2.CenteredAround(position, new Vector2(entry.SafetyRadius * 2f))))
                continue;

            _mapLoader.TryLoadGrid(mapId, entry.Path, out _, offset: position);
            return;
        }
    }

    private bool HasCollisions(MapId mapId, Box2 bounds)
    {
        _grids.Clear();
        _map.FindGridsIntersecting(mapId, bounds, ref _grids);
        return _grids.Count > 0;
    }
}
