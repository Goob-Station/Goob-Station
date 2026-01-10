using System.Linq;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Server.StationEvents.Metric;
using Content.Server.StationEvents.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{

    /// <summary>
    ///   Build a list of events to use for the entire story
    /// </summary>
    private void SetupEvents(GameDirectorComponent scheduler, PlayerCount count, SelectedGameRulesComponent? selectedRules = null)
    {
        scheduler.PossibleEvents.Clear();

        if (selectedRules != null)
            SelectFromTable(scheduler, selectedRules);
        else
            SelectFromAllEvents(scheduler, count);

        LogMessage("All possible events added");
    }

    private void SelectFromAllEvents(GameDirectorComponent scheduler, PlayerCount count)
    {
        foreach (var proto in GameTicker.GetAllGameRulePrototypes())
        {
            if (!proto.TryGetComponent<StationEventComponent>(out var stationEvent, _factory) || !stationEvent.IsSelectable)
                continue;

            // Gate here on players, but not on round runtime. The story will probably last long enough for the
            // event to be ready to run again, we'll check CanRun again before we actually launch the event.
            if (!_event.CanRun(proto, stationEvent, count.Players, TimeSpan.MaxValue))
                continue;

            scheduler.PossibleEvents.Add(new PossibleEvent(proto.ID, stationEvent.Chaos));
        }
    }

    private void SelectFromTable(GameDirectorComponent scheduler, SelectedGameRulesComponent? selectedRules)
    {
        if (selectedRules == null)
            return;

        if(!_event.TryBuildLimitedEvents(selectedRules.ScheduledGameRules, _event.AvailableEvents(), out var possibleEvents))
            return;

        foreach (var (proto, stationEvent) in possibleEvents)
        {
            LogMessage(proto.ID);
            scheduler.PossibleEvents.Add(new PossibleEvent(proto.ID, stationEvent.Chaos));
        }
    }

    /// <summary>
    ///   Sorts the possible events and then picks semi-randomly.
    ///   when maxRandom is 1 it's always the "best" event picked. Higher values allow more events to be randomly selected.
    /// </summary>
    private RankedEvent SelectBest(List<RankedEvent> bestEvents, int maxRandom)
    {
        var ranked = bestEvents.OrderBy(ev => ev.Score).Take(maxRandom).ToList();

        var rand = _random.NextFloat();
        rand *= rand; // Square it, which leads to a front-weighted distribution
        // Of 3 items, there is (50% chance of 1, 36% chance of 2 and 14% chance of 3)
        rand *= ranked.Count - 1;

        var rankedEvent = ranked[(int) Math.Round(rand)];

        // Pick this event
        var events = string.Join(", ", ranked.Select(r => r.PossibleEvent.StationEvent));
        LogMessage($"Picked {rankedEvent.PossibleEvent.StationEvent} from best events (in sequence) {events}");
        return rankedEvent;
    }

    /// <summary>
    ///   Returns the StoryBeat that should be currently used to select events.
    ///   Advances the current story and picks new stories when the current beat is complete.
    /// </summary>
    private StoryBeatPrototype DetermineNextBeat(GameDirectorComponent scheduler, ChaosMetrics chaos, PlayerCount count)
    {
        if (TryGetCurrentBeat(scheduler, chaos, out var currentBeat))
            return currentBeat;

        scheduler.BeatStart = _timing.CurTime;

        if (TryGetNextBeatInStory(scheduler, out var nextBeat))
            return nextBeat;

        return TryStartNewStory(scheduler, count, out var firstBeat) ? firstBeat : GetFallbackBeat(scheduler);
    }

    /// <summary>
    /// Checks if current beat should continue or if it's complete
    /// </summary>
    private bool TryGetCurrentBeat(GameDirectorComponent scheduler, ChaosMetrics chaos, out StoryBeatPrototype beat)
    {
        beat = null!;

        if (scheduler.RemainingBeats.Count == 0)
            return false;

        var beatName = scheduler.RemainingBeats[0];
        beat = _prototypeManager.Index(beatName);
        var secsInBeat = (_timing.CurTime - scheduler.BeatStart).TotalSeconds / _event.EventSpeedup;
        if (secsInBeat < beat.MinSecs)
            return true;

        // Check completion conditions
        if (secsInBeat > beat.MaxSecs)
        {
            _sawmill.Info($"StoryBeat {beatName} complete. Lasted {secsInBeat:F0}s (max {beat.MaxSecs}s).");
            scheduler.RemainingBeats.RemoveAt(0);
            return false;
        }

        if (!beat.EndIfAnyWorse.Empty && chaos.AnyWorseThan(beat.EndIfAnyWorse))
        {
            _sawmill.Info($"StoryBeat {beatName} complete. Chaos exceeds threshold (EndIfAnyWorse).");
            scheduler.RemainingBeats.RemoveAt(0);
            return false;
        }

        if (beat.EndIfAllBetter.Empty || !chaos.AllBetterThan(beat.EndIfAllBetter))
            return true;
        _sawmill.Info($"StoryBeat {beatName} complete. Chaos below threshold (EndIfAllBetter).");
        scheduler.RemainingBeats.RemoveAt(0);
        return false;

        // Beat continues
    }

    /// <summary>
    /// Gets the next beat in the current story if available
    /// </summary>
    private bool TryGetNextBeatInStory(GameDirectorComponent scheduler, out StoryBeatPrototype beat)
    {
        beat = null!;

        if (scheduler.RemainingBeats.Count == 0)
            return false;

        var beatName = scheduler.RemainingBeats[0];
        beat = _prototypeManager.Index(beatName);

        StoryBeatChangesTotal.WithLabels(scheduler.CurrentStoryName.ToString(), beatName).Inc();
        LogMessage($"New StoryBeat {beatName}: {beat.Description}. Goal is {beat.Goal}");
        return true;
    }

    /// <summary>
    /// Starts a new story that matches player count
    /// </summary>
    private bool TryStartNewStory(GameDirectorComponent scheduler, PlayerCount count, out StoryBeatPrototype beat)
    {
        beat = null!;

        if (scheduler.Stories == null)
            return false;

        var stories = scheduler.Stories.ToList();
        _random.Shuffle(stories);

        foreach (var storyName in stories)
        {
            var story = _prototypeManager.Index(storyName);

            if (!IsStoryValid(story, count))
                continue;

            // Initialize the new story
            scheduler.RemainingBeats.AddRange(story.Beats!);
            scheduler.CurrentStoryName = storyName;
            SetupEvents(scheduler, count);

            _sawmill.Info($"New Story {storyName}: {story.Description}. {scheduler.PossibleEvents.Count} events.");

            var beatName = scheduler.RemainingBeats[0];
            beat = _prototypeManager.Index(beatName);

            StoryBeatChangesTotal.WithLabels(storyName.ToString(), beatName).Inc();
            LogMessage($"First StoryBeat {beatName}: {beat.Description}. Goal is {beat.Goal}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a story is valid for the current player count
    /// </summary>
    private static bool IsStoryValid(StoryPrototype story, PlayerCount count)
    {
        return story.Beats != null
               && story.MinPlayers <= count.Players
               && story.MaxPlayers >= count.Players;
    }

    /// <summary>
    /// Gets the fallback beat when no stories are available
    /// </summary>
    private StoryBeatPrototype GetFallbackBeat(GameDirectorComponent scheduler)
    {
        scheduler.RemainingBeats.Add(scheduler.FallbackBeatName);
        return _prototypeManager.Index(scheduler.FallbackBeatName);
    }

    private static float RankChaosDelta(ChaosMetrics chaos)
    {
        // Just a sum of squares (trying to get close to 0 on every score)
        //   Lower is better
        // Note:  if the chaos value is above 655.36 then its square is above maxint (inside FixedPoint2) and it wraps
        //        around. We need a full float range to handle the square.
        return chaos.ChaosDict.Values.Sum(v => (float) v * (float) v);
    }

    private List<RankedEvent> ChooseEvents(GameDirectorComponent scheduler, StoryBeatPrototype beat, ChaosMetrics chaos, PlayerCount count)
    {
        // TODO : Potentially filter Chaos here using CriticalLevels & DangerLevels which force us to focus on
        //        big problems (lots of hostiles, spacing) prior to smaller ones (food & drink)
        var desiredChange = beat.Goal.ExclusiveSubtract(chaos);
        var result = FilterAndScore(scheduler, chaos, desiredChange, count);
        if (result.Count > 0)
            return result;
        // Fall back to improving all scores (not just the ones the beat is focused on)
        //   Generally this means reducing chaos (unspecified scores are desired to be 0).
        var allDesiredChange = beat.Goal - chaos;
        result = FilterAndScore(scheduler, chaos, allDesiredChange, count, inclNoChaos:true);

        return result;
    }

    /// <summary>
    ///   Filter only to events which improve the chaos score in alignment with desiredChange.
    ///   Score them (lower is better) in how well they do this.
    /// </summary>
    private List<RankedEvent> FilterAndScore(GameDirectorComponent scheduler, ChaosMetrics chaos, ChaosMetrics desiredChange, PlayerCount count, bool inclNoChaos = false)
    {
        var noEvent = RankChaosDelta(desiredChange);
        var result = new List<RankedEvent>();

        // Choose an event that specifically achieves chaos goals, focusing only on them.
        foreach (var possibleEvent in scheduler.PossibleEvents)
        {
            // How much of the relevant chaos will be left after this event has occurred
            var relevantChaosDelta = desiredChange.ExclusiveSubtract(possibleEvent.Chaos);
            var rank = RankChaosDelta(relevantChaosDelta);

            var allChaosAfter = chaos + possibleEvent.Chaos;
            // Some events have no chaos score assigned. Treat them as if they change nothing and mix them in for flavor.
            var noChaosEvent = inclNoChaos && possibleEvent.Chaos.Empty;

            if (!(rank < noEvent) && !noChaosEvent)
                continue;
            // Look up this event's prototype and check it is ready to run.
            var proto = _prototypeManager.Index<EntityPrototype>(possibleEvent.StationEvent);

            if (!proto.TryGetComponent<StationEventComponent>(out var stationEvent, _factory))
                continue;

            if (!_event.CanRun(proto, stationEvent, count.Players, GameTicker.RoundDuration()))
                continue;

            result.Add(new RankedEvent(possibleEvent, allChaosAfter, rank));
        }

        return result;
    }
}
