using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Server.StationEvents.Metric;
using Content.Goobstation.Shared.StationEvents;
using Content.Server.Antag.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.Antag;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{
    [Dependency] private SharedRoleSystem _roles = default!;

    private static readonly ProtoId<DepartmentPrototype> SecurityDepartment = "Security";

    private const float MinAntagFit = 0.25f;

    private readonly record struct ThreatState(float Live, float Pending, float Target);

    private readonly record struct AntagPick(
        EntProtoId Proto,
        int Count,
        int MinCount,
        int MaxCount,
        float Threat,
        bool GhostRole,
        float Score);

    private readonly record struct AntagCountRange(int PreferredMin, int PreferredMax, int HardMin, int HardMax, bool GhostRole);

    private static float RuleThreat(AntagThreatPrototype table, string? rule)
    {
        return rule != null && table.Rules.TryGetValue(rule, out var threat) ? threat : table.Default;
    }

    private bool IsThreatRule(GameDirectorComponent scheduler, EntProtoId rule)
    {
        return _prototypeManager.Index(scheduler.ThreatTable).Rules.TryGetValue(rule, out var threat) && threat > 0;
    }

    private bool TryGetCurrentStory(GameDirectorComponent scheduler, [NotNullWhen(true)] out StoryPrototype? story)
    {
        story = null;
        return !string.IsNullOrEmpty(scheduler.CurrentStoryName.Id)
               && _prototypeManager.TryIndex(scheduler.CurrentStoryName, out story);
    }

    private ThreatState MeasureThreat(GameDirectorComponent scheduler, StoryBeatPrototype beat, PlayerCount count, TimeSpan now)
    {
        var live = LiveThreat(scheduler);
        var pending = PendingThreat(scheduler, now);
        var target = 0f;

        if (beat.Threat > 0 && !beat.HelpOnly && TryGetCurrentStory(scheduler, out var story))
        {
            target = scheduler.ThreatPerPlayer * story.Threat * beat.Threat * count.Players;
            target = MathF.Min(target, scheduler.BaseThreatCap + scheduler.ThreatCapPerSecurity * count.Staff(SecurityDepartment));
        }

        return new ThreatState(live, pending, target);
    }

    private float LiveThreat(GameDirectorComponent scheduler)
    {
        var table = _prototypeManager.Index(scheduler.ThreatTable);
        var counted = new HashSet<EntityUid>();
        var threat = 0f;

        var rules = EntityQueryEnumerator<AntagSelectionComponent, GameRuleComponent, MetaDataComponent>();
        while (rules.MoveNext(out _, out var selection, out _, out var meta))
        {
            var value = RuleThreat(table, meta.EntityPrototype?.ID);
            foreach (var (mind, _) in selection.AssignedMinds)
            {
                if (counted.Add(mind) && IsActiveAntag(mind))
                    threat += value;
            }
        }

        var minds = EntityQueryEnumerator<MindComponent>();
        while (minds.MoveNext(out var mind, out _))
        {
            if (!counted.Contains(mind) && _roles.MindIsAntagonist(mind) && IsActiveAntag(mind))
                threat += table.Convert;
        }

        return threat;
    }

    private bool IsActiveAntag(EntityUid mindId)
    {
        return TryComp<MindComponent>(mindId, out var mind)
               && mind.OwnedEntity is { } body
               && _mobState.IsAlive(body)
               && mind.UserId is { } user
               && _playerManager.TryGetSessionById(user, out var session)
               && session.Status == SessionStatus.InGame;
    }

    private float PendingThreat(GameDirectorComponent scheduler, TimeSpan now)
    {
        var pending = 0f;
        foreach (var purchase in scheduler.Purchases)
            pending += MissingAntags(scheduler, purchase, now) * purchase.Threat;

        return pending;
    }

    private int PendingGhostRoles(GameDirectorComponent scheduler, TimeSpan now)
    {
        var pending = 0;
        foreach (var purchase in scheduler.Purchases)
        {
            if (purchase.GhostRole)
                pending += MissingAntags(scheduler, purchase, now);
        }

        return pending;
    }

    private int MissingAntags(GameDirectorComponent scheduler, ThreatPurchase purchase, TimeSpan now)
    {
        if (TerminatingOrDeleted(purchase.Rule))
            return 0;

        var window = purchase.Roundstart ? scheduler.RoundstartPendingTime : scheduler.PendingThreatTime;
        if (!HasComp<DelayedStartRuleComponent>(purchase.Rule) && (now - purchase.Time) * _event.EventSpeedup > window)
            return 0;

        var assigned = TryComp<AntagSelectionComponent>(purchase.Rule, out var selection) ? selection.AssignedMinds.Count : 0;
        return Math.Max(0, purchase.Count - assigned);
    }

    private bool TryPurchaseThreat(Entity<GameDirectorComponent> ent,
        StoryBeatPrototype beat,
        PlayerCount count,
        ThreatState threat,
        TimeSpan now)
    {
        var scheduler = ent.Comp;
        var gap = threat.Target - threat.Live - threat.Pending;
        if (gap < scheduler.MinThreatPurchase
            || !TryGetCurrentStory(scheduler, out var story)
            || story.Midround.Count == 0)
            return false;

        var ghosts = count.Ghosts - PendingGhostRoles(scheduler, now);
        var roundDuration = GameTicker.RoundDuration();
        var picks = new List<AntagPick>();
        var seen = new HashSet<EntProtoId>();

        foreach (var entry in story.Midround.SelectMany(pool => _prototypeManager.Index(pool).Rules))
        {
            if (!seen.Add(entry.Id)
                || !_prototypeManager.TryIndex(entry.Id, out var proto)
                || !CanPurchaseMidround(proto, count, roundDuration))
                continue;

            var repeat = RepeatMultiplier(scheduler, entry.Id, now);
            if (EvaluateAntag(scheduler, proto, entry, gap, ghosts, true) is { } pick)
                picks.Add(pick with { Score = pick.Score * repeat });
        }

        if (picks.Count == 0)
            return false;

        var chosen = PickWeighted(picks, p => p.Score);
        var rule = StartPurchase(scheduler, chosen, false, now);
        if (!HasComp<StationEventComponent>(rule))
            GameTicker.StartGameRule(rule);

        scheduler.FiredEvents.Add(new FiredEvent
        {
            Event = chosen.Proto,
            Rule = rule,
            Time = now,
            Before = new ChaosMetrics(scheduler.CurrentChaos),
        });

        scheduler.LastSpotlight[SecurityDepartment] = now;
        EventsRunTotal.WithLabels(chosen.Proto, beat.ID).Inc();
        LogMessage($"Threat is {threat.Live:F1} live + {threat.Pending:F1} pending of {threat.Target:F1}. " +
                   $"Sending {chosen.Proto} x{chosen.Count} ({chosen.Count * chosen.Threat:F1} threat)");
        return true;
    }

    private bool CanPurchaseMidround(EntityPrototype proto, PlayerCount count, TimeSpan roundDuration)
    {
        if (proto.TryComp<GameRuleComponent>(out var gameRule, _factory) && gameRule.MinPlayers > count.InGame)
            return false;

        return !proto.TryComp<StationEventComponent>(out var stationEvent, _factory)
               || _event.CanRun(proto, stationEvent, count.InGame, roundDuration);
    }

    private AntagPick? EvaluateAntag(GameDirectorComponent scheduler,
        EntityPrototype proto,
        AntagPoolEntry entry,
        float budget,
        int ghosts,
        bool midround)
    {
        var threat = RuleThreat(_prototypeManager.Index(scheduler.ThreatTable), proto.ID);
        if (threat <= 0 || budget <= 0)
            return null;

        var range = CountRange(scheduler, proto, entry, midround);
        var hardMax = range.GhostRole ? Math.Min(range.HardMax, ghosts) : range.HardMax;
        if (hardMax < range.HardMin)
            return null;

        var preferredMax = Math.Min(range.PreferredMax, hardMax);
        var wanted = (int) MathF.Round(budget / threat);

        var best = 0;
        var bestScore = 0f;
        foreach (var option in new[] { Math.Clamp(wanted, range.PreferredMin, Math.Max(range.PreferredMin, preferredMax)), Math.Clamp(wanted, range.HardMin, hardMax) })
        {
            if (option > hardMax)
                continue;

            var inRange = option >= range.PreferredMin && option <= range.PreferredMax;
            var score = Fit(option * threat, budget) * (inRange ? 1f : scheduler.OutOfRangePenalty);
            if (score <= bestScore)
                continue;

            best = option;
            bestScore = score;
        }

        if (best == 0 || bestScore < MinAntagFit)
            return null;

        var weight = entry.Weight
                     ?? (proto.TryComp<StationEventComponent>(out var stationEvent, _factory) ? stationEvent.Weight : 1f);

        return new AntagPick(proto.ID, best, range.HardMin, hardMax, threat, range.GhostRole, weight * bestScore);
    }

    private static float Fit(float cost, float budget)
    {
        var miss = (cost - budget) / budget;
        return miss <= 0 ? 1f + 0.5f * miss : MathF.Max(0f, 1f - 1.5f * miss);
    }

    private AntagCountRange CountRange(GameDirectorComponent scheduler, EntityPrototype proto, AntagPoolEntry entry, bool midround)
    {
        if (!proto.TryComp<AntagSelectionComponent>(out var selection, _factory) || selection.Definitions.Count == 0)
            return new AntagCountRange(1, 1, 1, 1, false);

        var min = 0;
        var max = 0;
        var ghostRole = midround;
        foreach (var def in selection.Definitions)
        {
            var defMin = def.MinRange?.Min ?? def.Min;
            min += defMin;
            max += Math.Max(defMin, def.MaxRange?.Max ?? def.Max);
            ghostRole &= def.SpawnerPrototype != null
                         && (!def.PickPlayer || selection.SelectionTime == AntagSelectionTime.PrePlayerSpawn);
        }

        var hardMin = Math.Max(1, entry.Min ?? min);
        var hardMax = Math.Max(hardMin, entry.Max ?? (int) MathF.Ceiling(Math.Max(1, max) * scheduler.CountStretch));
        var preferredMin = Math.Clamp(min, hardMin, hardMax);
        var preferredMax = Math.Clamp(max, preferredMin, hardMax);
        return new AntagCountRange(preferredMin, preferredMax, hardMin, hardMax, ghostRole);
    }

    private EntityUid StartPurchase(GameDirectorComponent scheduler, AntagPick pick, bool roundstart, TimeSpan now)
    {
        var rule = GameTicker.AddGameRule(pick.Proto);
        if (TryComp<GameRuleComponent>(rule, out var gameRule))
            gameRule.CancelPresetOnTooFewPlayers = false;

        if (TryComp<AntagSelectionComponent>(rule, out var selection))
            ApplyAntagCount((rule, selection), pick.Count);

        scheduler.Purchases.Add(new ThreatPurchase
        {
            Rule = rule,
            Proto = pick.Proto,
            Threat = pick.Threat,
            Count = pick.Count,
            MinCount = pick.MinCount,
            MaxCount = pick.MaxCount,
            Time = now,
            Roundstart = roundstart,
            GhostRole = pick.GhostRole,
        });

        return rule;
    }

    private static void ApplyAntagCount(Entity<AntagSelectionComponent> rule, int count)
    {
        var defs = rule.Comp.Definitions;
        if (defs.Count == 0)
            return;

        var mins = defs.Select(d => d.MinRange?.Min ?? d.Min).ToArray();
        var maxes = defs.Select((d, i) => Math.Max(mins[i], d.MaxRange?.Max ?? d.Max)).ToArray();
        var scalable = Enumerable.Range(0, defs.Count).Where(i => maxes[i] > mins[i]).ToList();
        var baseTotal = mins.Sum();
        var counts = new int[defs.Count];

        if (scalable.Count == 0 || count < baseTotal)
        {
            var total = Math.Max(1, baseTotal);
            var assigned = 0;
            for (var i = 0; i < defs.Count; i++)
            {
                counts[i] = mins[i] * count / total;
                assigned += counts[i];
            }

            var order = Enumerable.Range(0, defs.Count).OrderByDescending(i => mins[i]).ToList();
            for (var i = 0; assigned < count; i++, assigned++)
                counts[order[i % order.Count]]++;
        }
        else
        {
            Array.Copy(mins, counts, defs.Count);
            var remaining = count - baseTotal;
            while (remaining > 0 && scalable.Any(i => counts[i] < maxes[i]))
            {
                foreach (var i in scalable)
                {
                    if (remaining == 0 || counts[i] >= maxes[i])
                        continue;

                    counts[i]++;
                    remaining--;
                }
            }

            for (var i = 0; remaining > 0; i++, remaining--)
                counts[scalable[i % scalable.Count]]++;
        }

        for (var i = 0; i < defs.Count; i++)
            defs[i] = defs[i] with { Min = counts[i], Max = counts[i], MinRange = null, MaxRange = null };
    }

    private bool CompatibleWith(EntProtoId rule, List<EntProtoId> chosen)
    {
        foreach (var other in chosen)
        {
            if (_prototypeManager.TryIndex<IncompatibleGameModesPrototype>(rule.Id, out var incompatible)
                && incompatible.Modes.Contains(other.Id)
                || _prototypeManager.TryIndex<IncompatibleGameModesPrototype>(other.Id, out var otherIncompatible)
                && otherIncompatible.Modes.Contains(rule.Id))
                return false;
        }

        return true;
    }
}
