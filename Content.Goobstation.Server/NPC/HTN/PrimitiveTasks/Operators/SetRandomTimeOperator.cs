using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class SetRandomTimeOperator : HTNOperator
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;

    [DataField(required: true)]
    public string TargetKey = string.Empty;

    [DataField]
    public float MinSeconds;

    [DataField(required: true)]
    public float MaxSeconds;

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var delay = TimeSpan.FromSeconds(_random.NextFloat(MinSeconds, MaxSeconds));
        blackboard.SetValue(TargetKey, _timing.CurTime + delay);
        return HTNOperatorStatus.Finished;
    }
}
