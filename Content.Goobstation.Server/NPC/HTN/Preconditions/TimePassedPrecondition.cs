using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.NPC.HTN.Preconditions;

public sealed partial class TimePassedPrecondition : HTNPrecondition
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IGameTiming _timing = default!;

    [DataField(required: true)]
    public string Key = string.Empty;

    [DataField]
    public bool Passed = true;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        if (!blackboard.TryGetValue<TimeSpan>(Key, out var time, _entManager))
            return false;

        return _timing.CurTime >= time == Passed;
    }
}
