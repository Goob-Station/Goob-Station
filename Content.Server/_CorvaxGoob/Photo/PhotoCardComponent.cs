namespace Content.Server._CorvaxGoob.Photo;

[RegisterComponent]
public sealed partial class PhotoCardComponent : Component
{
    [DataField]
    public byte[]? ImageData;
}
