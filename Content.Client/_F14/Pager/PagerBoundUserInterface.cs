using Content.Shared._F14.Pager;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Client._F14.Pager;

public sealed class PagerBoundUserInterface : BoundUserInterface
{
    private PagerWindow? _window;

    public PagerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = new PagerWindow();
        _window.OnClose += Close;
        _window.OnButtonPressed += btn => SendMessage(new PagerButtonPressedMessage(btn));
        _window.OnMessageSent += (recipient, text) => SendMessage(new PagerSendMessageAlert(recipient, text));
        _window.OnCallRequested += target => SendMessage(new PagerCallRequestMessage(target));
        _window.OnSoftwareDownloadRequested += id => SendMessage(new PagerDownloadSoftwareMessage(id));
        _window.OnSoftwareUninstallRequested += id => SendMessage(new PagerUninstallSoftwareMessage(id));
        _window.OnEjectIdRequested += () => SendMessage(new PagerEjectIdMessage());
        _window.OnNoteAdded += text => SendMessage(new PagerAddNoteMessage(text));
        _window.OnNoteDeleted += idx => SendMessage(new PagerDeleteNoteMessage(idx));
        _window.OnCycleShieldsRequested += () => SendMessage(new PagerCycleShieldsMessage());
        _window.OnSupplyOrdered += id => SendMessage(new PagerOrderSupplyMessage(id));
        _window.OnScanTriggered += () => SendMessage(new PagerTriggerScanMessage());
        _window.OnIdInfoSet += (name, jobTitle) => SendMessage(new PagerSetIdInfoMessage(name, jobTitle));
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is PagerBoundUserInterfaceState pagerState)
            _window?.UpdateState(pagerState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }
}
