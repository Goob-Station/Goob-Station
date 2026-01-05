using Robust.Shared.Serialization;

namespace Content.Shared.Elevator;

[Serializable, NetSerializable]
public sealed class ElevatorGoToFloorMessage : BoundUserInterfaceMessage
{
    public int FloorId;

    public ElevatorGoToFloorMessage(int floorId)
    {
        FloorId = floorId;
    }
}

[Serializable, NetSerializable]
public sealed class ElevatorBuiState : BoundUserInterfaceState
{
    public int CurrentFloor;
    public List<(int id, string name)> Floors;

    public ElevatorBuiState(
        int currentFloor,
        List<(int, string)> floors)
    {
        CurrentFloor = currentFloor;
        Floors = floors;
    }
}
