using Content.Goobstation.Server.Blob.NPC.BlobPod;
using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;

namespace Content.Goobstation.Server.Xenobiology.HTN;

public sealed partial class SlimeLatchOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private SlimeMobActionsSystem _slimeMobActions = default!;

    [DataField]
    public string LatchKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _slimeMobActions = sysManager.GetEntitySystem<SlimeMobActionsSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(LatchKey);

        return _entManager.TryGetComponent<SlimeComponent>(owner, out var slime)
               && target.IsValid()
               && !_entManager.Deleted(target)
               && target != slime.LatchedTarget
               && target != slime.Tamer
               && _slimeMobActions.NpcTryLatch(owner, target, slime)
            ? HTNOperatorStatus.Finished
            : HTNOperatorStatus.Failed;
    }
}
