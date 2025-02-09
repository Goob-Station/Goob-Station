using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Server.NPC.Pathfinding;
using Content.Shared._Goobstation.MULE.Components;
using Content.Shared.Interaction;

namespace Content.Server._Goobstation.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class SetDropoffAsOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private EntityLookupSystem _lookup = default!;
    private PathfindingSystem _pathfinding = default!;

    /// <summary>
    /// Target entity to travel too
    /// </summary>
    [DataField("targetKey", required: true)]
    public string TargetKey = string.Empty;

    /// <summary>
    /// Target entitycoordinates to move to.
    /// </summary>
    [DataField("targetMoveKey", required: true)]
    public string TargetMoveKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _lookup = sysManager.GetEntitySystem<EntityLookupSystem>();
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!_entManager.TryGetComponent<MuleComponent>(owner, out var mule))
            return (false, null);

        //Needed to make sure it doesn't sometimes stop right outside it's interaction range
        var pathRange = SharedInteractionSystem.InteractionRange - 1f;
        var path = await _pathfinding.GetPath(owner, entity, pathRange, cancelToken);

        if (path.Result == PathResult.NoPath)
            return (false, null);

        return (true, new Dictionary<string, object>()
        {
            {TargetKey, entity},
            {TargetMoveKey, _entManager.GetComponent<TransformComponent>(entity).Coordinates},
            {NPCBlackboard.PathfindKey, path},
        });
    }
}
