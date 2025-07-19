using Content.Shared.Psionics;
using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

/// <summary>
/// EVERYTHING HERE IS A MODIFIED VERSION OF CRIMINAL RECORDS
/// </summary>

namespace Content.Shared.PsionicsRecords;

[Serializable, NetSerializable]
public enum PsionicsRecordsConsoleKey : byte
{
    Key
}

/// <summary>
///     Psionics records console state. There are a few states:
///     - SelectedKey null, Record null, RecordListing null
///         - The station record database could not be accessed.
///     - SelectedKey null, Record null, RecordListing non-null
///         - Records are populated in the database, or at least the station has
///           the correct component.
///     - SelectedKey non-null, Record null, RecordListing non-null
///         - The selected key does not have a record tied to it.
///     - SelectedKey non-null, Record non-null, RecordListing non-null
///         - The selected key has a record tied to it, and the record has been sent.
///
///     - there is added new filters and so added new states
///         -SelectedKey null, Record null, RecordListing null, filters non-null
///            the station may have data, but they all did not pass through the filters
///
///     Other states are erroneous.
/// </summary>
[Serializable, NetSerializable]
public sealed class PsionicsRecordsConsoleState : BoundUserInterfaceState
{
    /// <summary>
    /// Currently selected crewmember record key.
    /// </summary>
    public uint? SelectedKey = null;

    public PsionicsRecord? PsionicsRecord = null;
    public GeneralStationRecord? StationRecord = null;
    public readonly Dictionary<uint, string>? RecordListing;
    public readonly StationRecordsFilter? Filter;

    public PsionicsRecordsConsoleState(Dictionary<uint, string>? recordListing, StationRecordsFilter? newFilter)
    {
        RecordListing = recordListing;
        Filter = newFilter;
    }

    /// <summary>
    /// Default state for opening the console
    /// </summary>
    public PsionicsRecordsConsoleState() : this(null, null)
    {
    }

    public bool IsEmpty() => SelectedKey == null && StationRecord == null && PsionicsRecord == null && RecordListing == null;
}

/// <summary>
/// Used to change status, respecting the psionics nullability rules in <see cref="PsionicsRecord"/>.
/// </summary>
[Serializable, NetSerializable]
public sealed class PsionicsRecordChangeStatus : BoundUserInterfaceMessage
{
    public readonly PsionicsStatus Status;
    public readonly string? Reason;

    public PsionicsRecordChangeStatus(PsionicsStatus status, string? reason)
    {
        Status = status;
        Reason = reason;
    }
}
