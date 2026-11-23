using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Server.StationEvents.Metric;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.RoundEnd;
using Content.Server.StationEvents;
using Content.Server.StationEvents.Components;
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
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning, before: new[] { typeof(AntagSelectionSystem) });
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
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

        if (scheduler.LastHelp != TimeSpan.Zero)
            scheduler.LastHelp += args.PausedTime;

        if (scheduler.RelaxUntil != TimeSpan.Zero)
            scheduler.RelaxUntil += args.PausedTime;

        if (scheduler.OverwhelmedSince != TimeSpan.Zero)
            scheduler.OverwhelmedSince += args.PausedTime;

        if (scheduler.LastArmedResponse != TimeSpan.Zero)
            scheduler.LastArmedResponse += args.PausedTime;

        foreach (var department in scheduler.LastSpotlight.Keys.ToList())
            scheduler.LastSpotlight[department] += args.PausedTime;

        foreach (var purchase in scheduler.Purchases)
            purchase.Time += args.PausedTime;

        foreach (var fired in scheduler.FiredEvents)
            fired.Time += args.PausedTime;
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
        scheduler.Purchases.Clear();

        var connected = GetPlayerCount();
        LogMessage($"Total player count: {connected}", false);
        if (!TryStartNewStory((uid, scheduler), connected, null))
            return;

        if (GameTicker.RunLevel == GameRunLevel.PreRoundLobby)
            PlanRoundstart(scheduler, _prototypeManager.Index(scheduler.CurrentStoryName), EstimateRoundstartPlayers(connected));
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

        if (scheduler.Stories.Count == 0)
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
            foreach (var department in scheduler.SpotlightDepartments.Values)
                scheduler.LastSpotlight[department] = currTime;

            LogMessage($"Started, first event in {minimumTimeUntilFirstEvent} seconds");
            return;
        }

        RegenerateDestructionBudget(scheduler, currTime);
        LogEventEffects(scheduler, chaos, currTime);
        CleanupSpawners(scheduler, currTime, false);

        var pending = CalculatePendingChaos(scheduler, currTime);
        var effective = chaos + pending;
        var goal = EffectiveGoal(scheduler, beat, count);
        UpdateChaosMetrics(chaos, pending, goal, scheduler.DestructionBudget);

        var relaxing = UpdateRelax(scheduler, chaos, count, currTime);
        var threat = MeasureThreat(scheduler, beat, count, currTime);
        UpdateThreatMetrics(threat);

        if (TryArmedResponse(ent, beat, chaos, count, currTime)
            || !relaxing && TryPurchaseThreat(ent, beat, count, threat, currTime))
        {
            ScheduleNextEvent(scheduler, beat, count, currTime);
            return;
        }

        var chosenEvent = ChooseEvent(ent, beat, goal, effective, count, relaxing);
        if (chosenEvent == null)
        {
            LogMessage($"No events ran (chaos {effective})", false);
            scheduler.TimeNextEvent = currTime + scheduler.NoEventRetryDelay / _event.EventSpeedup;
            return;
        }

        RunEvent(ent, beat, chosenEvent, chaos, currTime);
        ScheduleNextEvent(scheduler, beat, count, currTime);
    }

    private void ScheduleNextEvent(GameDirectorComponent scheduler, StoryBeatPrototype beat, PlayerCount count, TimeSpan currTime)
    {
        var delay = _random.NextFloat(beat.EventDelayMin, beat.EventDelayMax) * LowPopDelayScale(scheduler, count);
        scheduler.TimeNextEvent = currTime + TimeSpan.FromSeconds(delay / _event.EventSpeedup);
    }

    private static float LowPopDelayScale(GameDirectorComponent scheduler, PlayerCount count)
    {
        if (count.Players >= scheduler.LowPopPlayers)
            return 1f;

        return MathF.Min(scheduler.MaxLowPopDelayScale, MathF.Sqrt((float) scheduler.LowPopPlayers / Math.Max(1, count.Players)));
    }

    private void RunEvent(Entity<GameDirectorComponent> ent, StoryBeatPrototype beat, PossibleEvent chosen, ChaosMetrics chaos, TimeSpan currTime)
    {
        var scheduler = ent.Comp;
        var rule = GameTicker.AddGameRule(chosen.StationEvent);

        scheduler.FiredEvents.Add(new FiredEvent
        {
            Event = chosen.StationEvent,
            Rule = rule,
            Time = currTime,
            Chaos = chosen.Chaos,
            Before = new ChaosMetrics(chaos),
        });

        if (IsHelpEvent(chosen))
            scheduler.LastHelp = currTime;

        MarkSpotlight(scheduler, chosen.Chaos, currTime);
        scheduler.DestructionBudget = MathF.Max(0f, scheduler.DestructionBudget - chosen.Destruction);
        EventsRunTotal.WithLabels(chosen.StationEvent, beat.ID).Inc();
        DestructionBudgetGauge.Set(scheduler.DestructionBudget);
    }

    private void MarkSpotlight(GameDirectorComponent scheduler, ChaosMetrics chaos, TimeSpan currTime)
    {
        foreach (var (metric, value) in chaos.ChaosDict)
        {
            if (value > 0 && scheduler.SpotlightDepartments.TryGetValue(metric, out var department))
                scheduler.LastSpotlight[department] = currTime;
        }
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
        var keep = new[] { scheduler.PendingDuration, scheduler.RepeatWindow, scheduler.EffectDelay, MaxGroupWindow() }.Max();
        scheduler.FiredEvents.RemoveAll(fired => (currTime - fired.Time) * _event.EventSpeedup > keep);

        var pending = new ChaosMetrics();
        foreach (var fired in scheduler.FiredEvents)
        {
            var remaining = PendingFraction(scheduler, fired, currTime);
            if (remaining <= 0)
                continue;

            foreach (var (metric, value) in fired.Chaos.ChaosDict)
                pending.ChaosDict[metric] = pending.ChaosDict.GetValueOrDefault(metric) + value * remaining;
        }

        return pending;
    }

    private double PendingFraction(GameDirectorComponent scheduler, FiredEvent fired, TimeSpan currTime)
    {
        var age = (currTime - fired.Time) * _event.EventSpeedup;
        if (age >= scheduler.PendingDuration)
            return 0;

        if (fired.Rule is not { } rule
            || TerminatingOrDeleted(rule)
            || !TryComp<StationEventComponent>(rule, out var stationEvent))
            return 1.0 - age / scheduler.PendingDuration;

        if (HasComp<DelayedStartRuleComponent>(rule) || stationEvent.EndTime is not { } end || currTime < end)
            return 1.0;

        return Math.Max(0.0, 1.0 - (currTime - end) * _event.EventSpeedup / scheduler.SettleTime);
    }

    private void LogEventEffects(GameDirectorComponent scheduler, ChaosMetrics chaos, TimeSpan currTime)
    {
        foreach (var fired in scheduler.FiredEvents)
        {
            var age = (currTime - fired.Time) * _event.EventSpeedup;
            if (fired.EffectLogged || age < scheduler.EffectDelay)
                continue;

            fired.EffectLogged = true;
            var changes = new List<string>();
            foreach (var (metric, value) in chaos.ChaosDict)
            {
                if (metric is ChaosMetric.Friend or ChaosMetric.Combat)
                    continue;

                var change = value - fired.Before.ChaosDict.GetValueOrDefault(metric);
                if (Math.Abs(change) >= 1 || fired.Chaos.ChaosDict.ContainsKey(metric))
                    changes.Add($"{metric} {change:+0;-0;0}");
            }

            LogMessage($"Effect of {fired.Event} after {age.TotalMinutes:F0}m: {string.Join(", ", changes)} (declared {fired.Chaos})", false);
        }
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
