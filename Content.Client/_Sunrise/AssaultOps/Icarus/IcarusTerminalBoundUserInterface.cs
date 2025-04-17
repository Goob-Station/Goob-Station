using Content.Shared._Sunrise.AssaultOps.Icarus;

namespace Content.Client._Sunrise.AssaultOps.Icarus;

public sealed class IcarusTerminalBoundUserInterface : BoundUserInterface
{
    private IcarusTerminalWindow? _window;

    public IcarusTerminalBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = new IcarusTerminalWindow();
        _window.OnClose += Close;
        _window.OpenCentered();

        _window.FireButtonPressed += OnFireButtonPressed;
    }

    private void OnFireButtonPressed()
    {
        if (_window == null)
            return;

        SendMessage(new IcarusTerminalFireMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null)
            return;

        if (state is not IcarusTerminalUiState cast)
            return;

        _window.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        if (_window != null)
            _window.OnClose -= Close;

        _window?.Dispose();
    }
}
