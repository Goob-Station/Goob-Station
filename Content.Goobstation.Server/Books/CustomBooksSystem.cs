using Content.Goobstation.Shared.Books;
using Content.Server.Administration.Managers;
using Content.Server.Audio;
using Content.Server.Chat.Managers;
using Content.Server.Database;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Paper;
using Content.Shared.Placeable;
using Content.Shared.Power;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Books;

public sealed partial class CustomBooksSystem : SharedCustomBooksSystem
{
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IServerDbManager _db = default!;

    private Dictionary<int, BookData> _pendingBooks = new();
    private int _nextPendingBook = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BookBinderComponent, MapInitEvent>(OnBinderInit);
        SubscribeLocalEvent<BookBinderComponent, CreateBookMessage>(OnCreateBookMessage);
        SubscribeLocalEvent<BookBinderComponent, EjectBinderPageMessage>(OnEjectBinderPage);

        SubscribeLocalEvent<BookScannerComponent, ItemPlacedEvent>(OnBookPlaced);
        SubscribeLocalEvent<BookScannerComponent, ItemRemovedEvent>(OnBookRemoved);
        SubscribeLocalEvent<BookScannerComponent, StartBookScanMessage>(OnStartScan);
        SubscribeLocalEvent<BookScannerComponent, PowerChangedEvent>(OnScannerPowerChanged);

        SubscribeLocalEvent<BookPrinterComponent, MapInitEvent>(OnPrinterInit);
        SubscribeLocalEvent<BookPrinterComponent, PrintBookMessage>(OnPrinterPrint);
        SubscribeLocalEvent<BookPrinterComponent, PowerChangedEvent>(OnPrinterPowerChanged);

