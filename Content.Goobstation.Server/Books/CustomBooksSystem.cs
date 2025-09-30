using Content.Goobstation.Shared.Books;
using Content.Server.Administration.Managers;
using Content.Server.Audio;
using Content.Server.Chat.Managers;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Paper;
using Content.Shared.Placeable;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Books;

public sealed partial class CustomBooksSystem : SharedCustomBooksSystem
{
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    private List<BookData> _pendingBooks = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BookBinderComponent, CreateBookMessage>(OnCreateBookMessage);
        SubscribeLocalEvent<BookBinderComponent, EjectBinderPageMessage>(OnEjectBinderPage);

        SubscribeLocalEvent<BookScannerComponent, ItemPlacedEvent>(OnBookPlaced);
        SubscribeLocalEvent<BookScannerComponent, ItemRemovedEvent>(OnBookRemoved);
        SubscribeLocalEvent<BookScannerComponent, StartBookScanMessage>(OnStartScan);
    }

    private void OnCreateBookMessage(Entity<BookBinderComponent> ent, ref CreateBookMessage args)
    {
        Container.EmptyContainer(ent.Comp.PaperContainer, true);
        UpdateBinderUi(ent);

        var book = Spawn("TestCustomBook", Transform(ent.Owner).Coordinates);
        var comp = EnsureComp<CustomBookComponent>(book);

        comp.Author = args.AuthorName;
        comp.Genre = args.Genre;
        comp.Title = args.Title;
        comp.Pages = new(args.Pages);
        comp.Binding = args.Binding;

        RegenerateBook((book, comp));
    }

    private void OnEjectBinderPage(Entity<BookBinderComponent> ent, ref EjectBinderPageMessage args)
    {
        Container.TryRemoveFromContainer(GetEntity(args.Page), true);
        UpdateBinderUi(ent);
    }

    private void OnBookPlaced(Entity<BookScannerComponent> ent, ref ItemPlacedEvent args)
    {
        if (!TryComp<CustomBookComponent>(args.OtherEntity, out var comp))
            return;

        _ambient.SetAmbience(ent.Owner, false);
        ent.Comp.IsScanning = false;
        ent.Comp.Book = args.OtherEntity;
        UI.SetUiState(ent.Owner, BookScannerUiKey.Key, new BookScannerUiState(comp.Author, comp.Genre, comp.Title, ent.Comp.NextScan <= _timing.CurTime));
    }

    private void OnBookRemoved(Entity<BookScannerComponent> ent, ref ItemRemovedEvent args)
    {
        _ambient.SetAmbience(ent.Owner, false);
        ent.Comp.IsScanning = false;
        ent.Comp.Book = null;
        UI.SetUiState(ent.Owner, BookScannerUiKey.Key, new BookScannerUiState("", "", "", ent.Comp.NextScan <= _timing.CurTime));
    }

    private void OnStartScan(Entity<BookScannerComponent> ent, ref StartBookScanMessage args)
    {
        if (!ent.Comp.Book.HasValue)
            return;

        _ambient.SetAmbience(ent.Owner, true);
        ent.Comp.ScanEndTime = _timing.CurTime + TimeSpan.FromSeconds(15);
        ent.Comp.IsScanning = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BookScannerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsScanning || !TryComp<CustomBookComponent>(comp.Book, out var bookComp))
                continue;

            if (comp.ScanEndTime > _timing.CurTime)
                continue;

            SaveBook((comp.Book.Value, bookComp));
            comp.NextScan = _timing.CurTime + TimeSpan.FromMinutes(15);
        }
    }

    private void SaveBook(Entity<CustomBookComponent> ent)
    {
        var data = new BookData(ent.Comp.Title, ent.Comp.Genre, ent.Comp.Author, ent.Comp.Desc, ent.Comp.Pages);
        _pendingBooks.Add(data);

        _chat.SendAdminAnnouncement(Loc.GetString("pending-book-chat-notify", ("title", ent.Comp.Title)));
        _audio.PlayGlobal(new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg"), Filter.Empty().AddPlayers(_adminManager.ActiveAdmins), false, AudioParams.Default.WithVolume(-8f));
    }
}
