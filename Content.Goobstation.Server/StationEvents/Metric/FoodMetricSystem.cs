// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Roles;
using Prometheus;

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Measure crew's hunger, thirst and charge
/// </summary>
public sealed class FoodMetricSystem : ChaosMetricSystem<FoodMetricComponent>
{
    private static readonly Gauge HungerThresholdCount = Metrics.CreateGauge(
        "game_director_metric_food_hunger_threshold_count",
        "Number of entities at a specific hunger threshold.",
        "threshold");

    private static readonly Gauge ThirstThresholdCount = Metrics.CreateGauge(
        "game_director_metric_food_thirst_threshold_count",
        "Number of entities at a specific thirst threshold.",
        "threshold");

    private static readonly Gauge SiliconChargeStateCount = Metrics.CreateGauge(
        "game_director_metric_food_silicon_charge_state_count",
        "Number of silicon entities at a specific normalized charge state.",
        "charge_state");

    private static readonly Gauge HungerChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_food_hunger_chaos_calculated",
        "Calculated chaos value contributed by hunger.");

    private static readonly Gauge ThirstChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_food_thirst_chaos_calculated",
        "Calculated chaos value contributed by thirst.");

    private static readonly Gauge ChargeChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_food_charge_chaos_calculated",
        "Calculated chaos value contributed by silicon charge levels.");


    protected override ChaosMetrics CalculateChaos(EntityUid metricUid, FoodMetricComponent component, CalculateChaosEvent args)
    {
        // Gather hunger and thirst scores
        var query = EntityQueryEnumerator<MindContainerComponent, MobStateComponent>();
        double hungerSc = 0;
        double thirstSc = 0;
        double chargeSc = 0;

        var hungerCounts = new Dictionary<HungerThreshold, int>();
        var thirstCounts = new Dictionary<ThirstThreshold, int>();
        var chargeCounts = new Dictionary<string, int>() { {"Critical", 0}, {"Low", 0}, {"Mid", 0} };

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
            thirstSc = CalculateThirst(component, thirstQ, uid, thirstSc, thirstCounts);
            hungerSc = CalculateHunger(component, hungerQ, uid, hungerSc, hungerCounts);
            chargeSc = CalculateCharge(component, siliconQ, uid, chargeSc, chargeCounts);
        }

        foreach (var threshold in Enum.GetValues<HungerThreshold>())
            HungerThresholdCount.WithLabels(threshold.ToString()).Set(hungerCounts.GetValueOrDefault(threshold));
        foreach (var threshold in Enum.GetValues<ThirstThreshold>())
            ThirstThresholdCount.WithLabels(threshold.ToString()).Set(thirstCounts.GetValueOrDefault(threshold));
        foreach (var kvp in chargeCounts)
            SiliconChargeStateCount.WithLabels(kvp.Key).Set(kvp.Value);

        HungerChaosCalculated.Set(hungerSc);
        ThirstChaosCalculated.Set(thirstSc);
        ChargeChaosCalculated.Set(chargeSc);


        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, double>()
        {
            {ChaosMetric.Hunger, hungerSc},
            {ChaosMetric.Thirst, thirstSc},
            {ChaosMetric.Charge, chargeSc},
        });
        return chaos;
    }

    private double CalculateCharge(FoodMetricComponent component,
        EntityQuery<SiliconComponent> siliconQ,
        EntityUid uid,
        double chargeSc,
        Dictionary<string, int> chargeCounts)
    {
        if (!siliconQ.TryGetComponent(uid, out var silicon))
            return chargeSc;
        var chargeStateValue = GetChargeState(silicon.ChargeState);
        var chargeStateLabel = GetChargeStateLabel(chargeStateValue); // Get string label
        chargeSc += component.ChargeScores.GetValueOrDefault(chargeStateValue).Double();
        chargeCounts[chargeStateLabel]++;

        return chargeSc;
    }

    private static double CalculateHunger(FoodMetricComponent component, EntityQuery<HungerComponent> hungerQ, EntityUid uid,
        double hungerSc, Dictionary<HungerThreshold, int> hungerCounts)
    {
        if (!hungerQ.TryGetComponent(uid, out var hunger))
            return hungerSc;
        var threshold = hunger.CurrentThreshold;
        hungerSc += component.HungerScores.GetValueOrDefault(threshold).Double();
        hungerCounts[threshold] = hungerCounts.GetValueOrDefault(threshold) + 1;
        return hungerSc;
    }

    private static double CalculateThirst(FoodMetricComponent component,
        EntityQuery<ThirstComponent> thirstQ,
        EntityUid uid, double thirstSc,
        Dictionary<ThirstThreshold, int> thirstCounts)
    {
        if (!thirstQ.TryGetComponent(uid, out var thirst))
            return thirstSc;
        var threshold = thirst.CurrentThirstThreshold;
        thirstSc += component.ThirstScores.GetValueOrDefault(threshold).Double();
        thirstCounts[threshold] = thirstCounts.GetValueOrDefault(threshold) + 1;

        return thirstSc;
    }

    private static float GetChargeState(short chargeState)
    {
        const float mid = 0.5f;
        const float low = 0.25f;
        const float critical = 0.1f;

        var normalizedCharge = chargeState / 10f; // Assuming ChargeState is from 0-10

        return normalizedCharge switch
        {
            <= critical => critical,
            <= low => low,
            _ => mid,
        };
    }

    private static string GetChargeStateLabel(float chargeStateValue)
    {
        const float low = 0.25f;
        const float critical = 0.1f;

        return chargeStateValue switch
        {
            <= critical => "Critical",
            <= low => "Low",
            _ => "Mid",
        };
    }
}
