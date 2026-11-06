using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class PickNearOperator : HTNOperator
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IRobustRandom _random = default!;

    private SharedMapSystem _map = default!;
    private SharedTransformSystem _transform = default!;
    private TurfSystem _turf = default!;

    [DataField]
    public string? OriginEntityKey;

    [DataField]
    public string? OriginCoordinatesKey;

    [DataField(required: true)]
    public string TargetKey = string.Empty;

    [DataField]
    public int Radius = 4;

    [DataField]
    public int Attempts = 16;

    [DataField]
    public string? ExactKey;

    [DataField]
    public CollisionGroup BlockedBy = CollisionGroup.Impassable;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _map = sysManager.GetEntitySystem<SharedMapSystem>();
        _transform = sysManager.GetEntitySystem<SharedTransformSystem>();
        _turf = sysManager.GetEntitySystem<TurfSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        if (!TryGetOrigin(blackboard, out var origin)
            || _transform.GetGrid(origin) is not { } gridUid
            || !_entManager.TryGetComponent<MapGridComponent>(gridUid, out var grid))
        {
            return (false, null);
        }

        var center = _map.TileIndicesFor(gridUid, grid, origin);
        var exact = ExactKey != null && blackboard.GetValueOrDefault<bool>(ExactKey, _entManager);
        var picked = exact
            ? Nearest(gridUid, grid, center)
            : Random(gridUid, grid, center);

        if (picked is not { } tile)
            return (false, null);

        var effects = new Dictionary<string, object>
        {
            { TargetKey, _map.GridTileToLocal(gridUid, grid, tile) },
        };

        if (ExactKey != null)
            effects[ExactKey] = false;

        return (true, effects);
    }

    private Vector2i? Random(EntityUid gridUid, MapGridComponent grid, Vector2i center)
    {
        for (var i = 0; i < Attempts; i++)
        {
            var tile = center + new Vector2i(_random.Next(-Radius, Radius + 1), _random.Next(-Radius, Radius + 1));
            if (Open(gridUid, grid, tile))
                return tile;
        }

        return null;
    }

    private Vector2i? Nearest(EntityUid gridUid, MapGridComponent grid, Vector2i center)
    {
        for (var ring = 0; ring <= Radius; ring++)
        {
            for (var dy = -ring; dy <= ring; dy++)
            {
                for (var dx = -ring; dx <= ring; dx++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != ring)
                        continue;

                    var tile = center + new Vector2i(dx, dy);
                    if (Open(gridUid, grid, tile))
                        return tile;
                }
            }
        }

        return null;
    }

    private bool Open(EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        return _map.TryGetTileRef(gridUid, grid, tile, out var tileRef)
            && !tileRef.Tile.IsEmpty
            && !_turf.IsTileBlocked(gridUid, tile, BlockedBy, grid);
    }

    private bool TryGetOrigin(NPCBlackboard blackboard, out EntityCoordinates origin)
    {
        origin = default;
        if (OriginEntityKey != null
            && blackboard.TryGetValue<EntityUid>(OriginEntityKey, out var entity, _entManager)
            && _entManager.TryGetComponent<TransformComponent>(entity, out var xform))
        {
            origin = xform.Coordinates;
            return true;
        }

        if (OriginCoordinatesKey != null
            && blackboard.TryGetValue<EntityCoordinates>(OriginCoordinatesKey, out var coordinates, _entManager)
            && _entManager.EntityExists(coordinates.EntityId))
        {
            origin = coordinates;
            return true;
        }

        return false;
    }
}
