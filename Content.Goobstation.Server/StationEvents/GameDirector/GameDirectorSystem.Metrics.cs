using System.Linq;
using Prometheus;

namespace Content.Goobstation.Server.StationEvents.GameDirector;

public sealed partial class GameDirectorSystem
{
    private static readonly Gauge EventsRunTotal = Metrics.CreateGauge(
        "game_director_events_run_total",
        "Total number of station events run by the Game Director.",
        "event_name");

    private static readonly Gauge StoryBeatChangesTotal = Metrics.CreateGauge(
        "game_director_story_beat_changes_total",
        "Total number of story beat changes.",
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
        "Total number of roundstart antagonists selected.",
        "antag_name");

    /// <summary>
    /// Removes all labels from a gauge.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="gauge"></param>
    private static void ResetGaugeLabels<TChild>(Collector<TChild> gauge) where TChild : ChildBase
    {
        // Get all the label values currently in use
        var labelValues = gauge.GetAllLabelValues().ToList();

        // For each set of label values, remove that shit.
        foreach (var labelSet in labelValues)
            gauge.RemoveLabelled(labelSet);
    }

    /// <summary>
    /// Lists all the label values of a gauge. Useful for debugging!
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    /// <param name="gauge"></param>
    public void ListAllLabelValues<TChild>(Collector<TChild> gauge) where TChild : ChildBase
    {
        var labelValues = gauge.GetAllLabelValues();
        foreach (var labelSet in labelValues)
        foreach (var label in labelSet)
            Log.Warning($"Label: {label}");
    }

    /// <summary>
    /// Resets all metrics.
    /// </summary>
    private static void ResetMetrics()
    {
        ActivePlayers.Set(0);
        ActiveGhosts.Set(0);
        ResetGaugeLabels(EventsRunTotal);
        ResetGaugeLabels(StoryBeatChangesTotal);
        ResetGaugeLabels(RoundstartAntagsSelectedTotal);
    }

}
