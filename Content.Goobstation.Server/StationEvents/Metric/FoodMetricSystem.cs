// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Roles;

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Measure crew's hunger and thirst
///
/// </summary>
public sealed class FoodMetricSystem : ChaosMetricSystem<FoodMetricComponent>
{
    [Dependency] private readonly SharedRoleSystem _roles = default!;

    public override ChaosMetrics CalculateChaos(EntityUid metric_uid, FoodMetricComponent component,
        CalculateChaosEvent args)
    {
        // Gather hunger and thirst scores
        var query = EntityQueryEnumerator<MindContainerComponent, MobStateComponent>();
        double hungerSc = 0;
        double thirstSc = 0;
        double chargeSc = 0;

        var thirstQ = GetEntityQuery<ThirstComponent>();
        var hungerQ = GetEntityQuery<HungerComponent>();
        var siliconQ = GetEntityQuery<SiliconComponent>();

        while (query.MoveNext(out var uid, out var mindContainer, out var mobState))
        {
            // Don't count anything that is mindless, do count antags
            if (mindContainer.Mind == null)
                continue;

            if (mobState.CurrentState != MobState.Alive)
                continue;

            if (thirstQ.TryGetComponent(uid, out var thirst))
            {
                thirstSc += component.ThirstScores.GetValueOrDefault(thirst.CurrentThirstThreshold).Double();
            }

            if (hungerQ.TryGetComponent(uid, out var hunger))
            {
                hungerSc += component.HungerScores.GetValueOrDefault(hunger.CurrentThreshold).Double();
            }

            if (siliconQ.TryGetComponent(uid, out var silicon))
            {
                var chargeState = GetChargeState(silicon.ChargeState);
                chargeSc += component.ChargeScores.GetValueOrDefault(chargeState).Double();
            }
        }

        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, double>()
        {
            {ChaosMetric.Hunger, hungerSc},
            {ChaosMetric.Thirst, thirstSc},
            {ChaosMetric.Charge, chargeSc},
        });
        return chaos;
    }

    private float GetChargeState(short chargeState)
    {
        var mid = 0.5f;
        var low = 0.25f;
        var critical = 0.1f;

        var normalizedCharge = chargeState / 10f; // Assuming ChargeState is from 0-10

        if (normalizedCharge <= critical)
            return critical;
        if (normalizedCharge <= low)
            return low;

        return mid;
    }

}
