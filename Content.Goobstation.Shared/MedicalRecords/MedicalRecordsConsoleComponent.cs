using Content.Shared.StationRecords;

namespace Content.Goobstation.Shared.MedicalRecords;

[RegisterComponent]
public sealed partial class MedicalRecordsConsoleComponent : Component
{
    [DataField]
    public uint? ActiveKey;

    /// <summary>
    /// Currently applied filter.
    /// </summary>
    [DataField]
    public StationRecordsFilter? Filter;
}
