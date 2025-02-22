using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Audio;

/// <summary>
/// Sent to some client to turn the boss music on (scary)
/// </summary>
[ImplicitDataDefinitionForInheritors]
[Serializable, NetSerializable]
public sealed partial class BossMusicStartupEvent : EntityEventArgs
{
    public BossMusicStartupEvent(ProtoId<BossMusicPrototype> musicId)
    {
        MusicId = musicId;
    }

    public ProtoId<BossMusicPrototype> MusicId;
}

/// <summary>
/// Sent to some client to turn the boss music off (no scary)
/// </summary>
[ImplicitDataDefinitionForInheritors]
[Serializable, NetSerializable]
public sealed partial class BossMusicStopEvent : EntityEventArgs;
