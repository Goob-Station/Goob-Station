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

        if(mule.CurrentTarget == EntityUid.Invalid)
            return (false, null);

        var navb = _entManager.GetEntityData(_entManager.GetNetEntity(mule.CurrentTarget));
        Logger.Debug(navb.Item2.EntityName);

        return (true, new Dictionary<string, object>()
        {
            {TargetKey, mule.CurrentTarget},
            {TargetMoveKey, _entManager.GetComponent<TransformComponent>(mule.CurrentTarget).Coordinates},
        });
    }
}
