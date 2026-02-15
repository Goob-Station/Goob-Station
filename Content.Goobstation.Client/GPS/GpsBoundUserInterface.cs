using Content.Goobstation.Shared.GPS;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.GPS;

[UsedImplicitly]
public sealed class GpsBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private GpsWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<GpsWindow>();
        _window.OnClose += Close;
        _window.TrackedEntitySelected += OnTrackedEntitySelected;
        _window.GpsNameChanged += OnGpsNameChanged;
        _window.DistressPressed += OnDistressPressed;
        _window.EnabledPressed += OnEnabledPressed;

        UpdateWindow();

        _window.UpdateGpsName(Owner); // Because of shitty logic i have to do that here
    }

    public void UpdateWindow()
    {
        if (_window != null)
            _window.UpdateState(Owner);
    }

    private void OnTrackedEntitySelected(NetEntity? netEntity)
    {
        SendPredictedMessage(new GpsSetTrackedEntityMessage(netEntity));
    }

    private void OnGpsNameChanged(string newName)
    {
        SendPredictedMessage(new GpsSetGpsNameMessage(newName));
    }

    private void OnDistressPressed(bool distressed)
    {
        SendPredictedMessage(new GpsSetInDistressMessage(distressed));
    }

    private void OnEnabledPressed(bool enabled)
    {
        SendPredictedMessage(new GpsSetEnabledMessage(enabled));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _window?.Close();
            _window = null;
        }
    }
}
