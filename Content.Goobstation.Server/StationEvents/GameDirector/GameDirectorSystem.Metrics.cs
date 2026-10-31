using System.Linq;
using Content.Goobstation.Server.StationEvents.Metric;
using Content.Server.Antag.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Roles;
using Prometheus;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{
    private static readonly Gauge EventsRunTotal = Metrics.CreateGauge(
        "game_director_events_run_total",
        "Total number of station events run by the Game Director this round.",
        "event_name",
        "beat_name");

    private static readonly Gauge StoryBeatChangesTotal = Metrics.CreateGauge(
        "game_director_story_beat_changes_total",
        "Total number of story beat changes this round.",
        "story_name",
        "beat_name");

    private static readonly Gauge BeatInfo = Metrics.CreateGauge(
        "game_director_beat_info",
        "The story and beat the Game Director is currently running. Always 1.",
        "story_name",
        "beat_name");

    private static readonly Gauge ActivePlayers = Metrics.CreateGauge(
        "game_director_active_players",
        "Current number of active players counted by the Game Director.");

    private static readonly Gauge ActiveGhosts = Metrics.CreateGauge(
        "game_director_active_ghosts",
        "Current number of active ghosts counted by the Game Director.");

    private static readonly Gauge RoundstartAntagsSelectedTotal = Metrics.CreateGauge(
        "game_director_roundstart_antags_selected_total",
        "Roundstart antag game rules the Game Director started this round (one per rule, not per player).",
        "antag_name");

    private static readonly Gauge ChaosGauge = Metrics.CreateGauge(
        "game_director_chaos",
        "Measured chaos per metric at the last Game Director tick.",
        "metric");

    private static readonly Gauge PendingChaosGauge = Metrics.CreateGauge(
        "game_director_pending_chaos",
        "Expected chaos from recently fired events that the Game Director still counts towards each metric.",
        "metric");

    private static readonly Gauge GoalGauge = Metrics.CreateGauge(
        "game_director_goal",
        "Effective goal per metric for the current beat, after population scaling and escalation.",
        "metric");

    private static readonly Gauge DepartmentStaff = Metrics.CreateGauge(
        "game_director_department_staff",
        "Alive, connected crew per department.",
        "department");

    private static readonly Gauge AntagsGauge = Metrics.CreateGauge(
        "game_director_antags",
        "Players assigned as antagonists by each active antag game rule, including rules started outside the Game Director.",
        "rule");

    private static readonly Gauge DestructionBudgetGauge = Metrics.CreateGauge(
        "game_director_destruction_budget",
        "Destruction budget available to the Game Director.");

    private static void ResetGaugeLabels<TChild>(Collector<TChild> gauge) where TChild : ChildBase
    {
        foreach (var labelSet in gauge.GetAllLabelValues().ToList())
            gauge.RemoveLabelled(labelSet);
    }

    private static void ResetMetrics()
    {
        ActivePlayers.Set(0);
        ActiveGhosts.Set(0);
        DestructionBudgetGauge.Set(0);
        ResetGaugeLabels(EventsRunTotal);
        ResetGaugeLabels(StoryBeatChangesTotal);
        ResetGaugeLabels(BeatInfo);
        ResetGaugeLabels(RoundstartAntagsSelectedTotal);
        ResetGaugeLabels(ChaosGauge);
        ResetGaugeLabels(PendingChaosGauge);
        ResetGaugeLabels(GoalGauge);
        ResetGaugeLabels(DepartmentStaff);
        ResetGaugeLabels(AntagsGauge);
    }

    private void UpdateStaffingMetrics(PlayerCount count)
    {
        foreach (var department in _prototypeManager.EnumeratePrototypes<DepartmentPrototype>())
            DepartmentStaff.WithLabels(department.ID).Set(count.Staff(department.ID));
    }

    private void UpdateAntagMetrics()
    {
        var counts = new Dictionary<string, int>();
        var query = EntityQueryEnumerator<AntagSelectionComponent, ActiveGameRuleComponent, MetaDataComponent>();
        while (query.MoveNext(out _, out var antag, out _, out var meta))
        {
            if (meta.EntityPrototype is not { } proto)
                continue;

            counts[proto.ID] = counts.GetValueOrDefault(proto.ID) + antag.AssignedMinds.Count;
        }

        ResetGaugeLabels(AntagsGauge);
        foreach (var (rule, count) in counts)
            AntagsGauge.WithLabels(rule).Set(count);
    }

    private static void SetBeatInfo(string storyName, string beatName)
    {
        ResetGaugeLabels(BeatInfo);
        BeatInfo.WithLabels(storyName, beatName).Set(1);
    }

    private static void UpdateChaosMetrics(ChaosMetrics chaos, ChaosMetrics pending, ChaosMetrics goal, float destructionBudget)
    {
        foreach (var metric in Enum.GetValues<ChaosMetric>())
        {
            var label = metric.ToString();
            ChaosGauge.WithLabels(label).Set(chaos.ChaosDict.GetValueOrDefault(metric));
            PendingChaosGauge.WithLabels(label).Set(pending.ChaosDict.GetValueOrDefault(metric));

            if (goal.ChaosDict.TryGetValue(metric, out var goalValue))
                GoalGauge.WithLabels(label).Set(goalValue);
            else
                GoalGauge.RemoveLabelled(label);
        }

        DestructionBudgetGauge.Set(destructionBudget);
    }
}
