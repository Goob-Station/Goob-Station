using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FadingInComponent : Component
{
    /// <summary>
    /// Time in seconds for the sprite to fade in from 0 → 1
    /// </summary>
    [DataField]
    public float FadeInTime = 1f;

    /// <summary>
    /// Tracks elapsed time since spawn
    /// </summary>
    [ViewVariables]
    public float Elapsed = 0f;

    /// <summary>
    /// True after the sprite has fully faded in
    /// </summary>
    [ViewVariables]
    public bool Finished => Elapsed >= FadeInTime;
}
