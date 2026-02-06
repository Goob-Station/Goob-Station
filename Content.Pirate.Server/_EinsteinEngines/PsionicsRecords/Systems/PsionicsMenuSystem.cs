/// <summary>
/// Pirate psionics records menu logic, adapted from the Goob wanted menu.
/// </summary>

using Content.Server.StationRecords;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.PsionicsRecords;
using Content.Shared.IdentityManagement;
using Content.Shared.Psionics;
using Content.Shared.StationRecords;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.PsionicsRecords.Systems;

public sealed partial class PsionicsRecordsConsoleSystem
{
	    private void UpdateUserInterface(Entity<IdExaminableComponent> ent, ref PsionicsMenuRequestState args)
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
        var listing = _stationRecords.BuildListing((owningStation.Value, stationRecords), null);
        targetRecord = listing.FirstOrNull(r => r.Value == metaData.EntityName);
        if (targetRecord == null)
            return false;
        return true;
    }
    private void UpdateUserInterface(Entity<IdExaminableComponent> ent)
    {
        PsionicsRecordsConsoleState? state;
        var ( uid, component ) = ent;
        if (!TryGetTargetRecord(uid, out var targetRecord, out var owningStation))
        {
            state = new PsionicsRecordsConsoleState(null, null);
            _ui.SetUiState(uid, SetPsionicsVerbMenu.Key, state);
            return;
        }
        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecords))
            return;
        var listing = _stationRecords.BuildListing((owningStation.Value, stationRecords), null);
        state = new PsionicsRecordsConsoleState(listing, null);
        if (targetRecord == null)
            return;
        var activeKey = targetRecord.Value.Key;
        var key = new StationRecordKey(activeKey, owningStation.Value);
        _stationRecords.TryGetRecord(key, out state.StationRecord, stationRecords);
        _stationRecords.TryGetRecord(key, out state.PsionicsRecord, stationRecords);
        state.SelectedKey = activeKey;

        _ui.SetUiState(uid, SetPsionicsVerbMenu.Key, state);
    }
    private bool CheckSelected(Entity<IdExaminableComponent> ent, EntityUid user,
        [NotNullWhen(true)] out EntityUid? mob, [NotNullWhen(true)] out StationRecordKey? key)
    {
        key = null;
        mob = null;

        // Check if user has Mantis access
        var userAccessTags = _access.FindAccessTags(user);
        var mantisAccess = new ProtoId<AccessLevelPrototype>("Mantis");
        if (!userAccessTags.Contains(mantisAccess))
        {
            _popup.PopupEntity(Loc.GetString("psionics-records-permission-denied"), ent, user);
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
    private void OnChangeStatus(Entity<IdExaminableComponent> ent, ref PsionicsRecordChangeStatus msg)
    {
        // prevent malf client violating registered/reason nullability
        var requireReason = msg.Status is PsionicsStatus.Suspected
            or PsionicsStatus.Registered
            or PsionicsStatus.Abusing;

        if (requireReason != (msg.Reason != null))
            return;

        if (!CheckSelected(ent, msg.Actor, out var mob, out var key))
            return;

        if (!_stationRecords.TryGetRecord<PsionicsRecord>(key.Value, out var record) || record.Status == msg.Status)
            return;

        // Get max string length from IdExaminablePsionicsComponent or use default
        var maxStringLength = 256u;
        if (TryComp<IdExaminablePsionicsComponent>(ent, out var psionicsComp))
            maxStringLength = psionicsComp.MaxStringLength;

        string? reason = null;
        if (msg.Reason != null)
        {
            reason = msg.Reason.Trim();
            if (reason.Length < 1 || reason.Length > maxStringLength)
                return;
        }

        var oldStatus = record.Status;

        var name = _stationRecords.RecordName(key.Value);
        GetOfficer(msg.Actor, out var officer);

        // will probably never fail given the checks above
        name = _stationRecords.RecordName(key.Value);
        officer = Loc.GetString("psionics-records-console-unknown-officer");
        var jobName = "Unknown";

        _stationRecords.TryGetRecord<GeneralStationRecord>(key.Value, out var entry);
        if (entry != null)
            jobName = entry.JobTitle;

        var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(null, msg.Actor);
        RaiseLocalEvent(tryGetIdentityShortInfoEvent);
        if (tryGetIdentityShortInfoEvent.Title != null)
            officer = tryGetIdentityShortInfoEvent.Title;

        _psionicsRecords.TryChangeStatus(key.Value, msg.Status, msg.Reason);

        (string, object)[] args;
        if (reason != null)
            args = new (string, object)[] { ("name", name), ("officer", officer), ("reason", reason), ("job", jobName) };
        else
            args = new (string, object)[] { ("name", name), ("officer", officer), ("job", jobName) };

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
        // Get radio channel from IdExaminablePsionicsComponent or use default
        var radioChannel = "Science";
        if (TryComp<IdExaminablePsionicsComponent>(ent, out var psionicsRadioComp))
            radioChannel = psionicsRadioComp.RadioChannel;

        _radio.SendRadioMessage(msg.Actor, Loc.GetString($"psionics-records-console-{statusString}", args),
            radioChannel, ent);

        UpdateUserInterface(ent);
        UpdatePsionicsIdentity(name, msg.Status);
    }

	    private void GetOfficer(EntityUid uid, out string officer)
	    {
	        var tryGetIdentityShortInfoEvent = new TryGetIdentityShortInfoEvent(null, uid);
	        RaiseLocalEvent(tryGetIdentityShortInfoEvent);
	        officer = tryGetIdentityShortInfoEvent.Title ?? Loc.GetString("psionics-records-console-unknown-officer");
	    }
}

