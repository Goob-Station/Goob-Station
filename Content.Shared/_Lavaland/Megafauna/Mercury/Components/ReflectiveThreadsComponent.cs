using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// 
/// </summary>

[RegisterComponent]
public sealed partial class ReflectiveThreadsComponent : Component
{
    /// <summary>
    /// Prototype spawned for cool.
    /// </summary>
    [DataField]
    public EntProtoId EffectPrototype = "ORTReflectiveThreadsEffect";
    public EntityUid? EffectEntity;

    /// <summary>
    /// Sound played.
    /// </summary>
    [DataField]
    public SoundSpecifier ReflectSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/veilin.ogg");

    [DataField]
    public float ReflectDuration = 5f;

    public bool Reflecting; // Reflect is active
    public float Accumulator;

}
