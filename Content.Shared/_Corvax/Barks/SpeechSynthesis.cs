using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Corvax.Speech.Synthesis.Components;

/// <summary>
/// Applies bark sounds to the essence.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpeechSynthesisComponent : Component
{
    /// <summary>
    /// A voice prototype for barks.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("voice", customTypeSerializer: typeof(PrototypeIdSerializer<BarkPrototype>))]
    public string? VoicePrototypeId { get; set; }

    /// <summary>
    /// The speed of sound playback.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("playbackSpeed")]
    public float PlaybackSpeed { get; set; } = 1.0f;

    /// <summary>
    /// The tone of the sound.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("pitch")]
    public float Pitch { get; set; } = 1.0f;

    /// <summary>
    /// Expressiveness of speech.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("expression")]
    public float Expression { get; set; } = 1.0f;
}
