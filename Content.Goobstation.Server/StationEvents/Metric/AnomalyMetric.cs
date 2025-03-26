using Content.Server.Spreader;
using Content.Shared.Anomaly.Components;
using Content.Shared.FixedPoint;

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Measures the number and severity of anomalies on the station.
///
///   Writes this to the Anomaly chaos value.
/// </summary>
public sealed class AnomalyMetric : ChaosMetricSystem<Goobstation.Server.StationEvents.Metric.Components.AnomalyMetricComponent>
{
    public override ChaosMetrics CalculateChaos(EntityUid metricUid,
        Goobstation.Server.StationEvents.Metric.Components.AnomalyMetricComponent component,
        CalculateChaosEvent args)
    {
        var anomalyChaos = FixedPoint2.Zero;

        // Consider each anomaly and add its stability and growth to the accumulator
        var anomalyQ = EntityQueryEnumerator<AnomalyComponent>();
        while (anomalyQ.MoveNext(out var uid, out var anomaly))
        {
            if (anomaly.Severity > 0.8f)
            {
                anomalyChaos += component.SeverityCost;
            }

            if (anomaly.Stability > anomaly.GrowthThreshold)
            {
                anomalyChaos += component.GrowingCost;
            }

            anomalyChaos += component.BaseCost;
        }

        var kudzuQ = EntityQueryEnumerator<KudzuComponent>();
        while (kudzuQ.MoveNext(out var uid, out var kudzu))
        {
            anomalyChaos += 0.25f;
        }

    var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, FixedPoint2>()
        {
            {ChaosMetric.Anomaly, anomalyChaos},
        });
        return chaos;
    }
}
