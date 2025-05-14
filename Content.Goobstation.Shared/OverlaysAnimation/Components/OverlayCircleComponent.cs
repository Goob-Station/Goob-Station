namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

/// <summary>
/// Draws a circle on AnimationsOverlay.
/// Also requires OverlayAnimationComponent to work
/// </summary>
[RegisterComponent]
public sealed partial class OverlayCircleComponent : OverlayObjectComponent
{
    [DataField]
    public float Radius;
}
