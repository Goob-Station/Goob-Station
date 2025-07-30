using System.Diagnostics.CodeAnalysis;
using Content.Server.StationRecords.Systems;
using Content.Shared.PsionicsRecords;
using Content.Shared.Psionics;
using Content.Shared.StationRecords;
using Content.Server.GameTicking;

/// <summary>
/// EVERYTHING HERE IS A MODIFIED VERSION OF CRIMINAL RECORDS
/// </summary>

namespace Content.Server.PsionicsRecords.Systems;

/// <summary>
///     Psionics records
///
///     Psionics Records inherit Station Records' core and add role-playing tools for Epistemics:
///         - Ability to track a person's status (None/Suspected/Registered/Abusing)
///         - See cataloguers' actions in Psionics Records in the radio
///         - See reasons for any action with no need to ask the officer personally
/// </summary>
public sealed class PsionicsRecordsSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AfterGeneralRecordCreatedEvent>(OnGeneralRecordCreated);
    }

    private void OnGeneralRecordCreated(AfterGeneralRecordCreatedEvent ev)
    {
        _stationRecords.AddRecordEntry(ev.Key, new PsionicsRecord());
        _stationRecords.Synchronize(ev.Key);
    }

    /// <summary>
    /// Tries to change the status of the record found by the StationRecordKey.
    /// Reason should only be passed if status is not None.
    /// </summary>
    /// <returns>True if the status is changed, false if not</returns>
    public bool TryChangeStatus(StationRecordKey key, PsionicsStatus status, string? reason)
    {
        // don't do anything if its the same status
        if (!_stationRecords.TryGetRecord<PsionicsRecord>(key, out var record)
            || status == record.Status)
            return false;

        record.Status = status;
        record.Reason = reason;

        _stationRecords.Synchronize(key);

        return true;
    }
}
