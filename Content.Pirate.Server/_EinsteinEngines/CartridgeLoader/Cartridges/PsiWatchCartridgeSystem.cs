using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.PsionicsRecords;
using Content.Shared.StationRecords;

/// <summary>
/// ADAPTED FROM SECWATCH - DELTAV
/// </summary>

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed class PsiWatchCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RecordModifiedEvent>(OnRecordModified);

        SubscribeLocalEvent<PsiWatchCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
    }

    private void OnRecordModified(RecordModifiedEvent args)
    {
        // when a record is modified update the ui of every loaded cartridge tuned to the same station
        var query = EntityQueryEnumerator<PsiWatchCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (cartridge.LoaderUid is not {} loader || comp.Station != args.Station)
                continue;

            UpdateUI((uid, comp), loader);
        }
    }

    private void OnUiReady(Entity<PsiWatchCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUI(ent, args.Loader);
    }

    private void UpdateUI(Entity<PsiWatchCartridgeComponent> ent, EntityUid loader)
    {
        // if the loader is on a grid, update the station
        // if it is off grid use the cached station
        if (_station.GetOwningStation(loader) is {} station)
            ent.Comp.Station = station;

        if (!TryComp<StationRecordsComponent>(ent.Comp.Station, out var records))
            return;

        station = ent.Comp.Station.Value;

        var entries = new List<PsiWatchEntry>();
        foreach (var (id, criminal) in _records.GetRecordsOfType<PsionicsRecord>(station, records))
        {
            if (!ent.Comp.Statuses.Contains(criminal.Status))
                continue;

            var key = new StationRecordKey(id, station);
            if (!_records.TryGetRecord<GeneralStationRecord>(key, out var general, records))
                continue;

            var status = criminal.Status;
            entries.Add(new PsiWatchEntry(general.Name, general.JobTitle, criminal.Status, criminal.Reason));
        }

        var state = new PsiWatchUiState(entries);
        _cartridgeLoader.UpdateCartridgeUiState(loader, state);
    }
}
