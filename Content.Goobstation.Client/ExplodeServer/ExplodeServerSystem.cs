using Content.Goobstation.Shared.ExplodeServer;
using Robust.Shared.Timing;
using Robust.Client.Graphics;


namespace Content.Goobstation.Client.ExplodeServer;

public sealed class ExplodeServerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    
    private TimeSpan? _roundEndOverlayTime; // for how long to have the overlay on
    private ExplodeServerOverlay _overlay = new()
    {
        TintColor = new(255f, 0f, 0f),
        BlurAmount = 0f
    };
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_roundEndOverlayTime == null)
            return;
        if (_gameTiming.CurTime <= _roundEndOverlayTime) // Start overlay and blink
        {
            
            if ((_gameTiming.CurTime.TotalSeconds % 2 == 0))
            {
                _overlayManager.AddOverlay(_overlay);
            }
            else
            {
                _overlayManager.RemoveOverlay(_overlay);
            }
        }

        if (_gameTiming.CurTime >= _roundEndOverlayTime) // Restart round
        {
            _overlayManager.RemoveOverlay(_overlay);
            RaiseLocalEvent(new ExplodeServerEvent(isExploding: false, exploded:true));
        }
    }
    public override void Initialize()
    {
        base.Initialize();
        
        _overlay.TintColor = Color.FromHex("#ff0000ff");
        
        SubscribeNetworkEvent<ExplodeServerEvent>(OnExplodeServer);
    }

    private void OnExplodeServer(ExplodeServerEvent e)
    {
        var check = e.IsExploding;
        if (check)
        {
            _roundEndOverlayTime = _gameTiming.CurTime + TimeSpan.FromMicroseconds(5105);
        }
    }
}
