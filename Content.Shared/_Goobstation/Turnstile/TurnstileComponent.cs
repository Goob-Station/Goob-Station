using System.Numerics;
using Robust.Client.Animations;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

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

    [DataField]
    public TurnstileVisualState State = TurnstileVisualState.Base;

    [DataField]
    public string OpeningSpriteState = "operate";

    [DataField]
    public string DenySpriteState = "deny";

    [DataField]
    public float AnimationTime = 1f;
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
