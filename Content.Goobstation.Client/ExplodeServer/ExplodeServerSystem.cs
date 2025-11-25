using Content.Goobstation.Common.ExplodeServer;
using Content.Shared.GameTicking;
using Robust.Client.Graphics;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.ExplodeServer;

public sealed class ExplodeServerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    private TimeSpan? _roundEndOverlayTime; // for how long to have the overlay on

    private readonly ExplodeServerScreenSpaceOverlay _screenSpaceOverlay = new()
    {
        IsActive = false
    };

    private readonly ExplodeServerWorldSpaceOverlay _worldSpaceOverlay = new()
    {
        TintColor = new Color(255f, 0f, 0f),
        BlurAmount = 1f,
        IsActive = false
    };

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_roundEndOverlayTime < _gameTiming.CurTime && _roundEndOverlayTime != null) ; // Start overlay and blink
        {
            var remainingTime = _roundEndOverlayTime - _gameTiming.CurTime;
            if (remainingTime?.TotalSeconds % 1.25d < 0.5d)
                RemoveOverlays();
            else
                AddOverlays();
        }
    }

    private void AddOverlays()
    {
        _worldSpaceOverlay.IsActive = true;
        _screenSpaceOverlay.IsActive = true;
        _overlayManager.AddOverlay(_worldSpaceOverlay);
        _overlayManager.AddOverlay(_screenSpaceOverlay);
    }

    private void RemoveOverlays()
    {
        _worldSpaceOverlay.IsActive = false;
        _worldSpaceOverlay.IsActive = true;
        _overlayManager.RemoveOverlay(_worldSpaceOverlay);
        _overlayManager.RemoveOverlay(_screenSpaceOverlay);
    }

    public override void Initialize()
    {
        base.Initialize();
        _worldSpaceOverlay.TintColor = Color.FromHex("#ff0000ff");
        SubscribeNetworkEvent<ExplodeServerEvent>(OnExplodeServer);
        SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnExplodeServer(ExplodeServerEvent ev)
    {
        _roundEndOverlayTime = _gameTiming.CurTime + TimeSpan.FromMilliseconds(5105);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        RemoveOverlays();
        _roundEndOverlayTime = TimeSpan.Zero;
    }
}
