using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Photo;

[Serializable, NetSerializable]
public sealed class PhotoLabelledMessage : BoundUserInterfaceMessage
{
    public string NewLabel = "";
}
