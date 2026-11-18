using JetBrains.Annotations;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.CursorTimer;

public sealed partial class CursorTimerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;

    /// <summary>
    /// Starts, or restarts, the cursor countdown so it ends at <paramref name="endTime"/>.
    /// </summary>
    /// <returns>The timer, so callers can adjust its appearance.</returns>
    [PublicAPI]
    public Entity<CursorTimerComponent> SetTimer(EntityUid uid, TimeSpan endTime)
    {
        var timer = EnsureComp<CursorTimerComponent>(uid);
        timer.EndTime = endTime;
        Dirty(uid, timer);
        return (uid, timer);
    }

    [PublicAPI]
    public void ClearTimer(EntityUid uid)
    {
        RemCompDeferred<CursorTimerComponent>(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CursorTimerComponent>();
        while (query.MoveNext(out var uid, out var timer))
        {
            if (_timing.CurTime >= timer.EndTime)
                RemCompDeferred<CursorTimerComponent>(uid);
        }
    }
}
