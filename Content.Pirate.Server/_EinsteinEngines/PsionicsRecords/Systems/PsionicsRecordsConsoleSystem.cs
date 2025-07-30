using Content.Server.Popups;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.PsionicsRecords;
using Content.Shared.PsionicsRecords.Components;
using Content.Shared.PsionicsRecords.Systems;
using Content.Shared.Psionics;
using Content.Shared.StationRecords;
using Robust.Server.GameObjects;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.IdentityManagement;
using Content.Shared.Psionics.Components;

/// <summary>
/// EVERYTHING HERE IS A MODIFIED VERSION OF CRIMINAL RECORDS
/// </summary>

namespace Content.Server.PsionicsRecords.Systems;

/// <summary>
/// Handles all UI for Psionics records console
/// </summary>
public sealed class PsionicsRecordsConsoleSystem : SharedPsionicsRecordsConsoleSystem
{
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly PsionicsRecordsSystem _psionicsRecords = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PsionicsRecordsConsoleComponent, RecordModifiedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<PsionicsRecordsConsoleComponent, AfterGeneralRecordCreatedEvent>(UpdateUserInterface);

        Subs.BuiEvents<PsionicsRecordsConsoleComponent>(PsionicsRecordsConsoleKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(UpdateUserInterface);
            subs.Event<SelectStationRecord>(OnKeySelected);
            subs.Event<SetStationRecordFilter>(OnFiltersChanged);
            subs.Event<PsionicsRecordChangeStatus>(OnChangeStatus);
        });
    }

    private void UpdateUserInterface<T>(Entity<PsionicsRecordsConsoleComponent> ent, ref T args)
    {
        // TODO: this is probably wasteful, maybe better to send a message to modify the exact state?
        UpdateUserInterface(ent);
    }

    private void OnKeySelected(Entity<PsionicsRecordsConsoleComponent> ent, ref SelectStationRecord msg)
    {
        // no concern of sus client since record retrieval will fail if invalid id is given
        ent.Comp.ActiveKey = msg.SelectedKey;
        UpdateUserInterface(ent);
    }

    private void OnFiltersChanged(Entity<PsionicsRecordsConsoleComponent> ent, ref SetStationRecordFilter msg)
    {
        if (ent.Comp.Filter == null ||
            ent.Comp.Filter.Type != msg.Type || ent.Comp.Filter.Value != msg.Value)
        {
            ent.Comp.Filter = new StationRecordsFilter(msg.Type, msg.Value);
            UpdateUserInterface(ent);
        }
    }

    private void OnChangeStatus(Entity<PsionicsRecordsConsoleComponent> ent, ref PsionicsRecordChangeStatus msg)
    {
        // prevent malf client violating registered/reason nullability
        if (msg.Status == PsionicsStatus.Registered != (msg.Reason != null) &&
            msg.Status == PsionicsStatus.Suspected != (msg.Reason != null) &&
            msg.Status == PsionicsStatus.Abusing != (msg.Reason != null))
            return;

        if (!CheckSelected(ent, msg.Actor, out var mob, out var key))
            return;

        if (!_stationRecords.TryGetRecord<PsionicsRecord>(key.Value, out var record) || record.Status == msg.Status)
            return;

        // validate the reason
        string? reason = null;
        if (msg.Reason != null)
        {
            reason = msg.Reason.Trim();
            if (reason.Length < 1 || reason.Length > ent.Comp.MaxStringLength)
                return;
        }

        var oldStatus = record.Status;

        // will probably never fail given the checks above
        _psionicsRecords.TryChangeStatus(key.Value, msg.Status, msg.Reason);

        var name = RecordName(key.Value);
        var officer = Loc.GetString("psionics-records-console-unknown-officer");

        var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(null, mob.Value);
        RaiseLocalEvent(tryGetIdentityShortInfoEvent);
        if (tryGetIdentityShortInfoEvent.Title != null)
        {
            officer = tryGetIdentityShortInfoEvent.Title;
        }

        (string, object)[] args;
        if (reason != null)
            args = new (string, object)[] { ("name", name), ("officer", officer), ("reason", reason) };
        else
            args = new (string, object)[] { ("name", name), ("officer", officer) };

        // figure out which radio message to send depending on transition
        var statusString = (oldStatus, msg.Status) switch
        {
            // person has been registered
            (_, PsionicsStatus.Registered) => "registered",
            // person did something suspicious
            (_, PsionicsStatus.Suspected) => "suspected",
            // person is abusing
            (_, PsionicsStatus.Abusing) => "abusing",
            // person is no longer suspicious
            (PsionicsStatus.Suspected, PsionicsStatus.None) => "not-suspected",
            // person is no longer registered
            (PsionicsStatus.Registered, PsionicsStatus.None) => "not-registered",
            // person is no longer abusing
            (PsionicsStatus.Abusing, PsionicsStatus.None) => "not-abusing",
            // this is impossible
            _ => "not-wanted"
        };
        _radio.SendRadioMessage(ent, Loc.GetString($"psionics-records-console-{statusString}", args),
            ent.Comp.RadioChannel, ent);

        UpdateUserInterface(ent);
        UpdatePsionicsIdentity(name, msg.Status);
    }

    private void UpdateUserInterface(Entity<PsionicsRecordsConsoleComponent> ent)
    {
        var (uid, console) = ent;
        var owningStation = _station.GetOwningStation(uid);

        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecords))
        {
            _ui.SetUiState(uid, PsionicsRecordsConsoleKey.Key, new PsionicsRecordsConsoleState());
            return;
        }

        var listing = _stationRecords.BuildListing((owningStation.Value, stationRecords), console.Filter);

        var state = new PsionicsRecordsConsoleState(listing, console.Filter);
        if (console.ActiveKey is { } id)
        {
            // get records to display when a crewmember is selected
            var key = new StationRecordKey(id, owningStation.Value);
            _stationRecords.TryGetRecord(key, out state.StationRecord, stationRecords);
            _stationRecords.TryGetRecord(key, out state.PsionicsRecord, stationRecords);
            state.SelectedKey = id;
        }

        _ui.SetUiState(uid, PsionicsRecordsConsoleKey.Key, state);
    }

    /// <summary>
    /// Boilerplate that most actions use, if they require that a record be selected.
    /// Obviously shouldn't be used for selecting records.
    /// </summary>
    private bool CheckSelected(Entity<PsionicsRecordsConsoleComponent> ent, EntityUid user,
        [NotNullWhen(true)] out EntityUid? mob, [NotNullWhen(true)] out StationRecordKey? key)
    {
        key = null;
        mob = null;

        if (!_access.IsAllowed(user, ent))
        {
            _popup.PopupEntity(Loc.GetString("psionics-records-permission-denied"), ent, user);
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
    /// Gets the name from a record, or empty string if this somehow fails.
    /// </summary>
    private string RecordName(StationRecordKey key)
    {
        if (!_stationRecords.TryGetRecord<GeneralStationRecord>(key, out var record))
            return "";

        return record.Name;
    }

    /// <summary>
    /// Checks if the new identity's name has a psionics record attached to it, and gives the entity the icon that
    /// belongs to the status if it does.
    /// </summary>
    public void CheckNewIdentity(EntityUid uid)
    {
        var name = Identity.Name(uid, EntityManager);
        var xform = Transform(uid);

        // TODO use the entity's station? Not the station of the map that it happens to currently be on?
        var station = _station.GetStationInMap(xform.MapID);

        if (station != null && _stationRecords.GetRecordByName(station.Value, name) is { } id)
        {
            if (_stationRecords.TryGetRecord<PsionicsRecord>(new StationRecordKey(id, station.Value),
                    out var record))
            {
                if (record.Status != PsionicsStatus.None)
                {
                    SetPsionicsIcon(name, record.Status, uid);
                    return;
                }
            }
        }
        RemComp<PsionicsRecordComponent>(uid);
    }
}
