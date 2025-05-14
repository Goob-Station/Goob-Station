using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.OverlaysAnimation.Animations;

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class ScaleOverlayAnimation : OverlayAnimation
{
    [DataField]
    public float StartScale = 1.0f;

    [DataField]
    public float EndScale = 1.0f;
}
