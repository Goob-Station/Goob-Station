using Content.Goobstation.Shared.Books;
using Content.Shared.Power;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Books;

public sealed partial class CustomBooksSystem
{
    private void InitializePrinter()
    {
        SubscribeLocalEvent<BookPrinterComponent, MapInitEvent>(OnPrinterInit);
        SubscribeLocalEvent<BookPrinterComponent, PrintBookMessage>(OnPrinterPrint);
        SubscribeLocalEvent<BookPrinterComponent, PowerChangedEvent>(OnPrinterPowerChanged);
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

    private async void UpdatePrinterUi(Entity<BookPrinterComponent> ent)
    {
        var result = new Dictionary<int, BookData>();

        // We need to get database data so using async methods
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

    private void UpdatePrinter(float frameTime)
    {
        var query = EntityQueryEnumerator<BookPrinterComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.PrintingBook == null || !comp.IsPrinting)
                continue;

            if (comp.PrintEnd > _timing.CurTime)
                continue;

            comp.NextPrint = _timing.CurTime + TimeSpan.FromMinutes(2);
            comp.IsPrinting = false;

            _ambient.SetAmbience(uid, false);
            Appearance.SetData(uid, BookPrinterVisuals.Printing, false);

            var book = Spawn("CustomBookTemplate", Transform(uid).Coordinates);
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
}
