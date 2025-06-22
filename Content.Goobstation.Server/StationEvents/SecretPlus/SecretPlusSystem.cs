// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Shared.StationEvents;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Server.StationEvents;
using Content.Server.StationEvents.Components;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.Random.Helpers;
using Content.Shared.Tag;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.StationEvents.SecretPlus;

/// <summary>
///   Temporary class for caching data.
/// </summary>
public sealed class SelectedEvent
{
    /// <summary>
    ///   The station event prototype
    /// </summary>
    public readonly EntityPrototype Proto;
    public readonly GameRuleComponent RuleComp;
    public readonly StationEventComponent? EvComp;

    public SelectedEvent(EntityPrototype proto, GameRuleComponent ruleComp, StationEventComponent? evComp = null)
    {
        Proto = proto;
        RuleComp = ruleComp;
        EvComp = evComp;
    }
}
public sealed class PlayerCount
{
    public int Players;
    public int Ghosts;
}

/// <summary>
///   A scheduler which keeps track of a 'chaos score' which it tries to get closer to 0.
/// </summary>
[UsedImplicitly]
public sealed class SecretPlusSystem : GameRuleSystem<SecretPlusComponent>
{
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    // cvars
    private float _minimumTimeUntilFirstEvent;
    private int _debugPlayerBias;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _log.GetSawmill("secret_plus");

        SubscribeLocalEvent<SecretPlusComponent, EntityUnpausedEvent>(OnUnpaused);

