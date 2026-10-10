using Content.Shared._ES.Camera;
using Content.Shared.GameTicking;
using Robust.Shared.Player;

namespace Content.Shared.Gravity;

public abstract partial class SharedGravitySystem
{
    [Dependency] private SharedGameTicker _ticker = default!;
    [Dependency] private ESScreenshakeSystem _shake = default!;

    private static readonly TimeSpan RoundStartShakeGrace = TimeSpan.FromSeconds(30);
    private static readonly ESScreenshakeParameters GridShake = new(0.8f, 0.04f, 0.015f);

    private bool TryScreenshakeGrid(EntityUid uid)
    {
        if (Timing.CurTime - _ticker.RoundStartTimeSpan < RoundStartShakeGrace)
            return true;

        _shake.Screenshake(Filter.BroadcastGrid(uid), GridShake, null);
        return true;
    }
}
