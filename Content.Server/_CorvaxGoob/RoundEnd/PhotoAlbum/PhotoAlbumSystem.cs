using Content.Server._CorvaxGoob.Photo;
using Content.Server.GameTicking;
using Content.Shared._CorvaxGoob.RoundEnd;
using Content.Shared.Labels.Components;
using Robust.Server.Containers;

namespace Content.Server._CorvaxGoob.RoundEnd.PhotoAlbum;
public sealed class PhotoAlbumSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent args)
    {
        Dictionary<byte[], string?>? imagesData = new Dictionary<byte[], string?>();
        var query = EntityQueryEnumerator<PhotoAlbumComponent>();

        while (query.MoveNext(out var uid, out var photoAlbum)) // query all photoalbums and send photos them to players
        {
            if (!_container.TryGetContainer(uid, photoAlbum.ContainerId, out var container))
                continue;

            foreach (var item in container.ContainedEntities)
            {
                if (!TryComp<PhotoCardComponent>(item, out var photoCard))
                    continue;

                if (photoCard.ImageData is null)
                    continue;

                string? label = default;

                if (TryComp<LabelComponent>(item, out var labelComp))
                    label = labelComp.CurrentLabel;

                imagesData.Add(photoCard.ImageData, label);
            }
        }

        if (imagesData.Count > 0)
            RaiseNetworkEvent(new PhotoAlbumEvent(imagesData));
    }
}
