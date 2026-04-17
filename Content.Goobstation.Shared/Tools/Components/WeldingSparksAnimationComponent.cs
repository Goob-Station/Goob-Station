using System.Numerics;

namespace Content.Goobstation.Shared.Tools;

/// <summary>
/// If an entity with this component is welded by a tool with <see cref="WeldingSparksComponent"/>, the sparks will be animated based on this component's datafields.
/// </summary>
[RegisterComponent]
public sealed partial class WeldingSparksAnimationComponent : Component
{
    /// <summary>
    /// The <see cref="Robust.Client.GameObjects.SpriteComponent.Offset"/> of the sparks effect at the beginning of the animation.
    /// </summary>
    /// <remarks>
    /// If <see cref="EndingOffset"/> is not set, the animation interpolates from this value to the opposite of this value in order to move the sparks across the entity.
    /// </remarks>
    [DataField(required: true)]
    public Vector2 StartingOffset;

    /// <summary>
    /// Optional secondary field to assign the <see cref="Robust.Client.GameObjects.SpriteComponent.Offset"/> of the sparks effect at the end of the animation.
    /// </summary>
    [DataField]
    public Vector2? EndingOffset;
}
