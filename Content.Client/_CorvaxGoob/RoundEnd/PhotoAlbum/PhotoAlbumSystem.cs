using Content.Shared._CorvaxGoob.RoundEnd;

namespace Content.Client._CorvaxGoob.RoundEnd.PhotoAlbum;

public sealed class PhotoAlbumSystem : EntitySystem
{
    public List<AlbumData>? Albums { get; private set; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PhotoAlbumEvent>(OnStationImagesReceived);
    }

    private void OnStationImagesReceived(PhotoAlbumEvent ev) => Albums = ev.Albums;

    public void ClearImagesData() => Albums = null;
}
