using Content.Client._Lavaland.GPS.UI;
using Content.Shared._Lavaland.GPS;
using Robust.Client.UserInterface;

namespace Content.Client._Lavaland.GPS.UI;

public sealed class GpsBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private GpsWindow? _window;

    public GpsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<GpsWindow>();
        _window.OpenCenteredLeft();
        _window.Title = Loc.GetString("gps-ui-title");

        _window.OnRequestRefresh += () => SendMessage(new GpsRefreshMessage());
        _window.OnChangeRangeMode += range => SendMessage(new GpsRefreshRangeMessage(range));
        _window.OnChangeScanMode += mode => SendMessage(new GpsRefreshModeMessage(mode));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is GpsSignalLocatorState cast)
            _window?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window?.Orphan();
    }
}
