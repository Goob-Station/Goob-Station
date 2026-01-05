using Robust.Shared.Serialization;

namespace Content.Shared.Elevator;

[Serializable, NetSerializable]
public sealed class ElevatorBuiState : BoundUserInterfaceState
{
    [DataField]
    public int CurrentFloor;

    [DataField]
    public List<ElevatorFloorUiData> Floors = new();

    public ElevatorBuiState(
        int currentFloor,
        List<ElevatorFloorUiData> floors)
    {
        CurrentFloor = currentFloor;
        Floors = floors;
    }
}

[Serializable, NetSerializable]
public sealed class ElevatorGoToFloorMessage : BoundUserInterfaceMessage
{
    [DataField]
    public int FloorId;

    public ElevatorGoToFloorMessage(int floorId)
    {
        FloorId = floorId;
    }
}
