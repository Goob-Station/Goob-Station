using System.Linq;
using Content.Goobstation.Common.StationEvents.SecretPlus;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Server.StationEvents.Metric;
using Content.Server.Antag.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{
    private static readonly ProtoId<EventTypePrototype> HostilesSpawnEventType = "HostilesSpawn";

    private void SetupEvents(Entity<GameDirectorComponent> ent)
    {
        var scheduler = ent.Comp;
        scheduler.PossibleEvents.Clear();

        if (TryComp<SelectedGameRulesComponent>(ent, out var selectedRules))
            SelectFromTable(scheduler, selectedRules);
        else
            SelectFromAllEvents(scheduler);

        LogMessage($"{scheduler.PossibleEvents.Count} possible events added: " +
                   string.Join(", ", scheduler.PossibleEvents.Select(e => e.StationEvent)), false);
    }

    private void SelectFromAllEvents(GameDirectorComponent scheduler)
    {
        foreach (var (proto, stationEvent) in _event.AllEvents())
        {
            if (stationEvent.IsSelectable)
                scheduler.PossibleEvents.Add(CreatePossibleEvent(proto, stationEvent));
        }
    }

    private void SelectFromTable(GameDirectorComponent scheduler, SelectedGameRulesComponent selectedRules)
    {
        var seen = new HashSet<EntProtoId>();
        foreach (var id in _entityTable.GetSpawns(selectedRules.ScheduledGameRules))
        {
            if (!seen.Add(id)
                || !_prototypeManager.TryIndex(id, out var proto)
                || proto.Abstract
                || !proto.TryComp<StationEventComponent>(out var stationEvent, _factory))
                continue;

            scheduler.PossibleEvents.Add(CreatePossibleEvent(proto, stationEvent));
        }
    }

    private PossibleEvent CreatePossibleEvent(EntityPrototype proto, StationEventComponent stationEvent)
    {
        var possibleEvent = new PossibleEvent(proto.ID,
            stationEvent.Chaos,
            stationEvent.Weight,
            stationEvent.Destruction,
            RequiredGhosts(proto),
            stationEvent.EventType);

        if (proto.TryComp<GameDirectorReinforcementComponent>(out var reinforcement, _factory))
        {
            possibleEvent.Reinforces = new HashSet<ProtoId<DepartmentPrototype>>(reinforcement.Departments);
            possibleEvent.UnderstaffedBelow = reinforcement.UnderstaffedBelow;
            possibleEvent.RequiredGhosts = Math.Max(possibleEvent.RequiredGhosts, reinforcement.RequiredGhosts);
        }

        if (proto.TryComp<GameDirectorRequiresStaffComponent>(out var requiresStaff, _factory))
            possibleEvent.RequiresStaff = new HashSet<ProtoId<DepartmentPrototype>>(requiresStaff.Departments);

        return possibleEvent;
    }

    private int RequiredGhosts(EntityPrototype proto)
    {
        if (!proto.TryComp<AntagSelectionComponent>(out var selection, _factory))
            return 0;

        var required = 0;
        foreach (var definition in selection.Definitions)
        {
            if (definition.PickPlayer || definition.SpawnerPrototype == null)
                continue;

            required += Math.Max(1, definition.MinRange?.Min ?? definition.Min);
        }

        return required;
    }

    private static float Score(ChaosMetrics chaos)
    {
        return chaos.ChaosDict.Values.Sum(v => (float) v * (float) v);
    }

    private PossibleEvent? ChooseEvent(Entity<GameDirectorComponent> ent,
        StoryBeatPrototype beat,
        ChaosMetrics goal,
        ChaosMetrics chaos,
        PlayerCount count)
    {
        var scheduler = ent.Comp;
        if (beat.AllowReinforcements && TryChooseReinforcement(scheduler, beat, count) is { } reinforcement)
            return reinforcement;

        var desiredChange = goal.ExclusiveSubtract(chaos);
        var noEventScore = Score(desiredChange);
        var now = _timing.CurTime;
        var roundDuration = GameTicker.RoundDuration();

        var ranked = new List<RankedEvent>();
        var calm = new List<RankedEvent>();

        foreach (var possibleEvent in scheduler.PossibleEvents)
        {
            if (possibleEvent.Reinforces.Count > 0
                || !CanDirectorRun(scheduler, beat, possibleEvent, count, roundDuration))
                continue;

            var repeat = RepeatMultiplier(scheduler, possibleEvent.StationEvent, now);

            if (possibleEvent.Chaos.Empty)
            {
                if (beat.AllowCalmEvents && possibleEvent.EventType != HostilesSpawnEventType)
                    calm.Add(new RankedEvent(possibleEvent, possibleEvent.Weight * repeat));
                continue;
            }

            var benefit = noEventScore - Score(desiredChange.ExclusiveSubtract(possibleEvent.Chaos));
            if (benefit <= 0)
                continue;

            ranked.Add(new RankedEvent(possibleEvent, benefit * repeat));
        }

        if (ranked.Count > 0)
        {
            var best = ranked.OrderByDescending(r => r.Score).Take(beat.RandomEventLimit).ToList();
            var picked = PickWeighted(best, r => r.Score * r.PossibleEvent.Weight);
            LogMessage($"Picked {picked.StationEvent} from best events (in sequence) {string.Join(", ", best.Select(r => r.PossibleEvent.StationEvent))}");
            return picked;
        }

        if (calm.Count == 0)
            return null;

        var calmPick = PickWeighted(calm, r => r.Score);
        LogMessage($"Picked calm event {calmPick.StationEvent}, nothing moves chaos towards the goal");
        return calmPick;
    }

    private PossibleEvent? TryChooseReinforcement(GameDirectorComponent scheduler, StoryBeatPrototype beat, PlayerCount count)
    {
        var now = _timing.CurTime;
        if (scheduler.LastReinforcement != TimeSpan.Zero
            && (now - scheduler.LastReinforcement) * _event.EventSpeedup < scheduler.ReinforcementCooldown)
            return null;

        var roundDuration = GameTicker.RoundDuration();
        var scale = PopulationScale(scheduler, count);
        var candidates = new List<RankedEvent>();
        var needed = new HashSet<ProtoId<DepartmentPrototype>>();

        foreach (var possibleEvent in scheduler.PossibleEvents)
        {
            if (possibleEvent.Reinforces.Count == 0)
                continue;

            var threshold = Math.Max(1, (int) MathF.Round(possibleEvent.UnderstaffedBelow * scale));
            var understaffed = possibleEvent.Reinforces.Where(d => count.Staff(d) < threshold).ToList();
            if (understaffed.Count == 0 || !CanDirectorRun(scheduler, beat, possibleEvent, count, roundDuration))
                continue;

            needed.UnionWith(understaffed);
            candidates.Add(new RankedEvent(possibleEvent,
                possibleEvent.Weight * understaffed.Count * RepeatMultiplier(scheduler, possibleEvent.StationEvent, now)));
        }

        if (candidates.Count == 0)
            return null;

        var picked = PickWeighted(candidates, r => r.Score);
        scheduler.LastReinforcement = now;
        LogMessage($"Understaffed: {string.Join(", ", needed.Select(d => $"{d} ({count.Staff(d)})"))}. Sending {picked.StationEvent}");
        return picked;
    }

    private bool CanDirectorRun(GameDirectorComponent scheduler,
        StoryBeatPrototype beat,
        PossibleEvent possibleEvent,
        PlayerCount count,
        TimeSpan roundDuration)
    {
        if (possibleEvent.Destruction > beat.MaxDestruction
            || possibleEvent.Destruction > scheduler.DestructionBudget
            || possibleEvent.RequiredGhosts > count.Ghosts
            || beat.BlockedEventTypes.Contains(possibleEvent.EventType)
            || !HasResponders(scheduler, possibleEvent.Chaos, count)
            || possibleEvent.RequiresStaff.Any(d => count.Staff(d) < scheduler.MinResponders))
            return false;

        var proto = _prototypeManager.Index(possibleEvent.StationEvent);
        return proto.TryComp<StationEventComponent>(out var stationEvent, _factory)
               && _event.CanRun(proto, stationEvent, count.InGame, roundDuration);
    }

    private static bool HasResponders(GameDirectorComponent scheduler, ChaosMetrics chaos, PlayerCount count)
    {
        foreach (var (metric, value) in chaos.ChaosDict)
        {
            if (value > 0
                && scheduler.Responders.TryGetValue(metric, out var department)
                && count.Staff(department) < scheduler.MinResponders)
                return false;
        }

        return true;
    }

    private float RepeatMultiplier(GameDirectorComponent scheduler, EntProtoId stationEvent, TimeSpan now)
    {
        var multiplier = 1f;
        foreach (var fired in scheduler.FiredEvents)
        {
            if (fired.Event != stationEvent)
                continue;

            var progress = (float) Math.Clamp((now - fired.Time) * _event.EventSpeedup / scheduler.RepeatWindow, 0.0, 1.0);
            multiplier *= scheduler.RepeatFactor + (1f - scheduler.RepeatFactor) * progress;
        }

        return multiplier;
    }

    private PossibleEvent PickWeighted(List<RankedEvent> events, Func<RankedEvent, float> weight)
    {
        var total = events.Sum(e => Math.Max(0f, weight(e)));
        if (total <= 0f)
            return _random.Pick(events).PossibleEvent;

        var roll = _random.NextFloat(total);
        foreach (var rankedEvent in events)
        {
            roll -= Math.Max(0f, weight(rankedEvent));
            if (roll <= 0f)
                return rankedEvent.PossibleEvent;
        }

        return events[^1].PossibleEvent;
    }
}
