using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MedicalRecords;

[Serializable, NetSerializable]
public sealed class MedicalRecordAddHistory : BoundUserInterfaceMessage
{
    public readonly string Line;

    public MedicalRecordAddHistory(string line)
    {
        Line = line;
    }
}
