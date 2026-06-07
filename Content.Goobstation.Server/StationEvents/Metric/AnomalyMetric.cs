// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Spreader;
using Content.Shared.Anomaly.Components;
using Content.Goobstation.Maths.FixedPoint;
using Prometheus;

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Measures the number and severity of anomalies on the station.
///
///   Writes this to the Anomaly chaos value.
/// </summary>
public sealed class AnomalyMetric : ChaosMetricSystem<Components.AnomalyMetricComponent>
{
    private static readonly Gauge AnomalyTotal = Metrics.CreateGauge(
        "game_director_metric_anomaly_total",
        "Total number of active anomalies.");

    private static readonly Gauge AnomalySevereTotal = Metrics.CreateGauge(
        "game_director_metric_anomaly_severe_total",
        "Total number of severe anomalies (severity > 0.8).");

    private static readonly Gauge AnomalyGrowingTotal = Metrics.CreateGauge(
        "game_director_metric_anomaly_growing_total",
        "Total number of growing anomalies (stability > growth threshold).");

    private static readonly Gauge KudzuTotal = Metrics.CreateGauge(
        "game_director_metric_kudzu_total",
        "Total number of kudzu patches.");

    private static readonly Gauge AnomalyChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_anomaly_chaos_calculated",
        "Calculated chaos value contributed by anomalies and kudzu.");

    private const float KudzuChaosValue = 0.25f;


    protected override ChaosMetrics CalculateChaos(EntityUid metricUid,
        Components.AnomalyMetricComponent component,
        CalculateChaosEvent args)
    {
        double anomalyChaos = 0;
        var anomalyCount = 0;
        var severeAnomalyCount = 0;
        var growingAnomalyCount = 0;
        var kudzuCount = 0;

        // Consider each anomaly and add its stability and growth to the accumulator
        var anomalyQ = EntityQueryEnumerator<AnomalyComponent>();
        while (anomalyQ.MoveNext(out _, out var anomaly))
        {
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

        var kudzuQ = EntityQueryEnumerator<KudzuComponent>();
        while (kudzuQ.MoveNext(out _, out _))
        {
            kudzuCount++;
            anomalyChaos += KudzuChaosValue;
        }

        AnomalyTotal.Set(anomalyCount);
        AnomalySevereTotal.Set(severeAnomalyCount);
        AnomalyGrowingTotal.Set(growingAnomalyCount);
        KudzuTotal.Set(kudzuCount);
        AnomalyChaosCalculated.Set(anomalyChaos);

        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, double>
        {
            {ChaosMetric.Anomaly, anomalyChaos},
        });
        return chaos;
    }
}
