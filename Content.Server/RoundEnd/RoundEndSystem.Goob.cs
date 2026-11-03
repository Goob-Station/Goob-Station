using System.Threading;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.RoundEnd;

public sealed partial class RoundEndSystem
{
    public bool DelayRoundEndCountdown(TimeSpan delay)
    {
        if (_countdownTokenSource == null || ExpectedCountdownEnd is not { } end)
            return false;

        _countdownTokenSource.Cancel();
        _countdownTokenSource = new CancellationTokenSource();

        ExpectedCountdownEnd = end + delay;
        var remaining = ExpectedCountdownEnd.Value - _gameTiming.CurTime;
        if (remaining < TimeSpan.Zero)
            remaining = TimeSpan.Zero;

        Timer.Spawn(remaining, _shuttle.DockEmergencyShuttle, _countdownTokenSource.Token);
        RaiseLocalEvent(RoundEndSystemChangedEvent.Default);
        return true;
    }
}
