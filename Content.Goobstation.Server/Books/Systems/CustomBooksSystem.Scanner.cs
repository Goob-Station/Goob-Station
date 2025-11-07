using Content.Goobstation.Shared.Books;
using Content.Shared.Placeable;
using Content.Shared.Power;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Books;

public sealed partial class CustomBooksSystem
{
    private void InitializeScanner()
    {
        SubscribeLocalEvent<BookScannerComponent, ItemPlacedEvent>(OnBookPlaced);
        SubscribeLocalEvent<BookScannerComponent, ItemRemovedEvent>(OnBookRemoved);
        SubscribeLocalEvent<BookScannerComponent, StartBookScanMessage>(OnStartScan);
        SubscribeLocalEvent<BookScannerComponent, PowerChangedEvent>(OnScannerPowerChanged);
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

    private void UpdateScannerUi(Entity<BookScannerComponent> ent)
    {
        if (!TryComp<CustomBookComponent>(ent.Comp.Book, out var comp))
            UI.SetUiState(ent.Owner, BookScannerUiKey.Key, new BookScannerUiState("", "", "", "", ent.Comp.IsScanning || ent.Comp.NextScan > _timing.CurTime));
        else
            UI.SetUiState(ent.Owner, BookScannerUiKey.Key, new BookScannerUiState(comp.Author, comp.Genre, comp.Title, comp.Desc, ent.Comp.IsScanning || ent.Comp.NextScan > _timing.CurTime));
    }

    private void UpdateScanner(float frameTime)
    {
        var query = EntityQueryEnumerator<BookScannerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsScanning || !TryComp<CustomBookComponent>(comp.Book, out var bookComp))
                continue;

            if (comp.ScanEndTime > _timing.CurTime)
                continue;

            AddPendingBook((comp.Book.Value, bookComp));
            comp.IsScanning = false;
            comp.ScanEndTime = TimeSpan.Zero;
            comp.NextScan = _timing.CurTime + TimeSpan.FromMinutes(15);
            _ambient.SetAmbience(uid, false);
            Appearance.SetData(uid, BookScannerVisuals.Scanning, false);
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg"), uid, AudioParams.Default.WithVolume(-4f));
            UpdateScannerUi((uid, comp));
        }
    }
}
