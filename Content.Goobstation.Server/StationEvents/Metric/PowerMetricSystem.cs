using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Server.GameTicking;
using Content.Server.Power.Components;
using Content.Shared.APC;
using Prometheus;

namespace Content.Goobstation.Server.StationEvents.Metric;

public sealed partial class PowerMetricSystem : ChaosMetricSystem<PowerMetricComponent>
{
    [Dependency] private GameTicker _gameTicker = default!;

    private static readonly Gauge ApcsTotal = Metrics.CreateGauge(
        "game_director_metric_power_apcs_total",
        "Total number of APCs on station grids.");

    private static readonly Gauge ApcsUnpowered = Metrics.CreateGauge(
        "game_director_metric_power_apcs_unpowered",
        "APCs on station grids that are switched off or cannot keep their area powered.");

    private static readonly Gauge PowerChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_power_chaos_calculated",
        "Calculated chaos value contributed by APC power.");

    protected override ChaosMetrics CalculateChaos(EntityUid uid, PowerMetricComponent component, CalculateChaosEvent args)
    {
        var stationGrids = GetEventStationGrids();
        var capture = !component.BaselineCaptured && _gameTicker.RoundDuration() >= component.BaselineDelay;
        var total = 0;
        var unpowered = 0;

        var query = EntityQueryEnumerator<ApcComponent, TransformComponent>();
        while (query.MoveNext(out var apcUid, out var apc, out var xform))
        {
            if (!OnGrids(xform, stationGrids) || component.BaselineUnpowered.Contains(apcUid))
                continue;

            var isUnpowered = !apc.MainBreakerEnabled || apc.LastChargeState == ApcChargeState.Lack;
            if (capture && isUnpowered)
            {
                component.BaselineUnpowered.Add(apcUid);
                continue;
            }

            total++;
            if (isUnpowered)
                unpowered++;
        }

        if (capture)
            component.BaselineCaptured = true;

        var powerChaos = total > 0 ? Math.Round((double) unpowered / total * component.UnpoweredCost) : 0;

        ApcsTotal.Set(total);
        ApcsUnpowered.Set(unpowered);
        PowerChaosCalculated.Set(powerChaos);

        return new ChaosMetrics(new Dictionary<ChaosMetric, double>
        {
            { ChaosMetric.Power, powerChaos },
        });
    }

    protected override void ResetMetrics()
    {
        ApcsTotal.Set(0);
        ApcsUnpowered.Set(0);
        PowerChaosCalculated.Set(0);
    }
}
