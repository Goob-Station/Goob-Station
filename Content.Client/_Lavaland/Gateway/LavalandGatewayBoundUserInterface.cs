using Content.Shared._Lavaland.Gateway;
using JetBrains.Annotations;

namespace Content.Client._Lavaland.Gateway;

[UsedImplicitly]
public sealed class LavalandGatewayBoundUserInterface : BoundUserInterface
{
    private LavalandGatewayWindow? _window;

    public LavalandGatewayBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new LavalandGatewayWindow();
        _window.OpenPortal += destination =>
        {
            SendMessage(new LavalandGatewayOpenPortalMessage(destination));
        };
        _window.OnClose += Close;
        _window?.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _window?.Dispose();
            _window = null;
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not LavalandGatewayBoundUserInterfaceState current)
            return;

        _window?.UpdateState(current);
    }
}
