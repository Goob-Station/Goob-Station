using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Vox;

[Serializable, NetSerializable]
public sealed class VoxPlayMessage(string message, List<ProtoId<VoxVoicePrototype>> voiceSet, float? delay, NetEntity? nuid) : EntityEventArgs
{
    public readonly string Message = message;
    public readonly List<ProtoId<VoxVoicePrototype>> VoiceSet = voiceSet;
    public readonly float? Delay = delay;
    public readonly float? MaxRuntime;
    public readonly NetEntity? TargetNuid = nuid;
}
