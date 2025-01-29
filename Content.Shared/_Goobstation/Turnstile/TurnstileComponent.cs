using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Turnstile;

/// <summary>
/// This is used for managing turnstile data.
/// </summary>
[RegisterComponent]
public sealed partial class TurnstileComponent : Component
{
    /// <summary>
    /// Direction that's allowed for entity passthrough.
    /// </summary>
    public Vector2 AllowedDirection = new Vector2(0, 1); // North

    /// <summary>
    /// Sound to play if the turnstile access is accepted.
    /// </summary>
    [DataField]
    public SoundSpecifier? AccessSound;

    /// <summary>
    /// Sound to play if the turnstile access is denied.
    /// </summary>
    [DataField]
    public SoundSpecifier? DenySound;

    [DataField]
    public string OpeningSpriteState = "operate";

    [DataField]
    public string DenySpriteState = "deny";

    [DataField]
    public float AnimationTime = 1f;

    [DataField]
    public EntityUid? PassingThrough;
}

[Serializable,NetSerializable]
public enum TurnstileVisuals : byte
{
    State,
}
[Serializable,NetSerializable]
public enum TurnstileVisualState : byte
{
    Base,
    Allow,
    Deny,
}
