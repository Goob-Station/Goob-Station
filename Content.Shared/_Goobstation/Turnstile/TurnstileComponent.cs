using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Turnstile;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class TurnstileComponent : Component
{
    /// <summary>
    /// Direction that's allowed for entity passthrough.
    /// </summary>
    [DataField("turnstileDirection")]
    public Vector2 AllowedDirection = new Vector2(0, 1); // North

    /// <summary>
    /// Sound to play if the turnstile access is accepted.
    /// </summary>
    public SoundSpecifier? AccessSound;

    /// <summary>
    /// Sound to play if the turnstile access is denied.
    /// </summary>
    [DataField]
    public SoundSpecifier? DenySound;

    /// <summary>
    /// Prototype name to search for.
    /// </summary>
    [DataField]
    public bool PassthroughAllowed;
}
