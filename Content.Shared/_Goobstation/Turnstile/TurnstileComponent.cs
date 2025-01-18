using System.Numerics;
using Robust.Shared.Audio;

namespace Content.Shared._Goobstation.Turnstile;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class TurnstileComponent : Component
{
    [DataField("turnstileDirection")]

    public Vector2 AllowedDirection = new Vector2(0, 1); // North

    /// <summary>
    /// Sound to play if the turnstile access is accepted.
    /// </summary>
    [DataField("accessSound")]
    public SoundSpecifier? AccessSound;

    /// <summary>
    /// Sound to play if the turnstile access is denied.
    /// </summary>
    [DataField("denySound")]
    public SoundSpecifier? DenySound;

    /// <summary>
    /// Prototype name to search for.
    /// </summary>
    [DataField("check")]
    public string Check;
}
