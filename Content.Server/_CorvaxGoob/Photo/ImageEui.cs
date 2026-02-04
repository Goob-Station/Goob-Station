using Content.Server.EUI;
using Content.Shared._CorvaxGoob.Photo;

namespace Content.Server._CorvaxGoob.Photo;

public sealed class ImageEui : BaseEui
{
    private byte[] _image;
    public ImageEui(byte[] image)
    {
        _image = image;
    }
    public override ImageEuiState GetNewState()
    {
        return new ImageEuiState(_image);
    }
}
