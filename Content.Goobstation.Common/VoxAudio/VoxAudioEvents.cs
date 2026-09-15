using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.VoxAudio;

[Serializable, NetSerializable]
public sealed class VoxPlayMessage : EntityEventArgs
{
    public readonly string Message;
    public readonly List<ProtoId<VoxVoicePrototype>> VoiceSet;
    public readonly float? Delay;
    public readonly float? MaxRuntime;
    public readonly NetEntity? TargetNuid;

    public VoxPlayMessage(string message, List<ProtoId<VoxVoicePrototype>> voiceSet, float? delay, NetEntity? nuid)
    {
        Message = message;
        VoiceSet = voiceSet;
        Delay = delay;
        TargetNuid = nuid;
    }
}
