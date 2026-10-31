using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Server.StationEvents.Metric;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Server.RoundEnd;
using Content.Server.StationEvents;
using Content.Shared.Database;
using Content.Shared.EntityTable;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

[UsedImplicitly]
public sealed partial class GameDirectorSystem : GameRuleSystem<GameDirectorComponent>
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private EventManagerSystem _event = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IComponentFactory _factory = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private IConfigurationManager _configManager = default!;
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    private int _gameDirectorDebugPlayerCount;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_configManager, GoobCVars.GameDirectorDebugPlayerCount, x => _gameDirectorDebugPlayerCount = x, true);
    }

    [SubscribeLocalEvent]
    private void OnUnpaused(Entity<GameDirectorComponent> ent, ref EntityUnpausedEvent args)
    {
        var scheduler = ent.Comp;
        scheduler.BeatStart += args.PausedTime;
        scheduler.TimeNextEvent += args.PausedTime;
        scheduler.LastBudgetUpdate += args.PausedTime;

        if (scheduler.LastReinforcement != TimeSpan.Zero)
            scheduler.LastReinforcement += args.PausedTime;

        for (var i = 0; i < scheduler.FiredEvents.Count; i++)
        {
            var fired = scheduler.FiredEvents[i];
            scheduler.FiredEvents[i] = fired with { Time = fired.Time + args.PausedTime };
        }
    }

    [SubscribeLocalEvent]
    private void OnRoundRestartCleanup(RoundRestartCleanupEvent args)
    {
        ResetMetrics();
    }

    protected override void Added(EntityUid uid, GameDirectorComponent scheduler, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        Log.Info($"Game Director Spawned at {uid}");
        ResetMetrics();
        TrySpawnRoundstartAntags(scheduler);
    }

    protected override void Ended(EntityUid uid, GameDirectorComponent scheduler, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        ResetMetrics();
    }

    protected override void ActiveTick(EntityUid uid, GameDirectorComponent scheduler, GameRuleComponent gameRule, float frameTime)
    {
        var currTime = _timing.CurTime;
        if (currTime < scheduler.TimeNextEvent)
            return;

        if (scheduler.Stories is not { Length: > 0 })
        {
            GameTicker.EndGameRule(uid, gameRule);
            return;
        }

        var ent = new Entity<GameDirectorComponent>(uid, scheduler);
        var chaos = CalculateChaos(uid);
        scheduler.CurrentChaos = chaos;
        LogMessage($"Chaos is: {chaos}", false);

        var count = CountActivePlayers();
        ActivePlayers.Set(count.Players);
        ActiveGhosts.Set(count.Ghosts);
        UpdateStaffingMetrics(count);
        UpdateAntagMetrics();

        var beat = DetermineNextBeat(ent, chaos, count);

        if (scheduler.TimeNextEvent == TimeSpan.Zero)
        {
            var minimumTimeUntilFirstEvent = _configManager.GetCVar(GoobCVars.MinimumTimeUntilFirstEvent) / _event.EventSpeedup;
            scheduler.TimeNextEvent = currTime + TimeSpan.FromSeconds(minimumTimeUntilFirstEvent);
            scheduler.LastBudgetUpdate = currTime;
            LogMessage($"Started, first event in {minimumTimeUntilFirstEvent} seconds");
            return;
        }

        RegenerateDestructionBudget(scheduler, currTime);

        var pending = CalculatePendingChaos(scheduler, currTime);
        var effective = chaos + pending;
        var goal = EffectiveGoal(scheduler, beat, count);
        UpdateChaosMetrics(chaos, pending, goal, scheduler.DestructionBudget);

        var chosenEvent = ChooseEvent(ent, beat, goal, effective, count);
        if (chosenEvent == null)
        {
            LogMessage($"No events ran (chaos {effective})", false);
            scheduler.TimeNextEvent = currTime + scheduler.NoEventRetryDelay / _event.EventSpeedup;
            return;
        }

        RunEvent(ent, beat, chosenEvent, currTime);
        scheduler.TimeNextEvent = currTime + TimeSpan.FromSeconds(_random.NextFloat(beat.EventDelayMin, beat.EventDelayMax) / _event.EventSpeedup);
    }

    private void RunEvent(Entity<GameDirectorComponent> ent, StoryBeatPrototype beat, PossibleEvent chosen, TimeSpan currTime)
    {
        var scheduler = ent.Comp;
        _event.RunNamedEvent(chosen.StationEvent);

        scheduler.FiredEvents.Add(new FiredEvent(chosen.StationEvent, currTime, chosen.Chaos));
        scheduler.DestructionBudget = MathF.Max(0f, scheduler.DestructionBudget - chosen.Destruction);
        EventsRunTotal.WithLabels(chosen.StationEvent, beat.ID).Inc();
        DestructionBudgetGauge.Set(scheduler.DestructionBudget);
    }

    private void RegenerateDestructionBudget(GameDirectorComponent scheduler, TimeSpan currTime)
    {
        var minutes = (float) (currTime - scheduler.LastBudgetUpdate).TotalMinutes * _event.EventSpeedup;
        scheduler.LastBudgetUpdate = currTime;
        scheduler.DestructionBudget = MathF.Min(scheduler.MaxDestructionBudget,
            scheduler.DestructionBudget + minutes * scheduler.DestructionRegenPerMinute);
    }

    private ChaosMetrics CalculatePendingChaos(GameDirectorComponent scheduler, TimeSpan currTime)
    {
        var keep = scheduler.PendingDuration > scheduler.RepeatWindow ? scheduler.PendingDuration : scheduler.RepeatWindow;
        scheduler.FiredEvents.RemoveAll(fired => (currTime - fired.Time) * _event.EventSpeedup > keep);

        var pending = new ChaosMetrics();
        foreach (var fired in scheduler.FiredEvents)
        {
            var age = (currTime - fired.Time) * _event.EventSpeedup;
            var remaining = 1.0 - age / scheduler.PendingDuration;
            if (remaining <= 0)
                continue;

            foreach (var (metric, value) in fired.Chaos.ChaosDict)
                pending.ChaosDict[metric] = pending.ChaosDict.GetValueOrDefault(metric) + value * remaining;
        }

        return pending;
    }

    private void LogMessage(string message, bool showChat = true)
    {
        _adminLogger.Add(LogType.GameDirector, showChat ? LogImpact.Medium : LogImpact.Low, $"{message}");
        if (showChat)
            _chat.SendAdminAnnouncement("GameDirector " + message);
    }

    public ChaosMetrics CalculateChaos(EntityUid uid)
    {
        var calcEvent = new CalculateChaosEvent(new ChaosMetrics());
        RaiseLocalEvent(uid, ref calcEvent);

        var metrics = calcEvent.Metrics;

        metrics.ChaosDict[ChaosMetric.Combat] = metrics.ChaosDict.GetValueOrDefault(ChaosMetric.Friend) +
                                                metrics.ChaosDict.GetValueOrDefault(ChaosMetric.Hostile);
        return calcEvent.Metrics;
    }
}
