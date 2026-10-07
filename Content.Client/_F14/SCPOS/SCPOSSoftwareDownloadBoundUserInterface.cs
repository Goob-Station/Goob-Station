// ==========================================================
// Content.Client/_F14/SCPOS/SCPOSSoftwareDownloadBoundUserInterface.cs
// ==========================================================
using Content.Shared._F14.SCPOS;

namespace Content.Client._F14.SCPOS;

public sealed class SCPOSSoftwareDownloadBoundUserInterface : BoundUserInterface
{
    private SCPOSSoftwareDownloadWindow? _window;

    public SCPOSSoftwareDownloadBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = new SCPOSSoftwareDownloadWindow();
        _window.OnClose += Close;
        _window.OnDownloadPressed += id => SendMessage(new SCPOSDownloadSoftwareMessage(id));
        _window.OnUninstallPressed += id => SendMessage(new SCPOSUninstallSoftwareMessage(id));
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is SCPOSSoftwareStoreState softwareState)
            _window?.UpdateState(softwareState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }
}
