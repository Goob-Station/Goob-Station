using Content.Server.Players.PlayTimeTracking;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CorvaxGoob.Players.GhostTimeTracking;
public sealed class GhostTimeTrackingSystem : EntitySystem
{
    private TimeSpan _lastUpdate = TimeSpan.Zero;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(60);

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly GhostTimeTrackingManager _ghostTimeTracking = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundRestartCleanupEvent ev)
    {
        _ghostTimeTracking.ClearAllGhostTimeData(); // don't forget to clean up all saved shit
    }

    public override void Update(float frameTime)
    {
        if (_timing.CurTime < _lastUpdate + _updateInterval)
            return;

        _lastUpdate = _timing.CurTime;

        var query = EntityQueryEnumerator<GhostComponent>();

        while (query.MoveNext(out var uid, out _))
            UpdateAndSendGhostTime(uid);
    }

    public void UpdateAndSendGhostTime(ICommonSession session)
    {
        _ghostTimeTracking.UpdatePlayerGhostTime(session, _updateInterval);
        _playTimeTracking.QueueSendTimers(session);
    }

    public void UpdateAndSendGhostTime(EntityUid uid)
    {
        if (!_players.TryGetSessionByEntity(uid, out var session))
            return;

        _ghostTimeTracking.UpdatePlayerGhostTime(session, _updateInterval);
        _playTimeTracking.QueueSendTimers(session);
    }
}
