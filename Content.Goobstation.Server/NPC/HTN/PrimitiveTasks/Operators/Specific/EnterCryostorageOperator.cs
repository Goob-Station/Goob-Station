using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Bed.Cryostorage;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class EnterCryostorageOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private SharedContainerSystem _container = default!;

    [DataField(required: true)]
    public string TargetKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _container = sysManager.GetEntitySystem<SharedContainerSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager) ||
            !_entManager.TryGetComponent<CryostorageComponent>(target, out var cryo))
        {
            return HTNOperatorStatus.Failed;
        }

        if (!_container.TryGetContainer(target, cryo.ContainerId, out var container))
            return HTNOperatorStatus.Failed;

        if (container.Contains(owner))
            return HTNOperatorStatus.Finished;

        return _container.Insert(owner, container)
            ? HTNOperatorStatus.Finished
            : HTNOperatorStatus.Failed;
    }
}
