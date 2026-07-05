using Content.Goobstation.Shared.MalfunctionAi;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.MalfunctionAi;

/// <summary>
/// Window listing the Malfunction AI's subverted cyborgs.
/// </summary>
public sealed class MalfBorgsBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private MalfBorgsWindow? _window;

    public MalfBorgsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MalfBorgsWindow>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MalfBorgsBuiState cast || _window == null)
            return;

        _window.Update(cast.Borgs);
    }
}
