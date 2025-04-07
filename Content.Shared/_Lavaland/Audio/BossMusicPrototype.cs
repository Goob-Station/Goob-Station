using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Audio;

/// <summary>
/// Attaches a rules prototype to sound files to play ambience.
/// </summary>
[Prototype("bossMusic")]
public sealed partial class BossMusicPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [DataField("fade")]
    public bool FadeIn;

    [DataField(required: true)]
    public SoundSpecifier Sound = default!;

    [DataField]
    public float FadeInTime = 5f;

    [DataField]
    public float FadeOutTime = 5f;

    /// <summary>
    /// Sets the playback position when the boss is defeated.
    /// </summary>
    [DataField]
    public float? PositionOnEnd;
}
