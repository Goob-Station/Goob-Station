using Content.Shared.Eui;
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

[Serializable, NetSerializable]
public sealed class CaptureScreenRequestEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class CaptureScreenResponseEvent : EntityEventArgs
{
    public byte[]? Image = default;

    public CaptureScreenResponseEvent(byte[] image)
    {
        this.Image = image;
    }
}

[Serializable, NetSerializable]
public sealed class ImageEuiState : EuiStateBase
{
    public byte[]? Image;

    public ImageEuiState(byte[] image)
    {
        Image = image;
    }
}
