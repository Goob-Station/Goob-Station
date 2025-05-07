// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Server.Station.Systems;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Roles;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Prometheus;

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Measures the strength of friendies and hostiles. Also calculates related health / death stats.
///
///   I've used 10 points per entity because later we might somehow estimate combat strength
///   as a multiplier. We could for instance detect damage delt / recieved and look also at
///   entity hitpoints & resistances as an analogue for danger.
///
///   Writes the following
///   Friend : -10 per each friendly entity on the station (negative is GOOD in chaos)
///   Hostile : about 10 points per hostile (those with antag roles) - varies per constants
///   Combat: friendlies + hostiles (to represent the balance of power)
///   Death: 20 per dead body,
///   Medical: 10 for crit + 0.05 * damage (so 5 for 100 damage),
/// </summary>
public sealed class CombatMetricSystem : ChaosMetricSystem<CombatMetricComponent>
{
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private static readonly Gauge HostileEntitiesTotal = Metrics.CreateGauge(
        "game_director_metric_combat_hostile_entities_total",
        "Total number of hostile entities counted.");

    private static readonly Gauge FriendlyEntitiesTotal = Metrics.CreateGauge(
        "game_director_metric_combat_friendly_entities_total",
        "Total number of alive friendly entities counted.");

    private static readonly Gauge DeadFriendlyEntitiesTotal = Metrics.CreateGauge(
        "game_director_metric_combat_dead_friendly_entities_total",
        "Total number of dead friendly entities counted.");

    private static readonly Gauge CritFriendlyEntitiesTotal = Metrics.CreateGauge(
        "game_director_metric_combat_crit_friendly_entities_total",
        "Total number of critical friendly entities counted.");

    private static readonly Gauge HostileInventoryThreatTotal = Metrics.CreateGauge(
        "game_director_metric_combat_hostile_inventory_threat_total",
        "Total calculated inventory threat for hostile entities.");

    private static readonly Gauge FriendlyInventoryThreatTotal = Metrics.CreateGauge(
        "game_director_metric_combat_friendly_inventory_threat_total",
        "Total calculated inventory threat for friendly entities.");

    private static readonly Gauge HostileChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_combat_hostile_chaos_calculated",
        "Calculated chaos value contributed by hostiles.");

    private static readonly Gauge FriendChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_combat_friend_chaos_calculated",
        "Calculated chaos value contributed by friendlies (positive value).");

    private static readonly Gauge MedicalChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_combat_medical_chaos_calculated",
        "Calculated chaos value contributed by medical state.");

    private static readonly Gauge DeathChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_combat_death_chaos_calculated",
        "Calculated chaos value contributed by deaths.");


    public double InventoryPower(EntityUid uid, CombatMetricComponent component)
    {
        // Iterate through items to determine how powerful the entity is
        // Having a good range of offensive items in your inventory makes you more dangerous
        double threat = 0;

        var tagsQ = GetEntityQuery<TagComponent>();
        var allTags = new HashSet<ProtoId<TagPrototype>>();

        foreach (var item in _inventory.GetHandOrInventoryEntities(uid))
        {
            if (tagsQ.TryGetComponent(item, out var tags)) // thanks code rabbit
            {
                allTags.UnionWith(tags.Tags);
            }
        }

        foreach (var key in allTags)
        {
            threat += component.ItemThreat.GetValueOrDefault(key);
        }

        if (threat > component.maxItemThreat)
            return component.maxItemThreat;

        return threat;
    }

    public override ChaosMetrics CalculateChaos(EntityUid metric_uid, CombatMetricComponent combatMetric,
        CalculateChaosEvent args)
    {
        // Add up the pain of all the puddles
        var query = EntityQueryEnumerator<MindContainerComponent, MobStateComponent, DamageableComponent, TransformComponent>();
        double hostilesChaos = 0;
        double friendliesChaos = 0;
        double medicalChaos = 0;
        double deathChaos = 0;

        // Prometheus Metric Accumulators
        int hostileCount = 0;
        int friendlyCount = 0;
        int deadFriendlyCount = 0;
        int critFriendlyCount = 0;
        double hostileInventoryThreat = 0;
        double friendlyInventoryThreat = 0;

        var powerQ = GetEntityQuery<CombatPowerComponent>();

        // var humanoidQ = GetEntityQuery<HumanoidAppearanceComponent>();
        var stationGrids = _stationSystem.GetAllStationGrids();

        while (query.MoveNext(out var uid, out var mind, out var mobState, out var damage, out var transform))
        {
            // Don't count anything that is mindless
            if (mind.Mind == null)
                continue;

            // Only count threats currently on station, which avoids salvage threats getting counted for instance.
            // Note this means for instance Nukies on nukie planet don't count, so the threat will spike when they arrive.
            if (transform.GridUid == null || !stationGrids.Contains(transform.GridUid.Value))
                // TODO: Check for NPCs here, they still count.
                continue;

            // Read per-entity scaling factor (for instance space dragon has much higher threat)
            powerQ.TryGetComponent(uid, out var power);
            var threatMultiple = power?.Threat ?? 1.0f;

            double entityThreat = 0;

            var antag = _roles.MindIsAntagonist(mind.Mind);
            if (antag)
            {
                if (mobState.CurrentState != MobState.Alive)
                    continue;

                hostileCount++;
            }
            else
            {
                // This is a friendly
                if (mobState.CurrentState == MobState.Dead)
                {
                    deadFriendlyCount++;
                    deathChaos += combatMetric.DeadScore;
                    continue;
                }
                else
                {
                    friendlyCount++;
                    var totalDamage = damage.Damage.GetTotal().Double();
                    medicalChaos += totalDamage * combatMetric.MedicalMultiplier;
                    if (mobState.CurrentState == MobState.Critical)
                    {
                        critFriendlyCount++;
                        medicalChaos += combatMetric.CritScore;
                    }
                }
            }

            // Iterate through items to determine how powerful the entity is
            entityThreat += InventoryPower(uid, combatMetric);

            if (antag)
            {
                hostileInventoryThreat += entityThreat;
                hostilesChaos += (entityThreat + combatMetric.HostileScore) * threatMultiple;
            }
            else
            {
                friendlyInventoryThreat += entityThreat;
                friendliesChaos += (entityThreat + combatMetric.FriendlyScore) * threatMultiple;
            }
        }

        HostileEntitiesTotal.Set(hostileCount);
        FriendlyEntitiesTotal.Set(friendlyCount);
        DeadFriendlyEntitiesTotal.Set(deadFriendlyCount);
        CritFriendlyEntitiesTotal.Set(critFriendlyCount);
        HostileInventoryThreatTotal.Set(hostileInventoryThreat);
        FriendlyInventoryThreatTotal.Set(friendlyInventoryThreat);
        HostileChaosCalculated.Set(hostilesChaos);
        FriendChaosCalculated.Set(friendliesChaos);
        MedicalChaosCalculated.Set(medicalChaos);
        DeathChaosCalculated.Set(deathChaos);


        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, double>()
        {
            {ChaosMetric.Friend, -friendliesChaos}, // Friendlies are good, so make a negative chaos score
            {ChaosMetric.Hostile, hostilesChaos},

            {ChaosMetric.Death, deathChaos},
            {ChaosMetric.Medical, medicalChaos},
        });
        return chaos;
    }
}