        SubscribeNetworkEvent<ApproveBookMessage>(OnBookApprove);
        SubscribeNetworkEvent<DeclineBookMessage>(OnBookDecline);
    }

    private void OnBinderInit(Entity<BookBinderComponent> ent, ref MapInitEvent args)
    {
        Appearance.SetData(ent.Owner, BookBinderVisuals.Inserting, false);
    }

    private void OnCreateBookMessage(Entity<BookBinderComponent> ent, ref CreateBookMessage args)
    {
        Container.EmptyContainer(ent.Comp.PaperContainer, true);
        _audio.PlayPvs(ent.Comp.BookCreatedSound, ent.Owner);
        UpdateBinderUi(ent);

        var book = Spawn("TestCustomBook", Transform(ent.Owner).Coordinates);
        var comp = EnsureComp<CustomBookComponent>(book);

        comp.Author = args.AuthorName;
        comp.Genre = args.Genre;
        comp.Title = args.Title;
        comp.Pages = new(args.Pages);
        comp.Binding = args.Binding;
        comp.Desc = args.Description;

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
        Appearance.SetData(ent.Owner, BookScannerVisuals.Scanning, false);
        ent.Comp.IsScanning = false;
        ent.Comp.Book = args.OtherEntity;
        UpdateScannerUi(ent);
    }

    private void OnBookRemoved(Entity<BookScannerComponent> ent, ref ItemRemovedEvent args)
    {
        if (args.OtherEntity != ent.Comp.Book)
            return;

        _ambient.SetAmbience(ent.Owner, false);
        Appearance.SetData(ent.Owner, BookScannerVisuals.Scanning, false);
        ent.Comp.IsScanning = false;
        ent.Comp.Book = null;
        UpdateScannerUi(ent);
    }

    private void OnStartScan(Entity<BookScannerComponent> ent, ref StartBookScanMessage args)
    {
        if (!ent.Comp.Book.HasValue)
            return;

        _ambient.SetAmbience(ent.Owner, true);
        Appearance.SetData(ent.Owner, BookScannerVisuals.Scanning, true);
        ent.Comp.ScanEndTime = _timing.CurTime + TimeSpan.FromSeconds(15);
        ent.Comp.IsScanning = true;
        UpdateScannerUi(ent);
    }

    private void OnScannerPowerChanged(Entity<BookScannerComponent> ent, ref PowerChangedEvent args)
    {
        if (!ent.Comp.Book.HasValue || args.Powered)
            return;

        _ambient.SetAmbience(ent.Owner, false);
        Appearance.SetData(ent.Owner, BookScannerVisuals.Scanning, false);
        ent.Comp.IsScanning = false;
        UpdateScannerUi(ent);
    }

    private void OnPrinterInit(Entity<BookPrinterComponent> ent, ref MapInitEvent args)
    {
        Appearance.SetData(ent.Owner, BookPrinterVisuals.Printing, false);
        UpdatePrinterUi(ent);
    }

    private void OnPrinterPrint(Entity<BookPrinterComponent> ent, ref PrintBookMessage args)
    {
        _ambient.SetAmbience(ent.Owner, true);
        Appearance.SetData(ent.Owner, BookPrinterVisuals.Printing, false);
        ent.Comp.PrintingBook = args.Book;
        ent.Comp.PrintEnd = _timing.CurTime + TimeSpan.FromSeconds(3);
        ent.Comp.IsPrinting = true;
    }

    private void OnPrinterPowerChanged(Entity<BookPrinterComponent> ent, ref PowerChangedEvent args)
    {
        if (ent.Comp.PrintingBook != null || args.Powered)
            return;

        _ambient.SetAmbience(ent.Owner, false);
        Appearance.SetData(ent.Owner, BookPrinterVisuals.Printing, false);
        ent.Comp.IsPrinting = false;
        ent.Comp.PrintingBook = null;
        ent.Comp.PrintEnd = TimeSpan.Zero;
    }

    private async void OnBookApprove(ApproveBookMessage args)
    {
        if (!_pendingBooks.TryGetValue(args.Book, out var book))
            return;

        SaveBookToDb(args.Book, book);
    }

    private void OnBookDecline(DeclineBookMessage args)
    {
        _pendingBooks.Remove(args.Book);
        var ev = new PopulatePendingBooksMenuMessage(_pendingBooks);
        RaiseNetworkEvent(ev, Filter.Empty().AddWhere(x => _adminManager.IsAdmin(x)));
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
            comp.IsScanning = false;
            comp.ScanEndTime = TimeSpan.Zero;
            comp.NextScan = _timing.CurTime + TimeSpan.FromMinutes(15);
            _ambient.SetAmbience(uid, false);
            Appearance.SetData(uid, BookScannerVisuals.Scanning, false);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg"), uid, AudioParams.Default.WithVolume(-4f));
            UpdateScannerUi((uid, comp));
        }

        var printerQuery = EntityQueryEnumerator<BookPrinterComponent>();
        while (printerQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.PrintingBook == null || !comp.IsPrinting)
                continue;

            if (comp.PrintEnd > _timing.CurTime)
                continue;

            comp.NextPrint = _timing.CurTime + TimeSpan.FromMinutes(2);
            comp.IsPrinting = false;

            _ambient.SetAmbience(uid, false);
            Appearance.SetData(uid, BookPrinterVisuals.Printing, false);

            var book = Spawn("TestCustomBook", Transform(uid).Coordinates);
            var bookComp = EnsureComp<CustomBookComponent>(book);

            bookComp.Author = comp.PrintingBook.Author;
            bookComp.Genre = comp.PrintingBook.Genre;
            bookComp.Title = comp.PrintingBook.Title;
            bookComp.Pages = new(comp.PrintingBook.Pages);
            bookComp.Binding = comp.PrintingBook.Binding;
            bookComp.Desc = comp.PrintingBook.Desc;

            RegenerateBook((book, bookComp));

            comp.PrintingBook = null;
        }
    }

    private void SaveBook(Entity<CustomBookComponent> ent)
    {
        if (ent.Comp.Binding == null)
            return;

        var data = new BookData(ent.Comp.Title, ent.Comp.Genre, ent.Comp.Author, ent.Comp.Desc, ent.Comp.Pages, ent.Comp.Binding);
        _pendingBooks.Add(_nextPendingBook, data);
        _nextPendingBook++;

        var ev = new PopulatePendingBooksMenuMessage(_pendingBooks);
        RaiseNetworkEvent(ev, Filter.Empty().AddWhere(x => _adminManager.IsAdmin(x)));

        _chat.SendAdminAnnouncement(Loc.GetString("pending-book-chat-notify", ("title", ent.Comp.Title)));
        _audio.PlayGlobal(new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg"), Filter.Empty().AddPlayers(_adminManager.ActiveAdmins), false, AudioParams.Default.WithVolume(-8f));
    }

    private void UpdateScannerUi(Entity<BookScannerComponent> ent)
    {
        if (!TryComp<CustomBookComponent>(ent.Comp.Book, out var comp))
            UI.SetUiState(ent.Owner, BookScannerUiKey.Key, new BookScannerUiState("", "", "", "", ent.Comp.IsScanning || ent.Comp.NextScan > _timing.CurTime));
        else
            UI.SetUiState(ent.Owner, BookScannerUiKey.Key, new BookScannerUiState(comp.Author, comp.Genre, comp.Title, comp.Desc, ent.Comp.IsScanning || ent.Comp.NextScan > _timing.CurTime));
    }

    public void OpenPendingBooks(ICommonSession session)
    {
        var ev = new OpenPendingBooksListMessage(_pendingBooks);
        RaiseNetworkEvent(ev, session);
    }

    public async void SaveBookToDb(int id, BookData data)
    {
        BookEntry entry = new()
        {
            Title = data.Title,
            Genre = data.Genre,
            Author = data.Author,
            Content = data.Pages,
            Description = data.Desc
        };

        entry.BindingMaps = new();
        entry.BindingPaths = new();
        entry.BindingStates = new();

        foreach (var item in data.Binding)
        {
            entry.BindingMaps.Add(item.Key);
            entry.BindingPaths.Add(item.Value.Path.CanonPath);
            entry.BindingStates.Add(item.Value.State);
        }

        await _db.UploadBookPrinterEntryAsync(entry);

        _pendingBooks.Remove(id);
        var ev = new PopulatePendingBooksMenuMessage(_pendingBooks);
        RaiseNetworkEvent(ev, Filter.Empty().AddWhere(x => _adminManager.IsAdmin(x)));

        foreach (var printer in EntityManager.AllEntities<BookPrinterComponent>())
        {
            UpdatePrinterUi(printer);
        }
    }

    private async void UpdatePrinterUi(Entity<BookPrinterComponent> ent)
    {
        var result = new Dictionary<int, BookData>();
        var entries = await _db.GetBookPrinterEntriesAsync();

        foreach (var entry in entries)
        {
            var data = new BookData(entry.Title, entry.Genre, entry.Author, entry.Description, entry.Content, new());
            for (var i = 0; i < entry.BindingMaps.Count; i++)
            {
                KeyValuePair<string, (ResPath Path, string State)> bindingLayer = new(entry.BindingMaps[i], (new(entry.BindingPaths[i]), entry.BindingStates[i]));
                data.Binding.Add(bindingLayer.Key, bindingLayer.Value);
            }

            result.Add(entry.Id, data);
        }

        UI.SetUiState(ent.Owner, BookPrinterUiKey.Key, new BookPrinterUiState(result, ent.Comp.AllowDeleting, ent.Comp.NextPrint > _timing.CurTime));
    }
}
