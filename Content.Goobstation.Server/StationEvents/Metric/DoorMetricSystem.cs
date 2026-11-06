// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Server.Power.Components;
using Content.Shared.Access.Components;
using Content.Shared.Doors.Components;
using Content.Goobstation.Maths.FixedPoint;
using Prometheus; // Added for Prometheus metrics

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Uses doors and firelocks to sample station chaos across the station
///
///   Emag - EmagCost per emaged door
/// </summary>
public sealed partial class DoorMetricSystem : ChaosMetricSystem<DoorMetricComponent>
{

    private static readonly Gauge DoorsTotal = Metrics.CreateGauge(
        "game_director_metric_door_total",
        "Total number of doors counted on station grids.");

    private static readonly Gauge FirelocksTotal = Metrics.CreateGauge(
        "game_director_metric_door_firelocks_total",
        "Total number of firelocks counted.");

    private static readonly Gauge AirlocksTotal = Metrics.CreateGauge(
        "game_director_metric_door_airlocks_total",
        "Total number of airlocks counted.");

    private static readonly Gauge FirelocksHoldingFire = Metrics.CreateGauge(
        "game_director_metric_door_firelocks_holding_fire",
        "Number of firelocks currently holding back fire.");

    private static readonly Gauge FirelocksHoldingPressure = Metrics.CreateGauge(
        "game_director_metric_door_firelocks_holding_pressure",
        "Number of firelocks currently holding back pressure.");

    private static readonly Gauge EmaggedAirlocksWeighted = Metrics.CreateGauge(
        "game_director_metric_door_emagged_airlocks_weighted",
        "Weighted count of emagged airlocks (higher value for higher access).");

    private static readonly Gauge UnpoweredDoors = Metrics.CreateGauge(
        "game_director_metric_door_unpowered_total",
        "Number of doors or firelocks currently without power.");

    private static readonly Gauge SecurityChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_door_security_chaos_calculated",
        "Calculated chaos value contributed by security status (emagged doors).");


    protected override ChaosMetrics CalculateChaos(
        EntityUid metricUid,
        DoorMetricComponent component,
        CalculateChaosEvent args)
    {
        var firelockQ = GetEntityQuery<FirelockComponent>();
        var airlockQ = GetEntityQuery<AirlockComponent>();

        double doorCounter = 0;
        double firelockCounter = 0;
        double airlockCounter = 0;
        double fireCount = 0;
        double pressureCount = 0;
        double emagWeightedCount = 0;
        double powerCount = 0;

        // Add up the pain of all the doors
        // Restrict to just doors on the main station
        var stationGrids = GetEventStationGrids();

        var queryFirelock = EntityQueryEnumerator<DoorComponent, ApcPowerReceiverComponent, TransformComponent>();
        while (queryFirelock.MoveNext(out var uid, out var door, out var power, out var transform))
        {
            if (!OnGrids(transform, stationGrids))
                continue;

            fireCount = CalculateFirelock(firelockQ, uid, fireCount, ref pressureCount, ref firelockCounter);
            emagWeightedCount = CalculateAirlock(airlockQ, uid, door, emagWeightedCount, ref airlockCounter);
            powerCount = CalculateDoorPower(power, powerCount, ref doorCounter);
        }

        double emagChaos = 0;
        if (airlockCounter > 0)
            emagChaos = Math.Round((emagWeightedCount / airlockCounter) * component.EmagCost);

        DoorsTotal.Set(doorCounter);
        FirelocksTotal.Set(firelockCounter);
        AirlocksTotal.Set(airlockCounter);
        FirelocksHoldingFire.Set(fireCount);
        FirelocksHoldingPressure.Set(pressureCount);
        EmaggedAirlocksWeighted.Set(emagWeightedCount);
        UnpoweredDoors.Set(powerCount);
        SecurityChaosCalculated.Set(emagChaos);

        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, double>()
        {
            {ChaosMetric.Security, emagChaos},
        });
        return chaos;
    }

    protected override void ResetMetrics()
    {
        DoorsTotal.Set(0);
        FirelocksTotal.Set(0);
        AirlocksTotal.Set(0);
        FirelocksHoldingFire.Set(0);
        FirelocksHoldingPressure.Set(0);
        EmaggedAirlocksWeighted.Set(0);
        UnpoweredDoors.Set(0);
        SecurityChaosCalculated.Set(0);
    }

    private static double CalculateDoorPower(ApcPowerReceiverComponent power, double powerCount, ref double doorCounter)
    {
        if (power is { NeedsPower: true, Powered: false })
            powerCount += 1;

        doorCounter += 1;
        return powerCount;
    }

    private double CalculateAirlock(EntityQuery<AirlockComponent> airlockQ,
        EntityUid uid,
        DoorComponent door,
        double emagWeightedCount,
        ref double airlockCounter)
    {
        if (!airlockQ.TryGetComponent(uid, out var airlock))
            return emagWeightedCount;
        if (door.State == DoorState.Emagging)
        {
            var modifier = GetAccessLevelModifier(uid);
            emagWeightedCount += 1 + modifier;
        }

        airlockCounter += 1;

        return emagWeightedCount;
    }

    private static double CalculateFirelock(EntityQuery<FirelockComponent> firelockQ,
        EntityUid uid,
        double fireCount,
        ref double pressureCount,
        ref double firelockCounter)
    {
        if (!firelockQ.TryGetComponent(uid, out var firelock))
            return fireCount;

        if (firelock.Temperature)
            fireCount += 1;
        else if (firelock.Pressure)
            pressureCount += 1;

        firelockCounter += 1;

        return fireCount;
    }

    private int GetAccessLevelModifier(EntityUid uid)
    {
        if (!TryComp<AccessReaderComponent>(uid, out var accessReaderComponent))
            return 0;

        var modifier = 0;
        var accessSet = accessReaderComponent.AccessLists.ElementAt(0);
        foreach (var accessPrototype in accessSet)
        {
            // TODO: PROTOTYPE
            switch (accessPrototype.Id)
            {
                case "Security":
                case "Atmospherics":
                    modifier += 1;
                    break;
                case "Armory":
                    modifier += 3;
                    break;
                case "Command":
                    modifier += 2;
                    break;
            }
        }
        return modifier;
    }
}
