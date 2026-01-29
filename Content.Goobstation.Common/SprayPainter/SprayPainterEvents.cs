using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.SprayPainter;

[Serializable, NetSerializable]
public sealed class SprayPainterSetDecalColorPickerMessage(bool toggle) : BoundUserInterfaceMessage
{
    public bool Toggle = toggle;
}
