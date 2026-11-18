using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class PlaySoundEffectOperator : HTNOperator
{
    private SharedAudioSystem _audio = default!;

    [DataField(required: true)]
    public SoundSpecifier Sound = default!;

    [DataField]
    public ProtoId<AudioPresetPrototype>? Effect;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _audio = sysManager.GetEntitySystem<SharedAudioSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var playing = _audio.PlayPvs(Sound, blackboard.GetValue<EntityUid>(NPCBlackboard.Owner));
        if (playing is { } audio
            && Effect is { } effect
            && _audio.Auxiliaries.TryGetValue(effect, out var auxiliary))
        {
            _audio.SetAuxiliary(audio.Entity, audio.Component, auxiliary);
        }

        return HTNOperatorStatus.Finished;
    }
}
