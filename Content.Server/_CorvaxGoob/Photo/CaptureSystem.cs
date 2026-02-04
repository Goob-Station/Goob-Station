using Content.Server.Chat.Managers;
using Content.Server.EUI;
using Content.Shared._CorvaxGoob.Photo;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server._CorvaxGoob.Photo;

public sealed class CaptureSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    private List<CaptureScreenRequest> _captureScreenRequests = new();

    private TimeSpan _nextRequestUpdate = TimeSpan.Zero;
    private TimeSpan _requestUpdateInterval = TimeSpan.FromSeconds(1);

    private TimeSpan _requestTimeoutTime = TimeSpan.FromSeconds(20);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CaptureScreenResponseEvent>(CaptureScreenResponse);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_captureScreenRequests.Count == 0)
            return;

        if (_timing.CurTime < _nextRequestUpdate)
            return;

        _nextRequestUpdate = _timing.CurTime + _requestUpdateInterval;

        if (!_captureScreenRequests.Any(req => req.TimeoutTime < _timing.CurTime))
            return;

        foreach (var request in _captureScreenRequests)
        {
            if (request.TimeoutTime < _timing.CurTime)
                _chat.SendAdminAnnouncementMessage(request.RequestBy, Loc.GetString("screen-capture-error-timeout"));
        }

        _captureScreenRequests.RemoveAll(req => req.TimeoutTime < _timing.CurTime);
    }

    public void RequestScreenCapture(ICommonSession session, ICommonSession requestBy)
    {
        if (_captureScreenRequests.Any(req => req.RequestBy == requestBy))
        {
            _chat.SendAdminAnnouncementMessage(requestBy, Loc.GetString("screen-capture-error-repeat-request"));
            return;
        }

        var requestHolder = new CaptureScreenRequest();

        requestHolder.Player = session;
        requestHolder.RequestBy = requestBy;
        requestHolder.TimeoutTime = _timing.CurTime + _requestTimeoutTime;

        _captureScreenRequests.Add(requestHolder);
        RaiseNetworkEvent(new CaptureScreenRequestEvent(), session);
    }

    private void CaptureScreenResponse(CaptureScreenResponseEvent ev, EntitySessionEventArgs args)
    {
        if (ev.Image is null)
            return;

        if (!_captureScreenRequests.Any(req => req.Player == args.SenderSession))
            return;

        foreach (var request in _captureScreenRequests)
        {
            if (request.Player == args.SenderSession)
            {
                var eui = new ImageEui(ev.Image);

                _eui.OpenEui(eui, request.RequestBy);
                eui.DoStateUpdate();
            }
        }

        _captureScreenRequests.RemoveAll(req => req.Player == args.SenderSession);
    }
}

public struct CaptureScreenRequest
{
    public ICommonSession RequestBy;
    public ICommonSession Player;
    public TimeSpan TimeoutTime;
}
