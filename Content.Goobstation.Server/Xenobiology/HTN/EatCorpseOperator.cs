using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.DoAfter;

namespace Content.Goobstation.Server.Xenobiology.HTN;

public sealed partial class EatCorpseOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private EatCorpseSystem _eatCorpse = default!;
    private SharedDoAfterSystem _doAfter = default!;

    [DataField]
    public string CorpseKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _eatCorpse = sysManager.GetEntitySystem<EatCorpseSystem>();
        _doAfter = sysManager.GetEntitySystem<SharedDoAfterSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        base.Update(blackboard, frameTime);

        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var target = blackboard.GetValue<EntityUid>(CorpseKey);

        if (!_entManager.TryGetComponent<CorpseEaterComponent>(owner, out var eater))
            return HTNOperatorStatus.Failed;

        if (eater.LastDoAfterId is { } doAfterId)
        {
            var status = _doAfter.GetStatus(doAfterId);
            if (status == DoAfterStatus.Running)
                return HTNOperatorStatus.Continuing;

            eater.LastDoAfterId = null;
            if (status == DoAfterStatus.Finished)
                return HTNOperatorStatus.Finished;

            return HTNOperatorStatus.Failed;
        }

        if (_eatCorpse.TryEatCorpse(owner, target, eater))
        {
            return HTNOperatorStatus.Continuing;
        }

        return HTNOperatorStatus.Failed;
    }
}
