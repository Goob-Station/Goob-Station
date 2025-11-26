using Content.Shared.Security;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MedicalRecords;

[Serializable, NetSerializable, DataRecord]
public sealed partial record MedicalRecord
{
    [DataField]
    public List<MedicalHistory> History = new();
}

[Serializable, NetSerializable]
public record struct MedicalHistory(TimeSpan? AddTime, string Message, string? InitiatorName);
