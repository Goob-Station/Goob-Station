using Content.Goobstation.Shared.GPS;
using JetBrains.Annotations;

namespace Content.Goobstation.Client.GPS;

[UsedImplicitly]
public sealed class GpsBoundUserInterface : BoundUserInterface
{
    private GpsWindow? _window;

    public GpsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = new GpsWindow();
        _window.OnClose += Close;
        _window.TrackedEntitySelected += OnTrackedEntitySelected;
        _window.GpsNameChanged += OnGpsNameChanged;
        _window.DistressPressed += OnDistressPressed;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null || state is not GpsBoundUserInterfaceState gpsState)
            return;

        _window.UpdateState(gpsState);

        if (!_window.IsOpen)
            _window.OpenCentered();
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        base.ReceiveMessage(message);

        if (_window == null)
            return;

        switch (message)
        {
            case GpsNameChangedMessage msg:
                _window.UpdateGpsName(msg.GpsName);
                break;
            case GpsDistressChangedMessage msg:
                _window.UpdateDistress(msg.InDistress);
                break;
            case GpsTrackedEntityChangedMessage msg:
                _window.UpdateTrackedEntity(msg.TrackedEntity);
                break;
            case GpsEntriesChangedMessage msg:
                _window.UpdateGpsEntries(msg.GpsEntries);
                break;
            case GpsUpdateTrackedCoordinatesMessage msg:
                _window.UpdateTrackedCoordinates(msg.NetEntity, msg.Coordinates);
                break;
        }
    }

    private void OnTrackedEntitySelected(NetEntity? netEntity)
    {
        SendMessage(new GpsSetTrackedEntityMessage(netEntity));
    }

    private void OnGpsNameChanged(string newName)
    {
        SendMessage(new GpsSetGpsNameMessage(newName));
    }

    private void OnDistressPressed(bool distressed)
    {
        SendMessage(new GpsSetInDistressMessage(distressed));
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
