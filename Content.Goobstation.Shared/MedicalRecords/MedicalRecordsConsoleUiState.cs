using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MedicalRecords;

[Serializable, NetSerializable]
public sealed class MedicalRecordsConsoleUiState : BoundUserInterfaceState
{
    /// <summary>
    /// Currently selected crewmember record key.
    /// </summary>
    public uint? SelectedKey = null;
    public MedicalRecord? MedicalRecord = null;
    public GeneralStationRecord? StationRecord = null;
    public readonly Dictionary<uint, string>? RecordListing;
    public readonly StationRecordsFilter? Filter;

    public MedicalRecordsConsoleUiState(Dictionary<uint, string>? recordListing, StationRecordsFilter? newFilter)
    {
        RecordListing = recordListing;
        Filter = newFilter;
    }

    /// <summary>
    /// Default state for opening the console
    /// </summary>
    public MedicalRecordsConsoleUiState() : this(null, null)
    {
    }

    public bool IsEmpty() => SelectedKey == null && StationRecord == null && MedicalRecord == null && RecordListing == null;
}

[Serializable, NetSerializable]
public enum MedicalRecordsConsoleKey : byte
{
    Key
}
