using System.Linq;
using Content.Goobstation.Server.StationEvents.Components;
using Content.Goobstation.Server.StationEvents.Metric;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{
    private const string NoStoryLabel = "None";

    private enum BeatEnd : byte
    {
        None,
        Timeout,
        Worse,
        Better,
    }

    private StoryBeatPrototype DetermineNextBeat(Entity<GameDirectorComponent> ent, ChaosMetrics chaos, PlayerCount count)
    {
        var scheduler = ent.Comp;
        UpdateDenouement(scheduler);

        if (scheduler.CurrentBeat is { } currentId
            && scheduler.RemainingBeats.Count > 0
            && scheduler.RemainingBeats[0] == currentId)
        {
            var current = _prototypeManager.Index(currentId);
            var end = CheckBeatEnd(scheduler, current, chaos, count);
            if (end == BeatEnd.None)
                return current;

            scheduler.RemainingBeats.RemoveAt(0);
            scheduler.CurrentBeat = null;
            BranchFrom(scheduler, current, end);
        }

        SkipUnstaffedBeats(scheduler, count);

        if (scheduler.RemainingBeats.Count == 0 && !TryStartNewStory(ent, count.Connected, count))
            scheduler.RemainingBeats.Add(scheduler.FallbackBeatName);

        return StartBeat(scheduler, chaos, scheduler.RemainingBeats[0]);
    }

    private BeatEnd CheckBeatEnd(GameDirectorComponent scheduler, StoryBeatPrototype beat, ChaosMetrics chaos, PlayerCount count)
    {
        var secsInBeat = (_timing.CurTime - scheduler.BeatStart).TotalSeconds * _event.EventSpeedup;
        if (secsInBeat < beat.MinSecs)
            return BeatEnd.None;

        var end = BeatEnd.None;
        if (secsInBeat > beat.MaxSecs)
            end = BeatEnd.Timeout;
        else if (!beat.EndIfAnyWorse.Empty && chaos.AnyWorseThan(ScaleForPopulation(scheduler, beat.EndIfAnyWorse, count)))
            end = BeatEnd.Worse;
        else if (!beat.EndIfAllBetter.Empty && chaos.AllBetterThan(ScaleForPopulation(scheduler, beat.EndIfAllBetter, count)))
            end = BeatEnd.Better;

        if (end != BeatEnd.None)
            LogMessage($"StoryBeat {beat.ID} complete after {secsInBeat:F0}s ({end}).", false);

        return end;
    }

    private void BranchFrom(GameDirectorComponent scheduler, StoryBeatPrototype beat, BeatEnd end)
    {
        var target = end switch
        {
            BeatEnd.Worse => beat.OnWorse,
            BeatEnd.Better => beat.OnBetter,
            _ => null,
        };

        if (target is not { } next
            || next.Id == beat.ID
            || scheduler.RemainingBeats.Count > 0 && scheduler.RemainingBeats[0] == next)
            return;

        scheduler.RemainingBeats.Insert(0, next);
        LogMessage($"StoryBeat {beat.ID} ended {end}, branching to {next}");
    }

    private void UpdateDenouement(GameDirectorComponent scheduler)
    {
        if (scheduler.DenouementBeat is not { } denouement)
            return;

        var active = scheduler.RemainingBeats.Count > 0 && scheduler.RemainingBeats[0] == denouement;
        var requested = _roundEnd.IsRoundEndRequested();

        if (requested && !active)
        {
            scheduler.RemainingBeats.Insert(0, denouement);
            LogMessage("Evacuation called, winding the story down");
            CleanupSpawners(scheduler, _timing.CurTime, true);
        }
        else if (!requested && active)
        {
            scheduler.RemainingBeats.RemoveAt(0);
            scheduler.CurrentBeat = null;
            LogMessage("Evacuation recalled, resuming the story");
        }
    }

    private void SkipUnstaffedBeats(GameDirectorComponent scheduler, PlayerCount count)
    {
        while (scheduler.RemainingBeats.Count > 0)
        {
            var beat = _prototypeManager.Index(scheduler.RemainingBeats[0]);
            var missing = beat.RequiredDepartments.Where(d => count.Staff(d) < scheduler.MinResponders).ToList();
            if (missing.Count == 0)
                return;

            LogMessage($"Skipping StoryBeat {beat.ID}, nobody available in {string.Join(", ", missing)}", false);
            scheduler.RemainingBeats.RemoveAt(0);
        }
    }

    private StoryBeatPrototype StartBeat(GameDirectorComponent scheduler, ChaosMetrics chaos, ProtoId<StoryBeatPrototype> beatId)
    {
        var beat = _prototypeManager.Index(beatId);
        scheduler.CurrentBeat = beatId;
        scheduler.BeatStart = _timing.CurTime;
        scheduler.BeatStartChaos = new ChaosMetrics(chaos);

        var storyName = scheduler.CurrentStoryName.Id ?? NoStoryLabel;
        StoryBeatChangesTotal.WithLabels(storyName, beat.ID).Inc();
        SetBeatInfo(storyName, beat.ID);
        LogMessage($"New StoryBeat {beat.ID}: {beat.Description}. Goal is {beat.Goal}, escalation {beat.Escalation}");
        return beat;
    }

    private bool TryStartNewStory(Entity<GameDirectorComponent> ent, int connected, PlayerCount? staffing)
    {
        var scheduler = ent.Comp;
        var candidates = new List<KeyValuePair<ProtoId<StoryPrototype>, float>>();
        foreach (var candidate in scheduler.Stories)
        {
            if (candidate.Value > 0 && IsStoryValid(_prototypeManager.Index(candidate.Key), connected))
                candidates.Add(candidate);
        }

        while (candidates.Count > 0)
        {
            var picked = PickWeighted(candidates, c => c.Value);
            candidates.Remove(picked);

            var story = _prototypeManager.Index(picked.Key);
            scheduler.RemainingBeats.AddRange(story.Beats!);
            if (staffing != null)
                SkipUnstaffedBeats(scheduler, staffing);

            if (scheduler.RemainingBeats.Count == 0)
                continue;

            scheduler.CurrentStoryName = picked.Key;
            SetupEvents(ent, story);
            LogMessage($"New Story {picked.Key}: {story.Description}. {scheduler.PossibleEvents.Count} events.");
            return true;
        }

        return false;
    }

    private static bool IsStoryValid(StoryPrototype story, int connected)
    {
        return story.Beats is { Length: > 0 }
               && story.MinPlayers <= connected
               && story.MaxPlayers >= connected;
    }

    private float PopulationScale(GameDirectorComponent scheduler, PlayerCount count)
    {
        if (scheduler.ReferencePlayers <= 0)
            return 1f;

        return Math.Clamp((float) count.Players / scheduler.ReferencePlayers,
            scheduler.MinPopulationScale,
            scheduler.MaxPopulationScale);
    }

    private ChaosMetrics ScaleForPopulation(GameDirectorComponent scheduler, ChaosMetrics metrics, PlayerCount count)
    {
        var scale = PopulationScale(scheduler, count);
        var scaled = new ChaosMetrics(metrics);
        foreach (var (metric, value) in metrics.ChaosDict)
        {
            if (scheduler.PopulationScaledMetrics.Contains(metric))
                scaled.ChaosDict[metric] = value * scale;
        }

        return scaled;
    }

    private ChaosMetrics EffectiveGoal(GameDirectorComponent scheduler, StoryBeatPrototype beat, PlayerCount count)
    {
        var goal = ScaleForPopulation(scheduler, beat.Goal, count);
        foreach (var (metric, value) in ScaleForPopulation(scheduler, beat.Escalation, count).ChaosDict)
            goal.ChaosDict[metric] = scheduler.BeatStartChaos.ChaosDict.GetValueOrDefault(metric) + value;

        return goal;
    }
}
