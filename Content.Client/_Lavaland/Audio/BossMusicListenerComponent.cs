using Content.Shared._Lavaland.Audio;
using Robust.Shared.Prototypes;

namespace Content.Client._Lavaland.Audio;

/// <summary>
/// State handler.
/// </summary>
[RegisterComponent]
public sealed partial class BossMusicListenerComponent : Component
{
    [ViewVariables]
    public ProtoId<BossMusicPrototype> Music;

    [ViewVariables]
    public EntityUid Boss;

    [ViewVariables]
    public TimeSpan? StartAt;

    [ViewVariables]
    public EntityUid? Stream;

    [ViewVariables]
    public float VolumeOffset;

    [ViewVariables]
    public bool Paused;
}
