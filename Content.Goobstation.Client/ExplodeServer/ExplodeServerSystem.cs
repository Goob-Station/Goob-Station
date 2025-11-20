using Content.Goobstation.Shared.ExplodeServer;
using Content.Shared.GameTicking;
using Robust.Shared.Timing;
using Robust.Client.Graphics;


namespace Content.Goobstation.Client.ExplodeServer;

public sealed class ExplodeServerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    private bool _started;
    private TimeSpan _roundEndOverlayTime; // for how long to have the overlay on
    
    private ExplodeServerOverlay _overlay = new()
    {
        TintColor = new(255f, 0f, 0f),
        BlurAmount = 1f,
        IsActive = false
    };

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var remainingTime = _roundEndOverlayTime - _gameTiming.CurTime;
        if (remainingTime.TotalSeconds <= 0 && !_started)
        {
            return;
        }
        if (remainingTime.TotalMilliseconds <= 5105) // Start overlay and blink
        {
            if (remainingTime.TotalSeconds % 1.25d < 0.5d)
            {
                _overlay.IsActive = false;
                _overlayManager.RemoveOverlay(_overlay);
                return;
            }
            else
            {
                _overlay.IsActive = true;
                _overlayManager.AddOverlay(_overlay);
            }
        }
    }
    public override void Initialize()
    {
        base.Initialize();
        _started = false;
        _overlay.TintColor = Color.FromHex("#ff0000ff");
        SubscribeNetworkEvent<ExplodeServerEvent>(OnExplodeServer);
        SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }
    private void OnExplodeServer(ExplodeServerEvent ev)
    {
        _started = true;
        _roundEndOverlayTime = _gameTiming.CurTime + TimeSpan.FromMilliseconds(5105);
    }
    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _overlay.IsActive = false;
        _started = false;
        _roundEndOverlayTime = TimeSpan.Zero;
        _overlayManager.RemoveOverlay(_overlay);
    }
}
