using Content.Goobstation.Server.ExplodeServer;
using Content.Goobstation.Shared.ExplodeServer;
using Content.Server.GameTicking;
using Robust.Shared.Timing;
using Robust.Shared.Toolshed.Commands.GameTiming;

namespace Content.Goobstation.Client.ExplodeServer;

public sealed class ExplodeServerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!; // Goobstation
    [Dependency] private readonly IEntityManager _e = default!;
    
    private TimeSpan? _roundEndOverlayTime; // for how long to have the overlay on
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_roundEndOverlayTime == null)
            return;
        if (_gameTiming.CurTime <= _roundEndOverlayTime) // Start overlay and blink
        {
            
        }

        if (_gameTiming.CurTime >= _roundEndOverlayTime) // Restart round
        {
            _e.System<GameTicker>().RestartRound();
        }
    }
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeNetworkEvent<ExplodeServerEvent>(OnExplodeServer);
    }

    protected void OnExplodeServer(ExplodeServerEvent e)
    {
        var check = e.IsExploding;
        if (check)
        {
            _roundEndOverlayTime = _gameTiming.CurTime + TimeSpan.FromSeconds(5);
        }
    }
}
