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
    private static readonly List<DirectorEventGroupPrototype> NoGroups = new();

    private Dictionary<EntProtoId, List<DirectorEventGroupPrototype>>? _eventGroups;

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<DirectorEventGroupPrototype>())
            _eventGroups = null;
    }

    private void SetupEvents(Entity<GameDirectorComponent> ent, StoryPrototype story)
    {
        var scheduler = ent.Comp;
        scheduler.PossibleEvents.Clear();

        if (story.Events is { } events)
            SelectFromTable(scheduler, _entityTable.GetSpawns(_prototypeManager.Index(events)));
        else if (TryComp<SelectedGameRulesComponent>(ent, out var selectedRules))
            SelectFromTable(scheduler, _entityTable.GetSpawns(selectedRules.ScheduledGameRules));
        else
            SelectFromAllEvents(scheduler);

        LogMessage($"{scheduler.PossibleEvents.Count} possible events added: " +
                   string.Join(", ", scheduler.PossibleEvents.Select(e => e.StationEvent)), false);
    }

    private void SelectFromAllEvents(GameDirectorComponent scheduler)
    {
        foreach (var (proto, stationEvent) in _event.AllEvents())
        {
            if (stationEvent.IsSelectable && !IsThreatRule(scheduler, proto.ID))
                scheduler.PossibleEvents.Add(CreatePossibleEvent(proto, stationEvent));
        }
    }

    private void SelectFromTable(GameDirectorComponent scheduler, IEnumerable<EntProtoId> spawns)
    {
        var seen = new HashSet<EntProtoId>();
        foreach (var id in spawns)
        {
            if (!seen.Add(id)
                || IsThreatRule(scheduler, id)
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

    private float Benefit(GameDirectorComponent scheduler, ChaosMetrics desired, ChaosMetrics change, float popScale)
    {
        var benefit = 0f;
        foreach (var (metric, want) in desired.ChaosDict)
        {
            var scale = MetricScale(scheduler, metric, popScale);
            var wanted = (float) want / scale;
            var residual = wanted - (float) change.ChaosDict.GetValueOrDefault(metric) / scale;
            var penalty = residual != 0f && MathF.Sign(residual) != MathF.Sign(wanted) ? scheduler.OvershootPenalty : 1f;
            benefit += wanted * wanted - penalty * residual * residual;
        }

        return benefit;
    }

    private static float MetricScale(GameDirectorComponent scheduler, ChaosMetric metric, float popScale)
    {
        var scale = scheduler.MetricScales.GetValueOrDefault(metric, 100f);
        if (scheduler.PopulationScaledMetrics.Contains(metric))
            scale *= popScale;

        return MathF.Max(1f, scale);
    }

    private PossibleEvent? ChooseEvent(Entity<GameDirectorComponent> ent,
        StoryBeatPrototype beat,
        ChaosMetrics goal,
        ChaosMetrics chaos,
        PlayerCount count,
        bool relaxing)
    {
        var scheduler = ent.Comp;
        if ((beat.AllowReinforcements || relaxing) && TryChooseReinforcement(scheduler, beat, count, relaxing) is { } reinforcement)
            return reinforcement;

        var desiredChange = goal.ExclusiveSubtract(chaos);
        var popScale = PopulationScale(scheduler, count);
        var now = _timing.CurTime;
        var roundDuration = GameTicker.RoundDuration();

        var ranked = new List<RankedEvent>();
        var calm = new List<RankedEvent>();

        foreach (var possibleEvent in scheduler.PossibleEvents)
        {
            if (possibleEvent.Reinforces.Count > 0
                || !CanDirectorRun(scheduler, beat, possibleEvent, count, roundDuration, relaxing))
                continue;

            var repeat = RepeatMultiplier(scheduler, possibleEvent.StationEvent, now);

            if (possibleEvent.Chaos.Empty)
            {
                if (beat.AllowCalmEvents && !relaxing && possibleEvent.EventType != HostilesSpawnEventType)
                    calm.Add(new RankedEvent(possibleEvent, possibleEvent.Weight * repeat));
                continue;
            }

            var benefit = Benefit(scheduler, desiredChange, possibleEvent.Chaos, popScale);
            if (benefit <= 0)
                continue;

            ranked.Add(new RankedEvent(possibleEvent, benefit * repeat * SpotlightMultiplier(scheduler, possibleEvent, count, now)));
        }

        if (ranked.Count > 0)
        {
            var best = ranked.OrderByDescending(r => r.Score).Take(beat.RandomEventLimit).ToList();
            var picked = PickWeighted(best, r => r.Score * r.PossibleEvent.Weight).PossibleEvent;
            LogMessage($"Picked {picked.StationEvent} from best events (in sequence) {string.Join(", ", best.Select(r => r.PossibleEvent.StationEvent))}");
            return picked;
        }

        if (calm.Count == 0)
            return null;

        var calmPick = PickWeighted(calm, r => r.Score).PossibleEvent;
        LogMessage($"Picked calm event {calmPick.StationEvent}, nothing moves chaos towards the goal");
        return calmPick;
    }

    private PossibleEvent? TryChooseReinforcement(GameDirectorComponent scheduler, StoryBeatPrototype beat, PlayerCount count, bool relaxing)
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
            if (understaffed.Count == 0 || !CanDirectorRun(scheduler, beat, possibleEvent, count, roundDuration, relaxing))
                continue;

            needed.UnionWith(understaffed);
            candidates.Add(new RankedEvent(possibleEvent,
                possibleEvent.Weight * understaffed.Count * RepeatMultiplier(scheduler, possibleEvent.StationEvent, now)));
        }

        if (candidates.Count == 0)
            return null;

        var picked = PickWeighted(candidates, r => r.Score).PossibleEvent;
        scheduler.LastReinforcement = now;
        LogMessage($"Understaffed: {string.Join(", ", needed.Select(d => $"{d} ({count.Staff(d)})"))}. Sending {picked.StationEvent}");
        return picked;
    }

    private bool CanDirectorRun(GameDirectorComponent scheduler,
        StoryBeatPrototype beat,
        PossibleEvent possibleEvent,
        PlayerCount count,
        TimeSpan roundDuration,
        bool relaxing)
    {
        if (possibleEvent.Destruction > beat.MaxDestruction
            || possibleEvent.Destruction > scheduler.DestructionBudget
            || possibleEvent.RequiredGhosts > count.Ghosts
            || beat.BlockedEventTypes.Contains(possibleEvent.EventType)
            || (beat.HelpOnly || relaxing) && possibleEvent.Chaos.ChaosDict.Values.Any(v => v > 0)
            || IsHelpEvent(possibleEvent) && OnHelpCooldown(scheduler)
            || !HasResponders(scheduler, possibleEvent.Chaos, count)
            || possibleEvent.RequiresStaff.Any(d => count.Staff(d) < scheduler.MinResponders))
            return false;

        var proto = _prototypeManager.Index(possibleEvent.StationEvent);
        return proto.TryComp<StationEventComponent>(out var stationEvent, _factory)
               && _event.CanRun(proto, stationEvent, count.InGame, roundDuration);
    }

    private static bool IsHelpEvent(PossibleEvent possibleEvent)
    {
        return !possibleEvent.Chaos.Empty && possibleEvent.Chaos.ChaosDict.Values.All(v => v <= 0);
    }

    private bool OnHelpCooldown(GameDirectorComponent scheduler)
    {
        return scheduler.LastHelp != TimeSpan.Zero
               && (_timing.CurTime - scheduler.LastHelp) * _event.EventSpeedup < scheduler.HelpCooldown;
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
        var groups = EventGroups(stationEvent);
        var multiplier = 1f;
        foreach (var fired in scheduler.FiredEvents)
        {
            var age = (now - fired.Time) * _event.EventSpeedup;
            if (fired.Event == stationEvent)
            {
                multiplier *= Recovery(scheduler.RepeatFactor, age, scheduler.RepeatWindow);
                continue;
            }

            foreach (var group in groups)
            {
                if (group.Events.Contains(fired.Event))
                    multiplier *= Recovery(group.Factor, age, group.Window);
            }
        }

        return multiplier;
    }

    private static float Recovery(float factor, TimeSpan age, TimeSpan window)
    {
        var progress = (float) Math.Clamp(age / window, 0.0, 1.0);
        return factor + (1f - factor) * progress;
    }

    private List<DirectorEventGroupPrototype> EventGroups(EntProtoId stationEvent)
    {
        if (_eventGroups == null)
        {
            _eventGroups = new Dictionary<EntProtoId, List<DirectorEventGroupPrototype>>();
            foreach (var group in _prototypeManager.EnumeratePrototypes<DirectorEventGroupPrototype>())
            {
                foreach (var member in group.Events)
                {
                    if (!_eventGroups.TryGetValue(member, out var list))
                        _eventGroups[member] = list = new List<DirectorEventGroupPrototype>();

                    list.Add(group);
                }
            }
        }

        return _eventGroups.TryGetValue(stationEvent, out var groups) ? groups : NoGroups;
    }

    private TimeSpan MaxGroupWindow()
    {
        var max = TimeSpan.Zero;
        foreach (var group in _prototypeManager.EnumeratePrototypes<DirectorEventGroupPrototype>())
        {
            if (group.Window > max)
                max = group.Window;
        }

        return max;
    }

    private float SpotlightMultiplier(GameDirectorComponent scheduler, PossibleEvent possibleEvent, PlayerCount count, TimeSpan now)
    {
        var idle = 0f;
        foreach (var (metric, value) in possibleEvent.Chaos.ChaosDict)
        {
            if (value <= 0
                || !scheduler.SpotlightDepartments.TryGetValue(metric, out var department)
                || count.Staff(department) < scheduler.MinResponders
                || !scheduler.LastSpotlight.TryGetValue(department, out var last))
                continue;

            idle = MathF.Max(idle, (float) Math.Clamp((now - last) * _event.EventSpeedup / scheduler.SpotlightWindow, 0.0, 1.0));
        }

        return 1f + scheduler.SpotlightBonus * idle;
    }

    private T PickWeighted<T>(IReadOnlyList<T> items, Func<T, float> weight)
    {
        var total = items.Sum(e => Math.Max(0f, weight(e)));
        if (total <= 0f)
            return _random.Pick(items);

        var roll = _random.NextFloat(total);
        foreach (var item in items)
        {
            roll -= Math.Max(0f, weight(item));
            if (roll <= 0f)
                return item;
        }

        return items[^1];
    }
}
