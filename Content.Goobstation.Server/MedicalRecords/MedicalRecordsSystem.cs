using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.CriminalRecords;
using Content.Shared.IdentityManagement;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Goobstation.Shared.MedicalRecords;

public sealed class MedicalRecordsSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AfterGeneralRecordCreatedEvent>(OnGeneralRecordCreated);

        SubscribeLocalEvent<MedicalRecordsConsoleComponent, RecordModifiedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<MedicalRecordsConsoleComponent, AfterGeneralRecordCreatedEvent>(UpdateUserInterface);

        Subs.BuiEvents<MedicalRecordsConsoleComponent>(MedicalRecordsConsoleKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(UpdateUserInterface);
            subs.Event<SelectStationRecord>(OnKeySelected);
            subs.Event<SetStationRecordFilter>(OnFiltersChanged);
            subs.Event<MedicalRecordAddHistory>(OnAddHistory);
            subs.Event<MedicalRecordRemoveHistory>(OnDeleteHistory);
        });
    }

    private void OnGeneralRecordCreated(AfterGeneralRecordCreatedEvent ev)
    {
        var record = new MedicalRecord();
        RandomizeHistory(record, ev.Profile);

        _records.AddRecordEntry(ev.Key, new MedicalRecord());
        _records.Synchronize(ev.Key);
    }

    private void UpdateUserInterface<T>(Entity<MedicalRecordsConsoleComponent> ent, ref T args)
    {
        // TODO: this is probably wasteful, maybe better to send a message to modify the exact state?
        UpdateUserInterface(ent);
    }

    private void OnKeySelected(Entity<MedicalRecordsConsoleComponent> ent, ref SelectStationRecord msg)
    {
        // no concern of sus client since record retrieval will fail if invalid id is given
        ent.Comp.ActiveKey = msg.SelectedKey;
        UpdateUserInterface(ent);
    }
    private void OnFiltersChanged(Entity<MedicalRecordsConsoleComponent> ent, ref SetStationRecordFilter msg)
    {
        if (ent.Comp.Filter == null ||
            ent.Comp.Filter.Type != msg.Type || ent.Comp.Filter.Value != msg.Value)
        {
            ent.Comp.Filter = new StationRecordsFilter(msg.Type, msg.Value);
            UpdateUserInterface(ent);
        }
    }

    private void GetOfficer(EntityUid uid, out string officer)
    {
        var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(null, uid);
        RaiseLocalEvent(tryGetIdentityShortInfoEvent);
        officer = tryGetIdentityShortInfoEvent.Title ?? Loc.GetString("criminal-records-console-unknown-officer");
    }

    private void OnAddHistory(Entity<MedicalRecordsConsoleComponent> ent, ref MedicalRecordAddHistory msg)
    {
        if (!CheckSelected(ent, msg.Actor, out var mob, out var key))
            return;

        GetOfficer(mob.Value, out var user);

        if (!TryAddHistory(key.Value, msg.Line, user))
            return;

        UpdateUserInterface(ent);
    }

    private void OnDeleteHistory(Entity<MedicalRecordsConsoleComponent> ent, ref MedicalRecordRemoveHistory msg)
    {
        if (!CheckSelected(ent, msg.Actor, out _, out var key))
            return;

        if (!TryDeleteHistory(key.Value, msg.Index))
            return;

        UpdateUserInterface(ent);
    }

    private void UpdateUserInterface(Entity<MedicalRecordsConsoleComponent> ent)
    {
        var (uid, console) = ent;
        var owningStation = _station.GetOwningStation(uid);

        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecords))
        {
            _ui.SetUiState(uid, MedicalRecordsConsoleKey.Key, new MedicalRecordsConsoleUiState());
            return;
        }

        // get the listing of records to display
        var listing = _records.BuildListing((owningStation.Value, stationRecords), console.Filter);

        var state = new MedicalRecordsConsoleUiState(listing, console.Filter);
        if (console.ActiveKey is { } id)
        {
            // get records to display when a crewmember is selected
            var key = new StationRecordKey(id, owningStation.Value);
            _records.TryGetRecord(key, out state.StationRecord, stationRecords);
            _records.TryGetRecord(key, out state.MedicalRecord, stationRecords);
            state.SelectedKey = id;
        }

        _ui.SetUiState(uid, MedicalRecordsConsoleKey.Key, state);
    }

    /// <summary>
    /// Boilerplate that most actions use, if they require that a record be selected.
    /// Obviously shouldn't be used for selecting records.
    /// </summary>
    private bool CheckSelected(Entity<MedicalRecordsConsoleComponent> ent, EntityUid user,
        [NotNullWhen(true)] out EntityUid? mob, [NotNullWhen(true)] out StationRecordKey? key)
    {
        key = null;
        mob = null;
        if (!_access.IsAllowed(user, ent))
        {
            _popup.PopupEntity(Loc.GetString("criminal-records-permission-denied"), ent, user);
            return false;
        }

        if (ent.Comp.ActiveKey is not { } id)
            return false;

        // checking the console's station since the user might be off-grid using on-grid console
        if (_station.GetOwningStation(ent) is not { } station)
            return false;

        key = new StationRecordKey(id, station);
        mob = user;
        return true;
    }

    /// <summary>
    /// Tries to add a history entry to a criminal record.
    /// </summary>
    /// <returns>True if adding succeeded, false if not</returns>
    public bool TryAddHistory(StationRecordKey key, MedicalHistory entry)
    {
        if (!_records.TryGetRecord<MedicalRecord>(key, out var record))
            return false;

        record.History.Add(entry);

        return true;
    }

    /// <summary>
    /// Creates and tries to add a history entry using the current time.
    /// </summary>
    public bool TryAddHistory(StationRecordKey key, string line, string? initiatorName = null)
    {
        var entry = new MedicalHistory(_ticker.RoundDuration(), line, initiatorName);
        return TryAddHistory(key, entry);
    }

    public bool TryDeleteHistory(StationRecordKey key, uint index)
    {
        if (!_records.TryGetRecord<MedicalRecord>(key, out var record))
            return false;

        if (index >= record.History.Count)
            return false;

        record.History.RemoveAt((int) index);

        return true;
    }

    private void RandomizeHistory(MedicalRecord record, HumanoidCharacterProfile profile)
    {
        List<string> records = new();

        foreach (var item in _proto.EnumeratePrototypes<AntagPrototype>().Where(x => x.SetPreference && x.MedicalRecord != null))
        {
            if (records.Contains(item.MedicalRecord!))
                continue;

            if (_random.Prob(profile.AntagPreferences.Contains(item.ID) ? item.RecordsProb : item.FakeRecordsProb))
                records.Add(item.MedicalRecord!);
        }

        foreach (var item in records)
        {
            record.History.Add(new MedicalHistory(null, item, null));
        }
    }
}
