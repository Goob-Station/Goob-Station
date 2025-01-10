using Content.Server._Goobstation.Blob.NPC.BlobPod;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared._Goobstation.Blob.Components;

namespace Content.Server_Goobstation.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class BlobPodZombifyOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private BlobPodSystem _blobPodSystem = default!;

    [DataField("zombifyKey")]
    public string ZombifyKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _blobPodSystem = sysManager.GetEntitySystem<BlobPodSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(ZombifyKey);

        if (!target.IsValid() || _entManager.Deleted(target))
            return HTNOperatorStatus.Failed;

        if (!_entManager.TryGetComponent<BlobPodComponent>(owner, out var pod))
            return HTNOperatorStatus.Failed;

        if (pod.ZombifiedEntityUid != null)
            return HTNOperatorStatus.Continuing;

        if (pod.IsZombifying)
            return HTNOperatorStatus.Continuing;

        if (pod.ZombifyTarget == null)
        {
            if (_blobPodSystem.NpcStartZombify(owner, target, pod))
                return HTNOperatorStatus.Continuing;
            else
                return HTNOperatorStatus.Failed;
        }

        pod.ZombifyTarget = null;
        return HTNOperatorStatus.Finished;
    }
}
