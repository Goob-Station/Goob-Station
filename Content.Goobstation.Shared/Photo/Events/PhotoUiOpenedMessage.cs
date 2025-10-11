using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Photo;

[Serializable, NetSerializable]
public sealed class PhotoUiOpenedMessage : BoundUserInterfaceMessage
{
    public MapId Map;

    public Vector2 Offset;

    public string Label = "";
}
