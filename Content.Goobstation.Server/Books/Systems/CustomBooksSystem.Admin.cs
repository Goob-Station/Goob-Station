using Content.Goobstation.Shared.Books;
using Content.Server.Database;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Books;

public sealed partial class CustomBooksSystem
{
    private void InitializeAdmin()
    {
        SubscribeNetworkEvent<ApproveBookMessage>(OnBookApprove);
        SubscribeNetworkEvent<DeclineBookMessage>(OnBookDecline);

        // its here because deleting books is for admins only
        SubscribeLocalEvent<BookPrinterComponent, DeleteBookMessage>(OnBookDelete);
    }

    private void OnBookApprove(ApproveBookMessage args)
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

    private void OnBookDelete(Entity<BookPrinterComponent> ent, ref DeleteBookMessage args)
    {
        if (!_adminManager.IsAdmin(args.Actor))
            return;

        RemoveBookFromDb(args.Id);
    }

    /// <summary>
    /// Saves a book in database. Called on admin approve
    /// </summary>
    /// <param name="id">Pending book index</param>
    /// <param name="data">The book itself</param>
    private async void SaveBookToDb(int id, BookData data)
    {
        // Build new entry
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

        // Save to db
        await _db.UploadBookPrinterEntryAsync(entry);

        _pendingBooks.Remove(id);
        var ev = new PopulatePendingBooksMenuMessage(_pendingBooks);
        RaiseNetworkEvent(ev, Filter.Empty().AddWhere(x => _adminManager.IsAdmin(x)));

        // Update printers
        foreach (var printer in EntityManager.AllEntities<BookPrinterComponent>())
        {
            UpdatePrinterUi(printer);
        }
    }

    private async void RemoveBookFromDb(int id)
    {
        await _db.DeleteBookPrinterEntryAsync(id);

        foreach (var printer in EntityManager.AllEntities<BookPrinterComponent>())
        {
            UpdatePrinterUi(printer);
        }
    }

    /// <summary>
    /// Adds a book in the pending list
    /// </summary>
    /// <param name="ent">Book entity</param>
    private void AddPendingBook(Entity<CustomBookComponent> ent)
    {
        if (ent.Comp.Binding == null)
            return;

        // Build book
        var data = new BookData(ent.Comp.Title, ent.Comp.Genre, ent.Comp.Author, ent.Comp.Desc, ent.Comp.Pages, ent.Comp.Binding);
        _pendingBooks.Add(_nextPendingBook, data);
        _nextPendingBook++;

        // Update ui
        var ev = new PopulatePendingBooksMenuMessage(_pendingBooks);
        RaiseNetworkEvent(ev, Filter.Empty().AddWhere(x => _adminManager.IsAdmin(x)));

        // Ping admeme
        _chat.SendAdminAnnouncement(Loc.GetString("pending-book-chat-notify", ("title", ent.Comp.Title)));
        _audio.PlayGlobal(new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg"), Filter.Empty().AddPlayers(_adminManager.ActiveAdmins), false, AudioParams.Default.WithVolume(-8f));
    }

    public void OpenPendingBooks(ICommonSession session)
    {
        var ev = new OpenPendingBooksListMessage(_pendingBooks);
        RaiseNetworkEvent(ev, session);
    }
}
