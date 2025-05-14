using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.OverlaysAnimation.Animations;

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class FadeOverlayAnimation : OverlayAnimation
{
    [DataField]
    public float StartOpacity = 1.0f;

    [DataField]
    public float EndOpacity = 1.0f;
}
