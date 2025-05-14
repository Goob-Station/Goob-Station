namespace Content.Goobstation.Shared.OverlaysAnimation.Components;

/// <summary>
/// Something that gets drawn on the AnimationsOverlay.
/// </summary>
/// <remarks>
/// Note: Multiple components inherited from this shouldn't be assigned to one entity normally,
/// but if you're too lazy to copy animations, you can change the offset or rendering layer and
/// this entity will work with it as a single object.
/// Consider this opportunity as legalized Shitcode.
/// </remarks>
public abstract partial class OverlayObjectComponent : Component
{
    [DataField]
    public Vector2i Offset;

    [DataField]
    public Angle Angle;

    [DataField]
    public int Layer;
}
