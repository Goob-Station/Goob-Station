// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Shared.StationEvents;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
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
    [Dependency] private readonly AntagSelectionSystem _antagSelection = default!;
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
    [Dependency] private readonly GameTicker _ticker = default!;

    // cvars
    private float _minimumTimeUntilFirstEvent;
    private float _roundstartChaosScoreMultiplier;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _log.GetSawmill("secret_plus");

        Subs.CVar(_cfg, GoobCVars.MinimumTimeUntilFirstEvent, value => _minimumTimeUntilFirstEvent = value, true);
        Subs.CVar(_cfg, GoobCVars.RoundstartChaosScoreMultiplier, value => _roundstartChaosScoreMultiplier = value, true);
    }

    protected override void Added(EntityUid uid, SecretPlusComponent scheduler, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        var totalPlayers = GetTotalPlayerCount(_playerManager.Sessions);
        // set up starting chaos score
        scheduler.ChaosScore =
            -_random.NextFloat(scheduler.MinStartingChaos * totalPlayers, scheduler.MaxStartingChaos * totalPlayers) *
            _roundstartChaosScoreMultiplier;

        // roll midroundchaos generation variation
        var roll = _random.NextFloat();
        roll = MathF.Pow(roll, scheduler.ChaosChangeVariationExponent);
        // 50% chance to bias towards either higher chaos or lower chaos
        scheduler.ChaosChangeVariation = MathHelper.Lerp(1f,
                                                         _random.Prob(0.5f) ? scheduler.ChaosChangeVariationMin : scheduler.ChaosChangeVariationMax,
                                                         roll);
        LogMessage($"Using chaos change multiplier of {scheduler.ChaosChangeVariation}");

        TrySpawnRoundstartAntags((uid, scheduler)); // Roundstart antags need to be selected in the lobby
        if(TryComp<SelectedGameRulesComponent>(uid, out var selectedRules))
            SetupEvents((uid, scheduler), CountActivePlayers(), selectedRules);
        else
            SetupEvents((uid, scheduler), CountActivePlayers());
    }

    /// <summary>
    ///   Build a list of events to use for the entire story
    /// </summary>
    private void SetupEvents(Entity<SecretPlusComponent> scheduler, PlayerCount count, SelectedGameRulesComponent? selectedRules = null)
    {
        scheduler.Comp.SelectedEvents.Clear();

        if (selectedRules != null)
        {
            SelectFromTable(scheduler, count, selectedRules);
        }
        else
        {
            SelectFromAllEvents(scheduler, count);
        }
    }

    private void SelectFromAllEvents(Entity<SecretPlusComponent> scheduler, PlayerCount count)
    {
        foreach (var proto in _ticker.GetAllGameRulePrototypes())
        {
            if (!proto.TryGetComponent<GameRuleComponent>(out var gameRule, _factory)
                || !proto.TryGetComponent<StationEventComponent>(out var stationEvent, _factory)
            )
                continue;

            if (scheduler.Comp.DisallowedEvents.Contains(stationEvent.EventType)
                || (!scheduler.Comp.IgnoreTimings
                    && !_event.CanRun(proto, stationEvent, count.Players, _ticker.RoundDuration(), 1f / GetRamping(scheduler)))
            )
                continue;

            scheduler.Comp.SelectedEvents.Add(new SelectedEvent(proto, gameRule, stationEvent));
        }
    }

    private void SelectFromTable(Entity<SecretPlusComponent> scheduler, PlayerCount count, SelectedGameRulesComponent? selectedRules)
    {
        if (selectedRules == null)
            return;

        var available = _event.AvailableEvents(scheduler.Comp.IgnoreTimings,
                            scheduler.Comp.IgnoreTimings ? int.MaxValue : null,
                            scheduler.Comp.IgnoreTimings ? TimeSpan.MaxValue : null,
                            1f / GetRamping(scheduler));

        if (!_event.TryBuildLimitedEvents(selectedRules.ScheduledGameRules, available, out var possibleEvents))
            return;

        foreach (var entry in possibleEvents)
        {
            var proto = entry.Key;
            var stationEvent = entry.Value;
            if (!proto.TryGetComponent<GameRuleComponent>(out var gameRule, _factory))
                continue;

            if (scheduler.Comp.DisallowedEvents.Contains(stationEvent.EventType))
                continue;

            scheduler.Comp.SelectedEvents.Add(new SelectedEvent(proto, gameRule, stationEvent));
        }
    }

    /// <summary>
    ///   Decide what event to run next
    /// </summary>
    protected override void ActiveTick(EntityUid uid, SecretPlusComponent scheduler, GameRuleComponent gameRule, float frameTime)
    {
        var count = CountActivePlayers();
        var ramp = GetRamping((uid, scheduler));
        var speedup = _event.EventSpeedup;
        var mult = scheduler.ChaosChangeVariation;

        scheduler.ChaosScore += count.Players * scheduler.LivingChaosChange * frameTime * ramp * speedup * mult;
        scheduler.ChaosScore += count.Ghosts * scheduler.DeadChaosChange * frameTime * speedup * mult;

        var currTime = _timing.CurTime;
        if (currTime < scheduler.TimeNextEvent)
            return;

        // This is the first event, add an automatic delay
        if (scheduler.TimeNextEvent == TimeSpan.Zero)
        {
            var time = _minimumTimeUntilFirstEvent / speedup;
            scheduler.TimeNextEvent = _timing.CurTime + TimeSpan.FromSeconds(time);
            LogMessage($"Started, first event in {time} seconds");
            return;
        }

        TimeSpan amt = TimeSpan.FromSeconds(_random.NextDouble(scheduler.EventIntervalMin.TotalSeconds, scheduler.EventIntervalMax.TotalSeconds) / ramp / speedup);
        scheduler.TimeNextEvent = currTime + amt;
                                                                          // generally more useful than curTime
        LogMessage($"Chaos score: {scheduler.ChaosScore}, Next event at: {_ticker.RoundDuration() + amt} (ramping {ramp})");

        if(TryComp<SelectedGameRulesComponent>(uid, out var selectedRules))
            SetupEvents((uid, scheduler), count, selectedRules);
        else
            SetupEvents((uid, scheduler), count);

        var selectedEvent = ChooseEvent((uid, scheduler));
        if (selectedEvent != null)
            StartRule((uid, scheduler), selectedEvent.Proto.ID, false);
        else
            LogMessage($"No runnable events");

    }

    private ProtoId<TagPrototype> _loneSpawnTag = "LoneRunRule";

    /// <summary>
    /// Tries to spawn roundstart antags at the beginning of the round.
    /// </summary>
    private void TrySpawnRoundstartAntags(Entity<SecretPlusComponent> scheduler)
    {
        if (scheduler.Comp.NoRoundstartAntags)
            return;

        var primaryWeightList = _prototypeManager.Index(scheduler.Comp.PrimaryAntagsWeightTable);
        var weightList = _prototypeManager.Index(scheduler.Comp.RoundStartAntagsWeightTable);

        var count = GetTotalPlayerCount(_playerManager.Sessions);

        LogMessage($"Trying to run roundstart rules, total player count: {count}", false);

        var weights = weightList.Weights.ToDictionary();
        var primaryWeights = primaryWeightList.Weights.ToDictionary();
        int maxIters = 50, i = 0; // in case something dumb is tried
        var origChaos = scheduler.Comp.ChaosScore;
        while (scheduler.Comp.ChaosScore < 0 && i < maxIters)
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

            var chaosScore = GetChaosScore(entProto, ruleComp);

            if (chaosScore == null)
            {
                Log.Error($"Tried running roundstart event {entProto.ID}, but chaos score was null");
                continue;
            }
                             // negative
            var pickProb = (-scheduler.Comp.ChaosScore) / chaosScore.Value;
            if (i == 1)
                pickProb *= scheduler.Comp.PrimaryAntagChaosBias;
            pickProb = MathF.Min(1f, pickProb); // to shut up debug
            if (!_random.Prob(pickProb)) // have a chance to re-pick if we have low chaos budget left compared to this
                continue;

            // for admeme presets
            if (!scheduler.Comp.IgnoreIncompatible)
            {
                weights.Remove(pick);

                if (_prototypeManager.TryIndex(pick, out IncompatibleGameModesPrototype? incompModes))
                    weights = weights.Where(w => !incompModes.Modes.Contains(w.Key)).ToDictionary();
            }

            IndexAndStartGameMode(pick, entProto, ruleComp);

            if (weights.Count == 0
                || (!scheduler.Comp.IgnoreIncompatible
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

                                                // pick less antags if we have less chaos left
            var effPlayers = (int)MathF.Round(count * scheduler.Comp.ChaosScore / origChaos);
            LogMessage($"Roundstart rule chosen: {pick} with score {GetChaosScore(pickProto, ruleComp, effPlayers)}");
            StartRule(scheduler, pick, false, effPlayers);
        }
    }

    /// <summary>
    ///   Adds and, optionally, starts a gamerule while respecting rules with variable chaos.
    /// </summary>
    private void StartRule(Entity<SecretPlusComponent> scheduler, string rule, bool doStart = true, int? players = null)
    {
        var ruleUid = _ticker.AddGameRule(rule);

        scheduler.Comp.ChaosScore += GetChaosScore(ruleUid, players)!.Value;

        // if we hijack playercount, also hijack how many antags we pick
        if (players != null && TryComp<AntagSelectionComponent>(ruleUid, out var selection))
        {
            // i love C#
            for (var i = 0; i < selection.Definitions.Count; i++)
            {
                var def = selection.Definitions[i];
                def.Min = def.Max = _antagSelection.GetTargetAntagCount((ruleUid, selection), players, def);
                selection.Definitions[i] = def;
            }
        }

        if (doStart)
            _ticker.StartGameRule(ruleUid);
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
                // don't count bodyless ghosts aka say roundstart spectators
                // not reliable but works
                else if (TryComp<GhostComponent>(player.AttachedEntity, out var ghost) && ghost.CanReturnToBody)
                    count.Ghosts += 1;
            }
        }

        count.Players += _event.PlayerCountBias;

        return count;
    }

    public float? GetChaosScore(Entity<GameRuleComponent?> rule, int? players = null)
    {
        if (!Resolve(rule, ref rule.Comp))
            return null;

        if (TryComp<AntagSelectionComponent>(rule, out var selection))
        {
            var any = false;
            var score = 0f;

            foreach (var def in selection.Definitions)
            {
                if (def.ChaosScore == null)
                    continue;

                any = true;
                var count = _antagSelection.GetTargetAntagCount((rule, selection), players ?? GetTotalPlayerCount(_playerManager.Sessions), def);
                score += def.ChaosScore.Value * count;
            }

            if (any)
                return score;
        }

        return rule.Comp.ChaosScore;
    }

    public float? GetChaosScore(EntityPrototype ruleProto, GameRuleComponent? ruleComp, int? players = null)
    {
        if (ruleComp == null && !ruleProto.TryGetComponent<GameRuleComponent>(out ruleComp, _factory))
            return null;

        if (ruleProto.TryGetComponent<AntagSelectionComponent>(out var selection, _factory))
        {
            var any = false;
            var score = 0f;

            foreach (var def in selection.Definitions)
            {
                if (def.ChaosScore == null)
                    continue;

                any = true;
                var count = _antagSelection.GetTargetAntagCount((EntityUid.Invalid, selection), players ?? GetTotalPlayerCount(_playerManager.Sessions), def);
                score += def.ChaosScore.Value * count;
            }

            if (any)
                return score;
        }

        return ruleComp.ChaosScore;
    }

    /// <summary>
    ///   Count all the players on the server.
    /// </summary>
    public int GetTotalPlayerCount(IList<ICommonSession> pool)
    {
        return _antagSelection.GetTotalPlayerCount(pool) + _event.PlayerCountBias;
    }

    public float GetRamping(Entity<SecretPlusComponent> scheduler)
    {
        var curTime = _ticker.RoundDuration();
        return 1f + (float)curTime.TotalSeconds * scheduler.Comp.SpeedRamping * _event.EventSpeedup;
    }

    /// <summary>
    ///   Picks an event based on current chaos score, events' chaos scores and weights.
    /// </summary>
    private SelectedEvent? ChooseEvent(Entity<SecretPlusComponent> scheduler)
    {
        var possible = scheduler.Comp.SelectedEvents;
        Dictionary<SelectedEvent, float> weights = new();

        foreach (var ev in possible)
        {
            if (ev.EvComp == null)
                continue;

            var chaosScore = GetChaosScore(ev.Proto, ev.RuleComp);
            if (chaosScore == null)
            {
                Log.Error($"Tried running event {ev.Proto.ID}, but chaos score was null");
                continue;
            }

            var weight = chaosScore.Value;
            bool negative = weight < 0f;
            weight = MathF.Abs(weight);
            weight = MathF.Pow(weight, scheduler.Comp.ChaosExponent);
            if (negative) weight = -weight;
            weight += scheduler.Comp.ChaosOffset; // offset negative-chaos events upwards too else they never happen
            weight += weight < 0f ? -scheduler.Comp.ChaosThreshold : scheduler.Comp.ChaosThreshold; // make sure it's not in (-1, 1) to not get absurdly low event probabilities
            var delta = ChaosDelta(-scheduler.Comp.ChaosScore, weight, scheduler.Comp.ChaosMatching, scheduler.Comp.ChaosThreshold * scheduler.Comp.ChaosThreshold);
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
