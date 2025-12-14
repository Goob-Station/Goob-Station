using Content.Shared._CorvaxGoob.RoundEnd;

namespace Content.Client._CorvaxGoob.RoundEnd.PhotoAlbum;

public sealed class PhotoAlbumSystem : EntitySystem
{
    public Dictionary<byte[], string?>? Images { get; private set; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PhotoAlbumEvent>(OnStationImagesReceived);
    }

    private void OnStationImagesReceived(PhotoAlbumEvent ev) => Images = ev.Images;

    public void ClearImagesData() => Images = null;
}
