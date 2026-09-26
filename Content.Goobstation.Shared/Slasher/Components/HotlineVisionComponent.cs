using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Gives the user drunk cam / a rainbow esq fov look.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HotlineVisionComponent : Component
{
    [DataField]
    public float IdleSwayDegrees = 0.85f;

    [DataField]
    public float IdleSwayRate = 1.0f;

    [DataField]
    public float MoveSwayDegrees = 2.25f;

    [DataField]
    public float MoveSwayRate = 2.1f;

    [DataField]
    public float MaxLeanDegrees = 2.5f;

    [DataField]
    public float LeanLerpSpeed = 3.5f;

    /// <summary>
    /// How fast the sway switches between moving and idle.
    /// </summary>
    [DataField]
    public float MoveBlendSpeed = 2.5f;

    [ViewVariables]
    public float SwayClock;

    [ViewVariables]
    public float Lean;

    [ViewVariables]
    public float MoveFactor;
}
