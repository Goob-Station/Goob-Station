using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.Photo;

[Serializable, NetSerializable]
public sealed class PhotoCardUiState : BoundUserInterfaceState
{
    public byte[]? ImageData { get; }

    public PhotoCardUiState(byte[]? imageData)
    {
        ImageData = imageData;
    }
}

[Serializable, NetSerializable]
public enum PhotoCardUiKey : byte
{
    Key
}
