using System.Linq;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Server.StationEvents.Metric;
using Content.Server.Antag.Components;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.StationEvents.Components;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{
    [Dependency] private GhostRoleSystem _ghostRoles = default!;

    private bool UpdateRelax(GameDirectorComponent scheduler, ChaosMetrics chaos, PlayerCount count, TimeSpan now)
    {
        var popScale = PopulationScale(scheduler, count);
        ChaosMetric? worst = null;
        var worstStrain = 0f;
        foreach (var metric in scheduler.RelaxMetrics)
        {
            var strain = (float) chaos.ChaosDict.GetValueOrDefault(metric) / MetricScale(scheduler, metric, popScale);
            if (strain <= worstStrain)
                continue;

            worstStrain = strain;
            worst = metric;
        }

        if (worst != null && worstStrain >= scheduler.RelaxStrain)
        {
            if (now >= scheduler.RelaxUntil)
                LogMessage($"Station under heavy strain ({worst} at {worstStrain:F1}x normal), only sending help for {scheduler.RelaxTime.TotalMinutes:F0} minutes");

            scheduler.RelaxUntil = now + scheduler.RelaxTime / _event.EventSpeedup;
        }

        var relaxing = now < scheduler.RelaxUntil;
        RelaxGauge.Set(relaxing ? 1 : 0);
        return relaxing;
    }

    private bool TryArmedResponse(Entity<GameDirectorComponent> ent,
        StoryBeatPrototype beat,
        ChaosMetrics chaos,
        PlayerCount count,
        TimeSpan now)
    {
        var scheduler = ent.Comp;
        var unmatched = chaos.ChaosDict.GetValueOrDefault(ChaosMetric.Security);
        var threshold = scheduler.ArmedResponseThreshold * PopulationScale(scheduler, count);
        if (unmatched < threshold)
        {
            scheduler.OverwhelmedSince = TimeSpan.Zero;
            return false;
        }

        if (scheduler.OverwhelmedSince == TimeSpan.Zero)
        {
            scheduler.OverwhelmedSince = now;
            return false;
        }

        if ((now - scheduler.OverwhelmedSince) * _event.EventSpeedup < scheduler.ArmedResponseDelay
            || scheduler.ArmedResponseCount >= scheduler.MaxArmedResponses
            || scheduler.ArmedResponses.Count == 0
            || count.Ghosts < scheduler.ArmedResponseGhosts
            || scheduler.LastArmedResponse != TimeSpan.Zero
            && (now - scheduler.LastArmedResponse) * _event.EventSpeedup < scheduler.ArmedResponseCooldown)
            return false;

        for (var i = Math.Min(scheduler.ArmedResponseCount, scheduler.ArmedResponses.Count - 1); i < scheduler.ArmedResponses.Count; i++)
        {
            var response = scheduler.ArmedResponses[i];
            if (!_prototypeManager.TryIndex(response, out var proto)
                || !proto.TryComp<StationEventComponent>(out var stationEvent, _factory)
                || !_event.CanRun(proto, stationEvent, count.InGame, TimeSpan.Zero))
                continue;

            var rule = GameTicker.AddGameRule(response);
            scheduler.FiredEvents.Add(new FiredEvent
            {
                Event = response,
                Rule = rule,
                Time = now,
                Before = new ChaosMetrics(chaos),
            });

            scheduler.ArmedResponseCount++;
            scheduler.LastArmedResponse = now;
            scheduler.OverwhelmedSince = TimeSpan.Zero;
            EventsRunTotal.WithLabels(response, beat.ID).Inc();
            LogMessage($"Security has been outmatched for over {scheduler.ArmedResponseDelay.TotalMinutes:F0} minutes " +
                       $"({unmatched:F0} unmatched threat). Sending {response}");
            return true;
        }

        return false;
    }

    private void CleanupSpawners(GameDirectorComponent scheduler, TimeSpan now, bool all)
    {
        var expired = new HashSet<EntityUid>();
        foreach (var purchase in scheduler.Purchases)
        {
            if (purchase.Roundstart
                || !purchase.GhostRole
                || purchase.SpawnersCleared
                || !all && (now - purchase.Time) * _event.EventSpeedup < scheduler.SpawnerLifetime)
                continue;

            purchase.SpawnersCleared = true;
            expired.Add(purchase.Rule);
        }

        if (expired.Count == 0)
            return;

        var open = _ghostRoles.GetGhostRolesInfo(null).Select(info => info.Identifier).ToHashSet();
        var removed = 0;
        var query = EntityQueryEnumerator<GhostRoleAntagSpawnerComponent, GhostRoleComponent>();
        while (query.MoveNext(out var uid, out var spawner, out var role))
        {
            if (spawner.Rule is not { } rule || !expired.Contains(rule) || !open.Contains(role.Identifier))
                continue;

            QueueDel(uid);
            removed++;
        }

        if (removed > 0)
            LogMessage($"Removed {removed} untaken antag spawners", false);
    }
}
