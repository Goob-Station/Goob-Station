using Content.Server._CorvaxGoob.Photo;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Shared._CorvaxGoob.RoundEnd;
using Content.Shared.Examine;
using Content.Shared.Labels.Components;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.Player;

namespace Content.Server._CorvaxGoob.RoundEnd.PhotoAlbum;
public sealed class PhotoAlbumSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly TagSystem _tags = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);

        SubscribeLocalEvent<PhotoAlbumComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<PhotoAlbumComponent, ExaminedEvent>(OnExamine);
    }

    private void OnGetVerbs(Entity<PhotoAlbumComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null || entity.Comp.IsSigned)
            return;

        if (args.Using is not { } pen || !_tags.HasTag(pen, "Write"))
            return;

        var target = args.Target;
        var user = args.User;

        var verb = new Verb
        {
            Text = Loc.GetString("photoalbum-sign-verb"),
            Act = () => VerbSignPhotoAlbum(entity, user)
        };
        args.Verbs.Add(verb);
    }

    private void OnExamine(Entity<PhotoAlbumComponent> entity, ref ExaminedEvent args)
    {
        if (!entity.Comp.IsSigned)
            return;

        args.PushMarkup(Loc.GetString("photoalbum-signed-examine"));
    }

    private void VerbSignPhotoAlbum(Entity<PhotoAlbumComponent> entity, EntityUid user)
    {
        entity.Comp.IsSigned = true;

        entity.Comp.SignerUid = user;

        if (_player.TryGetSessionByEntity(user, out var session))
            entity.Comp.SignerUsername = session.Data.UserName;

        _popup.PopupEntity(Loc.GetString("photoalbum-signed", ("user", user)), entity);
        _audio.PlayPvs(entity.Comp.SignSound, entity);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent args)
    {
        List<AlbumData>? albums = new();
        var query = EntityQueryEnumerator<PhotoAlbumComponent>();

        while (query.MoveNext(out var uid, out var photoAlbum)) // query all photoalbums and send photos them to players
        {
            if (!_container.TryGetContainer(uid, photoAlbum.ContainerId, out var container))
                continue;

            Dictionary<byte[], string?> photos = new();

            string? authorCKey = default;
            string? authorName = default;

            foreach (var item in container.ContainedEntities)
            {
                if (!TryComp<PhotoCardComponent>(item, out var photoCard))
                    continue;

                if (photoCard.ImageData is null)
                    continue;

                string? label = default;

                if (TryComp<LabelComponent>(item, out var labelComp))
                    label = labelComp.CurrentLabel;

                photos.Add(photoCard.ImageData, label);
            }

            if (photos.Count == 0)
                continue;

            if (photoAlbum.IsSigned)
            {
                if (photoAlbum.SignerUid is not null && Exists(photoAlbum.SignerUid))
                    authorName = MetaData(photoAlbum.SignerUid.Value).EntityName;

                authorCKey = photoAlbum.SignerUsername;
            }

            albums.Add(new AlbumData(photos, authorCKey, authorName));
        }

        if (albums.Count > 0)
            RaiseNetworkEvent(new PhotoAlbumEvent(albums));
    }
}
