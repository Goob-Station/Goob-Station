using Content.Shared.Elevator;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Elevator.UI;

public sealed class ElevatorBoundUserInterface : BoundUserInterface
{
    private ElevatorMenu? _menu;

    public ElevatorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ElevatorMenu>();
        _menu.OnFloorPressed += floorId =>
        {
            SendMessage(new ElevatorGoToFloorMessage(floorId));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (_menu == null || state is not ElevatorBuiState s)
            return;

        _menu.UpdateFloors(s);
    }
}
