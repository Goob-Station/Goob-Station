using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Server.Atmos.Components;
using Content.Server.GameTicking;
using Content.Shared.Atmos;
using Prometheus;

namespace Content.Goobstation.Server.StationEvents.Metric;

public sealed partial class StationAtmosMetricSystem : ChaosMetricSystem<StationAtmosMetricComponent>
{
    [Dependency] private GameTicker _ticker = default!;

    private static readonly Gauge TilesTotal = Metrics.CreateGauge(
        "game_director_metric_atmos_tiles_total",
        "Total number of simulated air tiles on station grids.");

    private static readonly Gauge SpacedTiles = Metrics.CreateGauge(
        "game_director_metric_atmos_spaced_tiles",
        "Station air tiles below hazardous low pressure.");

    private static readonly Gauge FireTiles = Metrics.CreateGauge(
        "game_director_metric_atmos_fire_tiles",
        "Station air tiles with an active fire.");

    private static readonly Gauge ToxicTiles = Metrics.CreateGauge(
        "game_director_metric_atmos_toxic_tiles",
        "Station air tiles containing dangerous amounts of toxic gas.");

    private static readonly Gauge BaselineTiles = Metrics.CreateGauge(
        "game_director_metric_atmos_baseline_tiles",
        "Station air tiles that were already spaced or toxic at round start and are ignored.");

    private static readonly Gauge AtmosChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_atmos_chaos_calculated",
        "Calculated chaos value contributed by station air.");

    protected override ChaosMetrics CalculateChaos(EntityUid uid, StationAtmosMetricComponent component, CalculateChaosEvent args)
    {
        var tiles = 0;
        var spaced = 0;
        var fire = 0;
        var toxic = 0;

        if (!component.BaselineCaptured && _ticker.RoundDuration() < component.BaselineDelay)
        {
            AtmosChaosCalculated.Set(0);
            return new ChaosMetrics(new Dictionary<ChaosMetric, double> { { ChaosMetric.Atmos, 0 } });
        }

        var capture = !component.BaselineCaptured;
        foreach (var grid in GetEventStationGrids())
        {
            if (!TryComp<GridAtmosphereComponent>(grid, out var atmosphere))
                continue;

            foreach (var tile in atmosphere.Tiles.Values)
            {
                if (tile.Space || tile.MapAtmosphere || tile.NoGridTile || tile.Air is not { } air)
                    continue;

                tiles++;

                if (tile.Hotspot.Valid)
                    fire++;

                var isSpaced = air.Pressure < component.SpacedPressure;
                var isToxic = IsToxic(component, air);
                if (!isSpaced && !isToxic)
                    continue;

                var key = (grid, tile.GridIndices);
                if (capture)
                {
                    component.BaselineTiles.Add(key);
                    continue;
                }

                if (component.BaselineTiles.Contains(key))
                    continue;

                if (isSpaced)
                    spaced++;

                if (isToxic)
                    toxic++;
            }
        }

        if (capture && tiles > 0)
            component.BaselineCaptured = true;

        var atmosChaos = spaced * component.SpacedTileCost
                         + toxic * component.ToxicTileCost;
        var fireChaos = fire * component.FireTileCost;

        TilesTotal.Set(tiles);
        BaselineTiles.Set(component.BaselineTiles.Count);
        SpacedTiles.Set(spaced);
        FireTiles.Set(fire);
        ToxicTiles.Set(toxic);
        AtmosChaosCalculated.Set(atmosChaos);

        return new ChaosMetrics(new Dictionary<ChaosMetric, double>
        {
            { ChaosMetric.Atmos, atmosChaos },
            { ChaosMetric.Fire, fireChaos },
        });
    }

    private static bool IsToxic(StationAtmosMetricComponent component, GasMixture air)
    {
        foreach (var (gas, moles) in component.ToxicGasMoles)
        {
            if (air.GetMoles(gas) > moles)
                return true;
        }

        return false;
    }

    protected override void ResetMetrics()
    {
        TilesTotal.Set(0);
        BaselineTiles.Set(0);
        SpacedTiles.Set(0);
        FireTiles.Set(0);
        ToxicTiles.Set(0);
        AtmosChaosCalculated.Set(0);
    }
}
