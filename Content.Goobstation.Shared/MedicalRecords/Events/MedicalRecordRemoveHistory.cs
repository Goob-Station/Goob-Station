using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MedicalRecords;

[Serializable, NetSerializable]
public sealed class MedicalRecordRemoveHistory : BoundUserInterfaceMessage
{
    public readonly uint Index;

    public MedicalRecordRemoveHistory(uint index)
    {
        Index = index;
    }
}
