// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Doors.Components;
using Content.Shared.FixedPoint;

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Uses doors and firelocks to sample station chaos across the station
///
///   Emag - EmagCost per emaged door
///   Power - PowerCost per door or firelock with no power
///   Atmos - PressureCost for holding spacing or FireCost for holding back fire
/// </summary>
public sealed class DoorMetricSystem : ChaosMetricSystem<DoorMetricComponent>
{
    [Dependency] private readonly StationSystem _stationSystem = default!;

    public override ChaosMetrics CalculateChaos(EntityUid metric_uid, DoorMetricComponent component,
        CalculateChaosEvent args)
    {
        var firelockQ = GetEntityQuery<FirelockComponent>();
        var airlockQ = GetEntityQuery<AirlockComponent>();

        // Keep counters to calculate average at the end.
        double doorCounter = 0;
        double firelockCounter = 0;
        double airlockCounter = 0;

        double fireCount = 0;
        double pressureCount = 0;
        double emagCount = 0;
        double powerCount = 0;

        // Add up the pain of all the doors
        // Restrict to just doors on the main station
        var stationGrids = _stationSystem.GetAllStationGrids();

        var queryFirelock = EntityQueryEnumerator<DoorComponent, ApcPowerReceiverComponent, TransformComponent>();
        while (queryFirelock.MoveNext(out var uid, out var door, out var power, out var transform))
        {
            if (transform.GridUid == null || !stationGrids.Contains(transform.GridUid.Value))
                continue;

            if (firelockQ.TryGetComponent(uid, out var firelock))
            {
                if (firelock.Temperature)
                {
                    fireCount += 1;
                }
                else if (firelock.Pressure)
                {
                    pressureCount += 1;
                }

                firelockCounter += 1;
            }

            if (airlockQ.TryGetComponent(uid, out var airlock))
            {
                if (door.State == DoorState.Emagging)
                {
                    var modifier = GetAccessLevelModifier(uid);
                    emagCount += 1 + modifier;
                }

                airlockCounter += 1;
            }

            if (power.Recalculate || !power.NeedsPower)
            {
                powerCount += 1;
            }

            doorCounter += 1;
        }

        double emagChaos = 0;
        double atmosChaos = 0;
        double powerChaos = 0;
        // Calculate each stat as a fraction of all doors in the station.
        //   That way the metrics do not "scale up"  on large stations.

        if (airlockCounter > 0)
            emagChaos = Math.Round((emagCount / airlockCounter) * component.EmagCost);

        if (firelockCounter > 0)
            atmosChaos = Math.Round((fireCount / firelockCounter) * component.FireCost +
                                    (pressureCount / firelockCounter) * component.PressureCost);

        if (doorCounter > 0)
            powerChaos = Math.Round((powerCount / doorCounter) * component.PowerCost);


        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, double>()
        {
            {ChaosMetric.Security, emagChaos},
            {ChaosMetric.Atmos, atmosChaos},
            {ChaosMetric.Power, powerChaos},
        });
        return chaos;
    }

    private int GetAccessLevelModifier(EntityUid uid)
    {
        if (!TryComp<AccessReaderComponent>(uid, out var accessReaderComponent))
            return 0;

        var modifier = 0;
        var accessSet = accessReaderComponent.AccessLists.ElementAt(0);
        foreach (var accessPrototype in accessSet)
        {
            switch (accessPrototype.Id)
            {
                case "Security":
                    modifier += 1;
                    break;
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