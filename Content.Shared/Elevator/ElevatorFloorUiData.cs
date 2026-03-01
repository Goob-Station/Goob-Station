using Robust.Shared.Serialization;

namespace Content.Shared.Elevator;

[Serializable, NetSerializable]
public sealed class ElevatorFloorUiData
{
    [DataField]
    public int Id;

    [DataField]
    public string Name = string.Empty;

    public ElevatorFloorUiData(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
