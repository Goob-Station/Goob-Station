// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Shared.Serialization;
using Robust.Shared.Enums; // Pirate: records photos

namespace Content.Shared.CriminalRecords;

[Serializable, NetSerializable]
public enum CriminalRecordsConsoleKey : byte
{
    Key
}

/// <summary>
///     Criminal records console state. There are a few states:
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
public sealed class CriminalRecordsConsoleState : BoundUserInterfaceState
{
    /// <summary>
    /// Currently selected crewmember record key.
    /// </summary>
    public uint? SelectedKey = null;
    public CriminalRecord? CriminalRecord = null;
    public GeneralStationRecord? StationRecord = null;
    public SecurityStatus FilterStatus = SecurityStatus.None;
    public readonly Dictionary<uint, string>? RecordListing;
    public readonly StationRecordsFilter? Filter;

    public CriminalRecordsConsoleState(Dictionary<uint, string>? recordListing, StationRecordsFilter? newFilter)
    {
        RecordListing = recordListing;
        Filter = newFilter;
    }

    /// <summary>
    /// Default state for opening the console
    /// </summary>
    public CriminalRecordsConsoleState() : this(null, null)
    {
    }

    public bool IsEmpty() => SelectedKey == null && StationRecord == null && CriminalRecord == null && RecordListing == null;
}

/// <summary>
/// Used to change status, respecting the wanted/reason nullability rules in <see cref="CriminalRecord"/>.
/// </summary>
[Serializable, NetSerializable]
public sealed class CriminalRecordChangeStatus : BoundUserInterfaceMessage
{
    public readonly SecurityStatus Status;
    public readonly string? Reason;

    public CriminalRecordChangeStatus(SecurityStatus status, string? reason)
    {
        Status = status;
        Reason = reason;
    }
}

/// <summary>
/// Used to add a single line to the record's crime history.
/// </summary>
[Serializable, NetSerializable]
public sealed class CriminalRecordAddHistory : BoundUserInterfaceMessage
{
    public readonly string Line;

    public CriminalRecordAddHistory(string line)
    {
        Line = line;
    }
}

/// <summary>
/// Used to delete a single line from the crime history, by index.
/// </summary>
[Serializable, NetSerializable]
public sealed class CriminalRecordDeleteHistory : BoundUserInterfaceMessage
{
    public readonly uint Index;

    public CriminalRecordDeleteHistory(uint index)
    {
        Index = index;
    }
}

/// <summary>
/// Used to set what status to filter by index.
///
/// </summary>
///
[Serializable, NetSerializable]

public sealed class CriminalRecordSetStatusFilter : BoundUserInterfaceMessage
{
    public readonly SecurityStatus FilterStatus;
    public CriminalRecordSetStatusFilter(SecurityStatus newFilterStatus)
    {
        FilterStatus = newFilterStatus;
    }
}

#region Pirate: records photos
[Serializable, NetSerializable]
public sealed class CriminalRecordEditIdentity : BoundUserInterfaceMessage
{
    public readonly string Species;
    public readonly string Nationality;
    public readonly string Employer;
    public readonly int Age;
    public readonly Gender Gender;

    public CriminalRecordEditIdentity(string species, string nationality, string employer, int age, Gender gender)
    {
        Species = species;
        Nationality = nationality;
        Employer = employer;
        Age = age;
        Gender = gender;
    }
}

[Serializable, NetSerializable]
public sealed class CriminalRecordCreateRecord : BoundUserInterfaceMessage
{
    public readonly string Name;

    public CriminalRecordCreateRecord(string name)
    {
        Name = name;
    }
}

[Serializable, NetSerializable]
public sealed class CriminalRecordEditForensics : BoundUserInterfaceMessage
{
    public readonly string Fingerprint;
    public readonly string Dna;

    public CriminalRecordEditForensics(string fingerprint, string dna)
    {
        Fingerprint = fingerprint;
        Dna = dna;
    }
}

[Serializable, NetSerializable]
public sealed class CriminalRecordPrintPhoto : BoundUserInterfaceMessage
{
    public readonly uint RecordKey;
    public readonly byte[]? GeneratedImageData;

    public CriminalRecordPrintPhoto(uint recordKey, byte[]? generatedImageData = null)
    {
        RecordKey = recordKey;
        GeneratedImageData = generatedImageData == null ? null : (byte[]) generatedImageData.Clone();
    }
}

[Serializable, NetSerializable]
public sealed class CriminalRecordUploadPhoto : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class CriminalRecordStoreGeneratedPhoto : BoundUserInterfaceMessage
{
    public readonly uint RecordKey;
    public readonly byte[] ImageData;

    public CriminalRecordStoreGeneratedPhoto(uint recordKey, byte[] imageData)
    {
        RecordKey = recordKey;
        ImageData = (byte[]) imageData.Clone();
    }
}
#endregion
