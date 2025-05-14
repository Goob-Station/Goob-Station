using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.OverlaysAnimation.Animations;

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class TransformOverlayAnimation : OverlayAnimation
{
    [DataField]
    public Vector2i StartPos;

    [DataField]
    public Vector2i EndPos;
}
