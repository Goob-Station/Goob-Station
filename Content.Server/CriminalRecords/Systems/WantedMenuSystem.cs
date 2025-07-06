// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.StationRecords;
using Content.Shared.Access.Components;
using Content.Shared.CriminalRecords;
using Content.Shared.IdentityManagement;
using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.CriminalRecords.Systems; // Goobstation-WantedMenu

public sealed partial class CriminalRecordsConsoleSystem
{
    private void UpdateUserInterface<T>(Entity<IdExaminableComponent> ent, ref T args)
    {
        UpdateUserInterface(ent);
    }

    public bool TryGetTargetRecord(EntityUid target, out KeyValuePair<uint,string>? targetRecord, out EntityUid? owningStation)
    {
        targetRecord = default;
        owningStation = _station.GetOwningStation(target);
        if (owningStation is not { } station)
            return false;
        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecords))
            return false;
        if (!TryComp(target, out MetaDataComponent? metaData))
            return false;
        var listing = _records.BuildListing((owningStation.Value, stationRecords), null);
        targetRecord = listing.FirstOrNull(r => r.Value == metaData.EntityName);
        if (targetRecord == null)
            return false;
        return true;
    }
    private void UpdateUserInterface(Entity<IdExaminableComponent> ent)
    {
        CriminalRecordsConsoleState? state;
        var ( uid, component ) = ent;
        if (!TryGetTargetRecord(uid, out var targetRecord, out var owningStation))
        {
            state = new CriminalRecordsConsoleState(null, null);
            _ui.SetUiState(uid, SetWantedVerbMenu.Key, state);
            return;
        }
        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecords))
            return;
        var listing = _records.BuildListing((owningStation.Value, stationRecords), null);
        state = new CriminalRecordsConsoleState(listing, null);
        if (targetRecord == null)
            return;
        var activeKey = targetRecord.Value.Key;
        var key = new StationRecordKey(activeKey, owningStation.Value);
        _records.TryGetRecord(key, out state.StationRecord, stationRecords);
        _records.TryGetRecord(key, out state.CriminalRecord, stationRecords);
        state.SelectedKey = activeKey;

        _ui.SetUiState(uid, SetWantedVerbMenu.Key, state);
    }
    private bool CheckSelected(Entity<IdExaminableComponent> ent, EntityUid user,
        [NotNullWhen(true)] out EntityUid? mob, [NotNullWhen(true)] out StationRecordKey? key)
    {
        key = null;
        mob = null;

        if (!_access.IsAllowed(user, ent))
        {
            _popup.PopupEntity(Loc.GetString("criminal-records-permission-denied"), ent, user);
            return false;
        }
        TryGetTargetRecord(ent, out var targetRecord, out var owningStation);
        if (owningStation is not { } station)
            return false;
        if (targetRecord == null)
            return false;
        var activeKey = targetRecord.Value.Key;
        key = new StationRecordKey(activeKey, owningStation.Value);
        mob = user;
        return true;
    }
    private void OnChangeStatus(Entity<IdExaminableComponent> ent, ref CriminalRecordChangeStatus msg)
    {
        // prevent malf client violating wanted/reason nullability
        var requireReason = msg.Status is SecurityStatus.Wanted
            or SecurityStatus.Suspected
            or SecurityStatus.Search
            or SecurityStatus.Dangerous;

        if (requireReason != (msg.Reason != null))
            return;

        if (!CheckSelected(ent, msg.Actor, out var mob, out var key))
            return;

        if (!_records.TryGetRecord<CriminalRecord>(key.Value, out var record) || record.Status == msg.Status)
            return;

        string? reason = null;
        if (msg.Reason != null)
        {
            reason = msg.Reason.Trim();
            if (reason.Length < 1 || reason.Length > ent.Comp.MaxStringLength)
                return;
        }

        var oldStatus = record.Status;

        var name = _records.RecordName(key.Value);
        GetOfficer(msg.Actor, out var officer);

        // will probably never fail given the checks above
        name = _records.RecordName(key.Value);
        officer = Loc.GetString("criminal-records-console-unknown-officer");
        var jobName = "Unknown";

        _records.TryGetRecord<GeneralStationRecord>(key.Value, out var entry);
        if (entry != null)
            jobName = entry.JobTitle;

        var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(null, msg.Actor);
        RaiseLocalEvent(tryGetIdentityShortInfoEvent);
        if (tryGetIdentityShortInfoEvent.Title != null)
            officer = tryGetIdentityShortInfoEvent.Title;

        _criminalRecords.TryChangeStatus(key.Value, msg.Status, msg.Reason, officer);

        (string, object)[] args;
        if (reason != null)
            args = new (string, object)[] { ("name", name), ("officer", officer), ("reason", reason), ("job", jobName) };
        else
            args = new (string, object)[] { ("name", name), ("officer", officer), ("job", jobName) };

        // figure out which radio message to send depending on transition
        var statusString = (oldStatus, msg.Status) switch
        {
            // person has been detained
            (_, SecurityStatus.Detained) => "detained",
            // person did something sus
            (_, SecurityStatus.Suspected) => "suspected",
            // released on parole
            (_, SecurityStatus.Paroled) => "paroled",
            // prisoner did their time
            (_, SecurityStatus.Discharged) => "released",
            // going from any other state to wanted, AOS or prisonbreak / lazy secoff never set them to released and they reoffended
            (_, SecurityStatus.Wanted) => "wanted",
            // person has been sentenced to perma
            (_, SecurityStatus.Perma) => "perma",
            // person needs to be searched
            (_, SecurityStatus.Search) => "search",
            // person is very dangerous
            (_, SecurityStatus.Dangerous) => "dangerous",
            // person is no longer sus
            (SecurityStatus.Suspected, SecurityStatus.None) => "not-suspected",
            // going from wanted to none, must have been a mistake
            (SecurityStatus.Wanted, SecurityStatus.None) => "not-wanted",
            // criminal status removed
            (SecurityStatus.Detained, SecurityStatus.None) => "released",
            // criminal is no longer on parole
            (SecurityStatus.Paroled, SecurityStatus.None) => "not-parole",
            // criminal is no longer in perma
            (SecurityStatus.Perma, SecurityStatus.None) => "not-perma",
            // person no longer needs to be searched
            (SecurityStatus.Search, SecurityStatus.None) => "not-search",
            // person is no longer dangerous
            (SecurityStatus.Dangerous, SecurityStatus.None) => "not-dangerous",
            // this is impossible
            _ => "not-wanted"
        };
        _radio.SendRadioMessage(msg.Actor, Loc.GetString($"criminal-records-console-{statusString}", args),
            ent.Comp.SecurityChannel, ent);

        UpdateUserInterface(ent);
    }
}
