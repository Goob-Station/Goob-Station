using Content.Goobstation.Shared.Gangwars.Events;

namespace Content.Goobstation.Client.Gangwars;

public sealed class ClientGangLeaderSystem : EntitySystem
{
    private GangCreatorWindow? _colorPickerWindow;
    private GangInviteWindow? _inviteWindow;
    private NetEntity _pendingInviteLeader;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<GangLeaderNeedsColorPickEvent>(OnNeedsColorPick);
        SubscribeNetworkEvent<GangInviteOfferEvent>(OnInviteOffer);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_inviteWindow is { Disposed: false } window && window.TryTimedOut())
        {
            RaiseNetworkEvent(new GangInviteResponseEvent(false, _pendingInviteLeader));
            _inviteWindow.Close();
        }
    }

    private void OnNeedsColorPick(GangLeaderNeedsColorPickEvent args)
    {
        if (_colorPickerWindow is { Disposed: false })
        {
            _colorPickerWindow.OpenCentered();
            return;
        }

        _colorPickerWindow = new GangCreatorWindow(args.GangNames);
        _colorPickerWindow.OnColorPicked += (color, name) =>
        {
            RaiseNetworkEvent(new GangColorChosenEvent(color, name));
        };
        _colorPickerWindow.OpenCentered();
    }

    private void OnInviteOffer(GangInviteOfferEvent args)
    {
        _inviteWindow?.Close();
        _pendingInviteLeader = args.LeaderEntity;

        _inviteWindow = new GangInviteWindow(args.LeaderName, args.GangColor, args.GangName);
        _inviteWindow.OnAccepted += () =>
        {
            RaiseNetworkEvent(new GangInviteResponseEvent(true, _pendingInviteLeader));
            _inviteWindow?.Close();
        };
        _inviteWindow.OnDenied += () =>
        {
            RaiseNetworkEvent(new GangInviteResponseEvent(false, _pendingInviteLeader));
            _inviteWindow?.Close();
        };
        _inviteWindow.OpenCentered();
    }
}
