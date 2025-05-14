using System.Numerics;

namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

/// <summary>
/// Draws a rectangle on AnimationsOverlay.
/// Also requires OverlayAnimationComponent to work
/// </summary>
[RegisterComponent]
public sealed partial class OverlayRectComponent : OverlayObjectComponent
{
    [DataField]
    public Vector2 BoxSize;
}
