using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;

public sealed partial class ForceHandcuffOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    private SharedCuffableSystem _cuffable = default!;
    private SharedAudioSystem _audio = default!;

    [DataField(required: true)]
    public string TargetKey = string.Empty;

    [DataField]
    public string? TargetArrestedSoundKey;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _cuffable = sysManager.GetEntitySystem<SharedCuffableSystem>();
        _audio = sysManager.GetEntitySystem<SharedAudioSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entMan) || _entMan.Deleted(target))
            return HTNOperatorStatus.Failed;

        if (!_entMan.TryGetComponent<CanForceHandcuffComponent>(owner, out var canForceCuff))
            return HTNOperatorStatus.Failed;

        if (canForceCuff.Container?.ContainedEntities.Count > 0)
            return HTNOperatorStatus.Continuing;

        if (!_cuffable.ForceCuff(canForceCuff, target, owner))
            return HTNOperatorStatus.Failed;

        if (TargetArrestedSoundKey != null && blackboard.TryGetValue<SoundSpecifier>(TargetArrestedSoundKey, out var targetArrestedSound, _entMan))
            _audio.PlayPvs(targetArrestedSound, owner);

        return HTNOperatorStatus.Finished;
    }
}
