using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Random.Helpers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Spawners;

/// <summary>
/// This system handles spawning from a list of entities within a range of the spawning entity.
/// </summary>
public sealed partial class VicinitySpawnerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private TurfSystem _turf = default!;

    private readonly List<EntityCoordinates> _freeSpots = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<VicinitySpawnerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime < comp.NextSpawn)
                continue;

            comp.NextSpawn = curTime + comp.SpawnInterval;
            Dirty(uid, comp);

            TrySpawn((uid, comp));
        }
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<VicinitySpawnerComponent> ent, ref MapInitEvent args)
    {
        TrySpawn(ent);

        ent.Comp.NextSpawn = _timing.CurTime + ent.Comp.SpawnInterval;
        Dirty(ent);
    }

    private void TrySpawn(Entity<VicinitySpawnerComponent> ent)
    {
        var xform = Transform(ent.Owner);
        var center = xform.Coordinates;

        var rand = new Random(SharedRandomExtensions.HashCodeCombine(new List<int> { (int) _timing.CurTick.Value, GetNetEntity(ent.Owner).Id }));

        _freeSpots.Clear();
        if (xform.GridUid is { } gridUid && TryComp<MapGridComponent>(gridUid, out var grid))
        {
            var origin = _map.TileIndicesFor(gridUid, grid, center);
            var range = ent.Comp.OffsetForSpawn;

            for (var x = -range; x <= range; x++)
            {
                for (var y = -range; y <= range; y++)
                {
                    var tile = origin + new Vector2i(x, y);

                    if (!_map.TryGetTileRef(gridUid, grid, tile, out var tileRef) || tileRef.Tile.IsEmpty)
                        continue;

                    if (_turf.IsTileBlocked(gridUid, tile, CollisionGroup.Impassable, grid))
                        continue;

                    _freeSpots.Add(_map.GridTileToLocal(gridUid, grid, tile));
                }
            }
        }

        // Checks how many entities it should spawn and spawns them at the same time in different coordinates.
        for (var i = 0; i < ent.Comp.NumberToSpawn; i++)
        {
            // Falls back to the spawner's own position if there are no free tiles left.
            var coords = center;
            if (_freeSpots.Count > 0)
            {
                var index = rand.Next(_freeSpots.Count);
                coords = _freeSpots[index];
                _freeSpots.RemoveSwap(index);
            }

            var proto = ent.Comp.Prototypes[rand.Next(ent.Comp.Prototypes.Count)];
            PredictedSpawnAtPosition(proto, coords);
        }
    }
}
