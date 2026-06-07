// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Milon <milonpl.git@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Server.StationEvents.Metric;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Server.StationEvents;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

/// <summary>
///   A scheduler which tries to keep station chaos within a set bound over time with the most suitable
///   good or bad events to nudge it in the correct direction.
/// </summary>
[UsedImplicitly]
public sealed partial class GameDirectorSystem : GameRuleSystem<GameDirectorComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    private ISawmill _sawmill = default!;
    private int _gameDirectorDebugPlayerCount;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _log.GetSawmill("game_rule");
        SubscribeLocalEvent<GameDirectorComponent, EntityUnpausedEvent>(OnUnpaused);
        Subs.CVar(_configManager, GoobCVars.GameDirectorDebugPlayerCount, x => _gameDirectorDebugPlayerCount = x, true);
    }

    private static void OnUnpaused(EntityUid uid, GameDirectorComponent scheduler, ref EntityUnpausedEvent args)
    {
        scheduler.BeatStart += args.PausedTime;
        scheduler.TimeNextEvent += args.PausedTime;
    }

    protected override void Added(EntityUid uid, GameDirectorComponent scheduler, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        _sawmill.Info($"Game Director Spawned at {uid}");
        ResetMetrics();
        TrySpawnRoundstartAntags(scheduler); // Roundstart antags need to be selected in the lobby
        if(TryComp<SelectedGameRulesComponent>(uid,out var selectedRules))
            SetupEvents(scheduler, CountActivePlayers(), selectedRules);
        else
            SetupEvents(scheduler, CountActivePlayers());
    }

    /// <summary>
    ///   Decide what event to run next
    /// </summary>
    protected override void ActiveTick(EntityUid uid, GameDirectorComponent scheduler, GameRuleComponent gameRule, float frameTime)
    {
        var currTime = _timing.CurTime;
        if (currTime < scheduler.TimeNextEvent)
            return;

        var chaos = CalculateChaos(uid);
        scheduler.CurrentChaos = chaos;
        LogMessage($"Chaos is: {chaos}");

        if (scheduler.Stories is not { Length: > 0 })
        {
            // No stories (e.g. dummy game rule for printing metrics), end game rule now
            GameTicker.EndGameRule(uid, gameRule);
            return;
        }
        // Decide what story beat to work with (which sets chaos goals)
        var count = CountActivePlayers();
        ActivePlayers.Set(count.Players);
        ActiveGhosts.Set(count.Ghosts);

        var beat = DetermineNextBeat(scheduler, chaos, count);

        // This is the first event, add an automatic delay
        if (scheduler.TimeNextEvent == TimeSpan.Zero)
        {
            var minimumTimeUntilFirstEvent = _configManager.GetCVar(GoobCVars.MinimumTimeUntilFirstEvent) / _event.EventSpeedup;
            scheduler.TimeNextEvent = _timing.CurTime + TimeSpan.FromSeconds(minimumTimeUntilFirstEvent);
            LogMessage($"Started, first event in {minimumTimeUntilFirstEvent} seconds");
            return;
        }

        RankedEvent? chosenEvent = null;
        // Pick the best events (which move the station towards the chaos desired by the beat)
        var bestEvents = ChooseEvents(scheduler, beat, chaos, count);

        // Run the best event here, if we have any to pick from.
        if (bestEvents.Count > 0)
        {
            // Sorts the possible events and then picks semi-randomly.
            // when beat.RandomEventLimit is 1 it's always the "best" event picked. Higher values
            // allow more events to be randomly selected.
            chosenEvent = SelectBest(bestEvents, beat.RandomEventLimit);

            _event.RunNamedEvent(chosenEvent.PossibleEvent.StationEvent);
        }

        if (chosenEvent != null)
        {
            EventsRunTotal.WithLabels(chosenEvent.PossibleEvent.StationEvent).Inc();
            // 2 - 6 minutes until the next event is considered, can vary per beat
            scheduler.TimeNextEvent = currTime + TimeSpan.FromSeconds(_random.NextFloat(beat.EventDelayMin, beat.EventDelayMax) / _event.EventSpeedup);
        }
        else
        {
            // No events were run. Consider again in 30 seconds (current beat or chaos might change)
            LogMessage($"Chaos is: {chaos} (No events ran)", false);
            scheduler.TimeNextEvent = currTime + TimeSpan.FromSeconds(30f);
        }
    }

    private void LogMessage(string message, bool showChat=true)
    {
        // TODO: LogMessage strings all require localization.
        _adminLogger.Add(LogType.GameDirector, showChat?LogImpact.Medium:LogImpact.High, $"{message}");
        if (showChat)
            _chat.SendAdminAnnouncement("GameDirector " + message);
    }

    public ChaosMetrics CalculateChaos(EntityUid uid)
    {
        // Send an event to chaos metric components on the Game Director's entity.
        var calcEvent = new CalculateChaosEvent(new ChaosMetrics());
        RaiseLocalEvent(uid, ref calcEvent);

        var metrics = calcEvent.Metrics;

        // Calculated metrics
        metrics.ChaosDict[ChaosMetric.Combat] = metrics.ChaosDict.GetValueOrDefault(ChaosMetric.Friend) +
                                                metrics.ChaosDict.GetValueOrDefault(ChaosMetric.Hostile);
        return calcEvent.Metrics;
    }
}
