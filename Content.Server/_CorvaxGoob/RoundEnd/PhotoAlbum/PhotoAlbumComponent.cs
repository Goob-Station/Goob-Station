namespace Content.Server._CorvaxGoob.RoundEnd.PhotoAlbum;

[RegisterComponent]
public sealed partial class PhotoAlbumComponent : Component
{
    [DataField]
    public string ContainerId { get; set; } = "storagebase";
}
