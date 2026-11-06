using System.Numerics;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Examine;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class PeekAtTargetOperator : HTNOperator
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;

    private ExamineSystemShared _examine = default!;
    private SharedMapSystem _map = default!;
    private SharedTransformSystem _transform = default!;
    private TurfSystem _turf = default!;

    private const string EndKey = "PeekEnd";

    [DataField(required: true)]
    public string TargetKey = string.Empty;

    [DataField]
    public float MinRange = 5f;

    [DataField]
    public float MaxRange = 9f;

    [DataField]
    public float Duration = 10f;

    [DataField]
    public float LookAwayAngle = 55f;

    [DataField]
    public float VanishDistance = 3f;

    [DataField]
    public float VanishMinRange = 8f;

    [DataField]
    public float VanishMaxRange = 14f;

    [DataField]
    public int VanishAttempts = 24;

    [DataField]
    public CollisionGroup BlockedBy = CollisionGroup.Impassable;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _examine = sysManager.GetEntitySystem<ExamineSystemShared>();
        _map = sysManager.GetEntitySystem<SharedMapSystem>();
        _transform = sysManager.GetEntitySystem<SharedTransformSystem>();
        _turf = sysManager.GetEntitySystem<TurfSystem>();
    }

    public override void Startup(NPCBlackboard blackboard)
    {
        base.Startup(blackboard);
        blackboard.Remove<TimeSpan>(EndKey);

        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)
            || FindPeekSpot(target) is not { } spot)
        {
            return;
        }

        var targetPos = _transform.GetWorldPosition(target);
        _transform.SetMapCoordinates(owner, spot);
        _transform.SetWorldRotation(owner, (targetPos - spot.Position).ToWorldAngle());
        blackboard.SetValue(EndKey, _timing.CurTime + TimeSpan.FromSeconds(Duration));
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        if (!blackboard.TryGetValue<TimeSpan>(EndKey, out var end, _entManager))
            return HTNOperatorStatus.Failed;

        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)
            || !_entManager.EntityExists(target))
        {
            return HTNOperatorStatus.Finished;
        }

        var ownerPos = _transform.GetWorldPosition(owner);
        var targetPos = _transform.GetWorldPosition(target);
        var offset = ownerPos - targetPos;
        _transform.SetWorldRotation(owner, (-offset).ToWorldAngle());

        var facing = _transform.GetWorldRotation(target).ToWorldVec();
        var lookedAway = offset.LengthSquared() > 1e-6f
            && MathF.Acos(Math.Clamp(Vector2.Dot(Vector2.Normalize(offset), facing), -1f, 1f)) > MathHelper.DegreesToRadians(LookAwayAngle);

        if (_timing.CurTime < end
            && !lookedAway
            && offset.Length() > VanishDistance
            && _examine.InRangeUnOccluded(target, owner, MaxRange + VanishMaxRange))
        {
            return HTNOperatorStatus.Continuing;
        }

        Vanish(owner, target);
        return HTNOperatorStatus.Finished;
    }

    private MapCoordinates? FindPeekSpot(EntityUid target)
    {
        var xform = _entManager.GetComponent<TransformComponent>(target);
        if (xform.GridUid is not { } gridUid || !_entManager.TryGetComponent<MapGridComponent>(gridUid, out var grid))
            return null;

        var origin = _transform.GetMapCoordinates(target, xform);
        var facing = _transform.GetWorldRotation(xform).ToWorldVec();
        MapCoordinates? best = null;

        for (var distance = 1f; distance <= MaxRange; distance += 0.5f)
        {
            var point = new MapCoordinates(origin.Position + facing * distance, origin.MapId);
            var tile = _map.TileIndicesFor(gridUid, grid, point);
            if (!Open(gridUid, grid, tile))
                break;

            var center = new MapCoordinates(_map.GridTileToWorldPos(gridUid, grid, tile), origin.MapId);
            if (distance >= MinRange && _examine.InRangeUnOccluded(origin, center, MaxRange + 1f, null))
                best = center;
        }

        return best;
    }

    private void Vanish(EntityUid owner, EntityUid target)
    {
        var xform = _entManager.GetComponent<TransformComponent>(target);
        if (xform.GridUid is not { } gridUid || !_entManager.TryGetComponent<MapGridComponent>(gridUid, out var grid))
            return;

        var origin = _transform.GetMapCoordinates(target, xform);
        for (var i = 0; i < VanishAttempts; i++)
        {
            var angle = _random.NextFloat(0f, MathF.Tau);
            var distance = _random.NextFloat(VanishMinRange, VanishMaxRange);
            var point = new MapCoordinates(origin.Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance, origin.MapId);
            var tile = _map.TileIndicesFor(gridUid, grid, point);
            if (!Open(gridUid, grid, tile))
                continue;

            var center = new MapCoordinates(_map.GridTileToWorldPos(gridUid, grid, tile), origin.MapId);
            if (_examine.InRangeUnOccluded(origin, center, VanishMaxRange + 1f, null))
                continue;

            _transform.SetMapCoordinates(owner, center);
            return;
        }
    }

    private bool Open(EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        return _map.TryGetTileRef(gridUid, grid, tile, out var tileRef)
            && !tileRef.Tile.IsEmpty
            && !_turf.IsTileBlocked(gridUid, tile, BlockedBy, grid);
    }
}
