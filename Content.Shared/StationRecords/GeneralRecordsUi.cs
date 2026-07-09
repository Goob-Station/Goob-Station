// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.StationRecords;

[Serializable, NetSerializable]
public enum GeneralStationRecordConsoleKey : byte
{
    Key
}

/// <summary>
///     General station records console state. There are a few states:
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
public sealed class GeneralStationRecordConsoleState : BoundUserInterfaceState
{
    /// <summary>
    /// Current selected key.
    /// Station is always the station that owns the console.
    /// </summary>
    public readonly uint? SelectedKey;
    public readonly GeneralStationRecord? Record;
    public readonly Dictionary<uint, string>? RecordListing;
    public readonly StationRecordsFilter? Filter;
    public readonly bool CanDeleteEntries;
    public readonly uint MaxStringLength; // Pirate: records photos

    public GeneralStationRecordConsoleState(uint? key,
        GeneralStationRecord? record,
        Dictionary<uint, string>? recordListing,
        StationRecordsFilter? newFilter,
        bool canDeleteEntries,
        uint maxStringLength) // Pirate: records photos
    {
        SelectedKey = key;
        Record = record;
        RecordListing = recordListing;
        Filter = newFilter;
        CanDeleteEntries = canDeleteEntries;
        MaxStringLength = maxStringLength; // Pirate: records photos
    }

    public GeneralStationRecordConsoleState() : this(null, null, null, null, false, 256) // Pirate: records photos
    {
    }

    public bool IsEmpty() => SelectedKey == null
        && Record == null && RecordListing == null;
}

/// <summary>
/// Select a specific crewmember's record, or deselect.
/// Used by any kind of records console including general and criminal.
/// </summary>
[Serializable, NetSerializable]
public sealed class SelectStationRecord : BoundUserInterfaceMessage
{
    public readonly uint? SelectedKey;

    public SelectStationRecord(uint? selectedKey)
    {
        SelectedKey = selectedKey;
    }
}


[Serializable, NetSerializable]
public sealed class DeleteStationRecord : BoundUserInterfaceMessage
{
    public DeleteStationRecord(uint id)
    {
        Id = id;
    }

    public readonly uint Id;
}

#region Pirate: records photos
[Serializable, NetSerializable]
public sealed class GeneralRecordCreateRecord : BoundUserInterfaceMessage
{
    public readonly string Name;

    public GeneralRecordCreateRecord(string name)
    {
        Name = name;
    }
}

[Serializable, NetSerializable]
public sealed class GeneralRecordEditIdentity : BoundUserInterfaceMessage
{
    public readonly string Species;
    public readonly string Nationality;
    public readonly string Employer;
    public readonly int Age;
    public readonly Robust.Shared.Enums.Gender Gender;

    public GeneralRecordEditIdentity(string species, string nationality, string employer, int age, Robust.Shared.Enums.Gender gender)
    {
        Species = species;
        Nationality = nationality;
        Employer = employer;
        Age = age;
        Gender = gender;
    }
}

[Serializable, NetSerializable]
public sealed class GeneralRecordEditForensics : BoundUserInterfaceMessage
{
    public readonly string Fingerprint;
    public readonly string Dna;

    public GeneralRecordEditForensics(string fingerprint, string dna)
    {
        Fingerprint = fingerprint;
        Dna = dna;
    }
}
#endregion
