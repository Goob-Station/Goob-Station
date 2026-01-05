using Content.Shared.Elevator;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Elevator.UI;

public sealed class ElevatorBoundUserInterface : BoundUserInterface
{
    private ElevatorMenu? _menu;

    private ElevatorBuiState? _lastState;

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

        if (_lastState != null)
            _menu.UpdateFloors(_lastState);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not ElevatorBuiState s)
            return;

        _lastState = s;

        if (_menu == null)
            return;

        _menu.UpdateFloors(s);
    }

}
