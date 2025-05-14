using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.OverlaysAnimation.Animations;

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class ColorOverlayAnimation : OverlayAnimation
{
    [DataField]
    public Color StartColor = Color.White;

    [DataField]
    public Color EndColor = Color.White;
}