        Subs.CVar(_cfg, GoobCVars.MinimumTimeUntilFirstEvent, value => _minimumTimeUntilFirstEvent = value, true);
        Subs.CVar(_cfg, GoobCVars.SecretPlusPlayerBias, value => _debugPlayerBias = value, true);
    }

    private void OnUnpaused(EntityUid uid, SecretPlusComponent component, ref EntityUnpausedEvent args)
    {
        component.TimeNextEvent += args.PausedTime;
    }

    protected override void Added(EntityUid uid, SecretPlusComponent scheduler, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        var totalPlayers = GetTotalPlayerCount(_playerManager.Sessions);
        // set up starting chaos score
        scheduler.ChaosScore = -_random.NextFloat(scheduler.MinStartingChaos * totalPlayers, scheduler.MaxStartingChaos * totalPlayers);

        TrySpawnRoundstartAntags(scheduler); // Roundstart antags need to be selected in the lobby
        if(TryComp<SelectedGameRulesComponent>(uid, out var selectedRules))
            SetupEvents(scheduler, CountActivePlayers(), selectedRules);
        else
            SetupEvents(scheduler, CountActivePlayers());
    }

    /// <summary>
    ///   Build a list of events to use for the entire story
    /// </summary>
    private void SetupEvents(SecretPlusComponent scheduler, PlayerCount count, SelectedGameRulesComponent? selectedRules = null)
    {
        scheduler.SelectedEvents.Clear();

        if (selectedRules != null)
        {
            SelectFromTable(scheduler, count, selectedRules);
        }
        else
        {
            SelectFromAllEvents(scheduler, count);
        }
        LogMessage($"All possible events added");
    }

    private void SelectFromAllEvents(SecretPlusComponent scheduler, PlayerCount count)
    {
        foreach (var proto in GameTicker.GetAllGameRulePrototypes())
        {
            if (!proto.TryGetComponent<GameRuleComponent>(out var gameRule, _factory)
                || !proto.TryGetComponent<StationEventComponent>(out var stationEvent, _factory)
            )
                continue;

            if (scheduler.DisallowedEvents.Contains(stationEvent.EventType) || (!scheduler.IgnoreTimings && !_event.CanRun(proto, stationEvent, count.Players, _timing.CurTime)))
                continue;

            scheduler.SelectedEvents.Add(new SelectedEvent(proto, gameRule, stationEvent));
        }
    }

    private void SelectFromTable(SecretPlusComponent scheduler, PlayerCount count, SelectedGameRulesComponent? selectedRules)
    {
        if (selectedRules == null)
            return;

        if(!_event.TryBuildLimitedEvents(selectedRules.ScheduledGameRules,
            _event.AvailableEvents(scheduler.IgnoreTimings, scheduler.IgnoreTimings ? int.MaxValue : null, scheduler.IgnoreTimings ? TimeSpan.MaxValue : null),
            out var possibleEvents))
            return;

        foreach (var entry in possibleEvents)
        {
            var proto = entry.Key;
            var stationEvent = entry.Value;
            if (!proto.TryGetComponent<GameRuleComponent>(out var gameRule, _factory))
                continue;

            if (scheduler.DisallowedEvents.Contains(stationEvent.EventType) || (!scheduler.IgnoreTimings && !_event.CanRun(proto, stationEvent, count.Players, _timing.CurTime)))
                continue;

            scheduler.SelectedEvents.Add(new SelectedEvent(proto, gameRule, stationEvent));
        }
    }

    /// <summary>
    ///   Decide what event to run next
    /// </summary>
    protected override void ActiveTick(EntityUid uid, SecretPlusComponent scheduler, GameRuleComponent gameRule, float frameTime)
    {
        var count = CountActivePlayers();

        scheduler.ChaosScore += count.Players * scheduler.LivingChaosChange * frameTime;
        scheduler.ChaosScore += count.Ghosts * scheduler.DeadChaosChange * frameTime;

        var currTime = _timing.CurTime;
        if (currTime < scheduler.TimeNextEvent)
            return;

        // This is the first event, add an automatic delay
        if (scheduler.TimeNextEvent == TimeSpan.Zero)
        {
            scheduler.TimeNextEvent = _timing.CurTime + TimeSpan.FromSeconds(_minimumTimeUntilFirstEvent);
            LogMessage($"Started, first event in {_minimumTimeUntilFirstEvent} seconds");
            return;
        }

        TimeSpan amt = TimeSpan.FromSeconds(_random.NextDouble(scheduler.EventIntervalMin.TotalSeconds, scheduler.EventIntervalMax.TotalSeconds));
        scheduler.TimeNextEvent = currTime + amt;
        LogMessage($"Chaos score: {scheduler.ChaosScore}, Next event at: {scheduler.TimeNextEvent}");

        if(TryComp<SelectedGameRulesComponent>(uid, out var selectedRules))
            SetupEvents(scheduler, count, selectedRules);
        else
            SetupEvents(scheduler, count);

        var selectedEvent = ChooseEvent(scheduler);
        if (selectedEvent != null)
        {
            _event.RunNamedEvent(selectedEvent.Proto.ID);
            // nullcheck done in ChooseEvent
            scheduler.ChaosScore += selectedEvent.RuleComp.ChaosScore!.Value;
        }
        else {
            LogMessage($"No runnable events");
        }

    }

    private ProtoId<TagPrototype> _loneSpawnTag = "LoneRunRule";

    /// <summary>
    /// Tries to spawn roundstart antags at the beginning of the round.
    /// </summary>
    private void TrySpawnRoundstartAntags(SecretPlusComponent scheduler)
    {
        if (scheduler.NoRoundstartAntags)
            return;

        var primaryWeightList = _prototypeManager.Index(scheduler.PrimaryAntagsWeightTable);
        var weightList = _prototypeManager.Index(scheduler.RoundStartAntagsWeightTable);

        var count = GetTotalPlayerCount(_playerManager.Sessions);

        LogMessage($"Trying to run roundstart rules, total player count: {count}", false);

        var weights = weightList.Weights.ToDictionary();
        var primaryWeights = primaryWeightList.Weights.ToDictionary();
        int maxIters = 50, i = 0; // in case something dumb is tried
        while (scheduler.ChaosScore < 0 && i < maxIters)
        {
            i++;

            // on first iter pick a primary antag
            var pick = _random.Pick(i == 1 ? primaryWeights : weights);

            // on lowpop this may still go no likey even for the primary antag pick and pick thief or something, intended
            GameRuleComponent? ruleComp = null;
            if (!_prototypeManager.TryIndex(pick, out var entProto)
                || !entProto.TryGetComponent<GameRuleComponent>(out ruleComp, _factory)
            )
                continue;

            if (ruleComp.ChaosScore == null)
            {
                Log.Error($"Tried running roundstart event {entProto.ID}, but chaos score was null");
                continue;
            }
                             // negative
            var pickProb = (-scheduler.ChaosScore) / ruleComp.ChaosScore.Value;
            if (i == 1)
                pickProb *= scheduler.PrimaryAntagChaosBias;
            pickProb = MathF.Min(1f, pickProb); // to shut up debug
            if (!_random.Prob(pickProb)) // have a chance to re-pick if we have low chaos budget left compared to this
                continue;

            // for admeme presets
            if (!scheduler.IgnoreIncompatible)
            {
                weights.Remove(pick);

                if (_prototypeManager.TryIndex(pick, out IncompatibleGameModesPrototype? incompModes))
                    weights = weights.Where(w => !incompModes.Modes.Contains(w.Key)).ToDictionary();
            }

            IndexAndStartGameMode(pick, entProto, ruleComp);

            if (weights.Count == 0
                || (!scheduler.IgnoreIncompatible
                    && entProto.TryGetComponent<TagComponent>(out var tagComp, _factory)
                    && _tag.HasTag(tagComp, _loneSpawnTag)
                )
            )
                return;
        }

        return;

        void IndexAndStartGameMode(string pick, EntityPrototype? pickProto, GameRuleComponent? ruleComp)
        {
            if(pickProto == null
               || ruleComp == null
               || ruleComp.MinPlayers > count
            )
                return;

            LogMessage($"Roundstart rule chosen: {pick}");
            GameTicker.AddGameRule(pick);
            scheduler.ChaosScore += ruleComp.ChaosScore!.Value;
        }
    }

    /// <summary>
    ///   Count the active players and ghosts on the server to determine how chaos changes.
    /// </summary>
    private PlayerCount CountActivePlayers()
    {
        var allPlayers = _playerManager.Sessions.ToList();
        var count = new PlayerCount();
        foreach (var player in allPlayers)
        {
            // TODO: A
            if (player.AttachedEntity != null)
            {
                // TODO: Consider a custom component here instead of HumanoidAppearanceComponent to represent
                //        "significant enough to count as a whole player"
                if (HasComp<HumanoidAppearanceComponent>(player.AttachedEntity))
                    count.Players += 1;
                else if (HasComp<GhostComponent>(player.AttachedEntity))
                    count.Ghosts += 1;
            }
        }

        count.Players += _debugPlayerBias;

        return count;
    }

    /// <summary>
    ///   Count all the players on the server.
    /// </summary>
    public int GetTotalPlayerCount(IList<ICommonSession> pool)
    {
        var count = 0;
        foreach (var session in pool)
        {
            if (session.Status is SessionStatus.Disconnected or SessionStatus.Zombie)
                continue;

            count++;
        }

        return count + _debugPlayerBias;
    }

    /// <summary>
    ///   Picks an event based on current chaos score, events' chaos scores and weights.
    /// </summary>
    private SelectedEvent? ChooseEvent(SecretPlusComponent scheduler)
    {
        var possible = scheduler.SelectedEvents;
        Dictionary<SelectedEvent, float> weights = new();

        foreach (var ev in possible)
        {
            if (ev.EvComp == null)
                continue;

            if (ev.RuleComp.ChaosScore == null)
            {
                Log.Error($"Tried running event {ev.Proto.ID}, but chaos score was null");
                continue;
            }

            var weight = ev.RuleComp.ChaosScore.Value;
            bool negative = weight < 0f;
            weight = MathF.Abs(weight);
            weight = MathF.Pow(weight, scheduler.ChaosExponent);
            if (negative) weight = -weight;
            weight += scheduler.ChaosOffset; // offset negative-chaos events upwards too else they never happen
            weight += weight < 0f ? -scheduler.ChaosThreshold : scheduler.ChaosThreshold; // make sure it's not in (-1, 1) to not get absurdly low event probabilities
            var delta = ChaosDelta(-scheduler.ChaosScore, weight, scheduler.ChaosMatching, scheduler.ChaosThreshold);
            weights[ev] = ev.EvComp.Weight / (delta + 1f);
        }

        return weights.Count == 0 ? null : _random.Pick(weights);
    }

    private float ChaosDelta(float chaos1, float chaos2, float logBase, float differentSignMultiplier)
    {
        float ratio = chaos2 / chaos1;
        if (ratio < 0f) ratio = MathF.Abs(chaos2 * chaos1 / differentSignMultiplier);
        return MathF.Abs(MathF.Log(ratio, logBase));
    }

    private void LogMessage(string message, bool showChat=true)
    {
        // TODO: LogMessage strings all require localization.
        _adminLogger.Add(LogType.SecretPlus, showChat?LogImpact.Medium:LogImpact.High, $"{message}");
        if (showChat)
            _chat.SendAdminAnnouncement("SecretPlus " + message);

    }
}
