using Content.Goobstation.Shared.Virology;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Virology;

public sealed class VirologyConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private VirologyConsoleWindow? _menu;

    public VirologyConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<VirologyConsoleWindow>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case VirologyConsoleState st:
                // TODO virology
                break;
        }
    }
}
