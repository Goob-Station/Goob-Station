using Content.Goobstation.Shared.Nullspace;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Nullspace.HTN;

public sealed partial class NullspaceClownStateOperator : HTNOperator
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IGameTiming _timing = default!;

    private NullspaceSystem _nullspace = default!;

    private static readonly TimeSpan StingerCooldown = TimeSpan.FromSeconds(30);

    [DataField(required: true)]
    public NullspaceClownState State;

    [DataField]
    public string TargetKey = "Target";

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _nullspace = sysManager.GetEntitySystem<NullspaceSystem>();
    }

    public override void Startup(NPCBlackboard blackboard)
    {
        base.Startup(blackboard);

        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var spawned = blackboard.GetValueOrDefault<TimeSpan>(NullspaceBlackboard.SpawnTime, _entManager);
        EntityUid? target = null;
        if (State == NullspaceClownState.Chase && blackboard.TryGetValue<EntityUid>(TargetKey, out var chased, _entManager))
            target = chased;

        if (!_nullspace.SetClownState(owner, State, spawned, target) || target is not { } quarry)
            return;

        var now = _timing.CurTime;
        if (blackboard.TryGetValue<TimeSpan>(NullspaceBlackboard.LastStinger, out var last, _entManager)
            && now - last <= StingerCooldown)
        {
            return;
        }

        blackboard.SetValue(NullspaceBlackboard.LastStinger, now);
        _nullspace.Stinger(quarry);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        return HTNOperatorStatus.Finished;
    }
}
