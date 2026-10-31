// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Anomaly.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Xenoarchaeology.Artifact.Components;
using Prometheus;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Measures the number and severity of anomalies on the station.
///
///   Writes this to the Anomaly chaos value.
/// </summary>
public sealed partial class AnomalyMetric : ChaosMetricSystem<Components.AnomalyMetricComponent>
{
    [Dependency] private SharedContainerSystem _container = default!;

    private static readonly Gauge AnomalyTotal = Metrics.CreateGauge(
        "game_director_metric_anomaly_total",
        "Total number of active anomalies.");

    private static readonly Gauge AnomalySevereTotal = Metrics.CreateGauge(
        "game_director_metric_anomaly_severe_total",
        "Total number of severe anomalies (severity > 0.8).");

    private static readonly Gauge AnomalyGrowingTotal = Metrics.CreateGauge(
        "game_director_metric_anomaly_growing_total",
        "Total number of growing anomalies (stability > growth threshold).");

    private static readonly Gauge ArtifactTotal = Metrics.CreateGauge(
        "game_director_metric_artifact_total",
        "Unsuppressed xenoartifacts on station grids outside of containers.");

    private static readonly Gauge AnomalyChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_anomaly_chaos_calculated",
        "Calculated chaos value contributed by anomalies and artifacts.");

    protected override ChaosMetrics CalculateChaos(EntityUid metricUid,
        Components.AnomalyMetricComponent component,
        CalculateChaosEvent args)
    {
        double anomalyChaos = 0;
        var anomalyCount = 0;
        var severeAnomalyCount = 0;
        var growingAnomalyCount = 0;
        var artifactCount = 0;

        var stationGrids = GetEventStationGrids();

        // Consider each anomaly and add its stability and growth to the accumulator
        var anomalyQ = EntityQueryEnumerator<AnomalyComponent, TransformComponent>();
        while (anomalyQ.MoveNext(out _, out var anomaly, out var anomalyXform))
        {
            if (!OnGrids(anomalyXform, stationGrids))
                continue;

            anomalyCount++;
            if (anomaly.Severity > 0.8f)
            {
                severeAnomalyCount++;
                anomalyChaos += component.SeverityCost;
            }

            if (anomaly.Stability > anomaly.GrowthThreshold)
            {
                growingAnomalyCount++;
                anomalyChaos += component.GrowingCost;
            }

            anomalyChaos += component.BaseCost;
        }

        var artifactQ = EntityQueryEnumerator<XenoArtifactComponent, TransformComponent>();
        while (artifactQ.MoveNext(out var artifactUid, out var artifact, out var artifactXform))
        {
            if (artifact.Suppressed || !OnGrids(artifactXform, stationGrids) || _container.IsEntityInContainer(artifactUid))
                continue;

            artifactCount++;
            anomalyChaos += component.ArtifactCost;
        }

        AnomalyTotal.Set(anomalyCount);
        AnomalySevereTotal.Set(severeAnomalyCount);
        AnomalyGrowingTotal.Set(growingAnomalyCount);
        ArtifactTotal.Set(artifactCount);
        AnomalyChaosCalculated.Set(anomalyChaos);

        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, double>
        {
            {ChaosMetric.Anomaly, anomalyChaos},
        });
        return chaos;
    }

    protected override void ResetMetrics()
    {
        AnomalyTotal.Set(0);
        AnomalySevereTotal.Set(0);
        AnomalyGrowingTotal.Set(0);
        ArtifactTotal.Set(0);
        AnomalyChaosCalculated.Set(0);
    }
}
