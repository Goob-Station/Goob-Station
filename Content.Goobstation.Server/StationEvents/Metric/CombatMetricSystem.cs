// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Server.Station.Systems;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
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
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    public readonly string NanotrasenFactionId = "NanoTrasen";

    private static readonly Gauge HostileEntitiesTotal = Metrics.CreateGauge(
        "game_director_metric_combat_hostile_entities_total",
        "Total number of hostile entities counted.");

    private static readonly Gauge FriendlyEntitiesTotal = Metrics.CreateGauge(
        "game_director_metric_combat_friendly_entities_total",
        "Total number of alive friendly entities counted.");

    private static readonly Gauge DeadFriendlyEntitiesTotal = Metrics.CreateGauge(
        "game_director_metric_combat_dead_friendly_entities_total",
        "Total number of dead friendly entities counted.");

    private static readonly Gauge DeadHostileEntitiesTotal = Metrics.CreateGauge(
        "game_director_metric_combat_dead_hostile_entities_total",
        "Total number of dead hostile entities counted.");

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

    private double InventoryPower(EntityUid uid, CombatMetricComponent component)
    {
        // Iterate through items to determine how powerful the entity is
        // Having a good range of offensive items in your inventory makes you more dangerous
        var tagsQ = GetEntityQuery<TagComponent>();
        var allTags = new HashSet<ProtoId<TagPrototype>>();

        foreach (var item in _inventory.GetHandOrInventoryEntities(uid))
            if (tagsQ.TryGetComponent(item, out var tags)) // thanks code rabbit
                allTags.UnionWith(tags.Tags);

        var threat = allTags.Sum(key => component.ItemThreat.GetValueOrDefault(key));

        return threat > component.MaxItemThreat ? component.MaxItemThreat : threat;
    }

    protected override ChaosMetrics CalculateChaos(EntityUid metricUid,
        CombatMetricComponent combatMetric,
        CalculateChaosEvent args)
    {
        // Add up the pain of all the puddles
        var query = EntityQueryEnumerator<MobStateComponent, DamageableComponent, TransformComponent>();
        var mindQuery = GetEntityQuery<MindContainerComponent>();
        var npcFacQ = GetEntityQuery<NpcFactionMemberComponent>();
        var powerQ = GetEntityQuery<CombatPowerComponent>();
        double hostilesChaos = 0;
        double friendliesChaos = 0;
        double medicalChaos = 0;
        double deathChaos = 0;

        // Prometheus Metric Accumulators
        var hostileCount = 0;
        var friendlyCount = 0;
        var deadFriendlyCount = 0;
        var deadHostileCount = 0;
        var critFriendlyCount = 0;
        double hostileInventoryThreat = 0;
        double friendlyInventoryThreat = 0;


        // var humanoidQ = GetEntityQuery<HumanoidAppearanceComponent>();
        var stationGrids = _stationSystem.GetAllStationGrids();
        while (query.MoveNext(out var uid, out var mobState, out var damage, out var transform))
        {
            if (transform.GridUid == null || !stationGrids.Contains(transform.GridUid.Value))
                continue;

            var isAntag = false;

            if (mindQuery.TryGetComponent(uid, out var mind) && mind.Mind != null)
                isAntag = _roles.MindIsAntagonist(mind.Mind);
            else
            {
                if (!CalculateNPC(npcFacQ, uid, powerQ, ref isAntag))
                    continue;
            }

            powerQ.TryGetComponent(uid, out var power);
            var threatMultiple = power?.Threat ?? 1.0f;

            if (isAntag)
            {
                if (mobState.CurrentState != MobState.Alive)
                {
                    deadHostileCount++;
                    deathChaos += combatMetric.DeadScore;
                    continue;
                }
                hostileCount++;
            }
            else
            {
                if (!CalculateFriendlyCount(combatMetric, mobState, damage, ref deadFriendlyCount, ref deathChaos, ref friendlyCount, ref medicalChaos, ref critFriendlyCount))
                    continue;
            }

            // 4. Calculate Threat
            var entityThreat = InventoryPower(uid, combatMetric);

            if (isAntag)
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
        DeadHostileEntitiesTotal.Set(deadHostileCount);
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

    private bool CalculateNPC(
        EntityQuery<NpcFactionMemberComponent> npcFacQ,
        EntityUid uid,
        EntityQuery<CombatPowerComponent> powerQ,
        ref bool isAntag)
    {
        if (!npcFacQ.TryGetComponent(uid, out var fac))
            return true;

        var hasPower = powerQ.HasComponent(uid);
        var isHostile = _faction.IsFactionHostile(NanotrasenFactionId, (uid, fac));

        if (isHostile)
        {
            isAntag = true;
            return !hasPower;
        }

        if (!hasPower)
            return true;

        isAntag = false;
        return false;
    }

    private static bool CalculateFriendlyCount(CombatMetricComponent combatMetric,
        MobStateComponent mobState,
        DamageableComponent damage,
        ref int deadFriendlyCount,
        ref double deathChaos,
        ref int friendlyCount,
        ref double medicalChaos,
        ref int critFriendlyCount)
    {
        if (mobState.CurrentState == MobState.Dead)
        {
            deadFriendlyCount++;
            deathChaos += combatMetric.DeadScore;
            return false;
        }
        friendlyCount++;
        var totalDamage = damage.Damage.GetTotal().Double();
        medicalChaos += totalDamage * combatMetric.MedicalMultiplier;
        if (mobState.CurrentState != MobState.Critical)
            return true;
        critFriendlyCount++;
        medicalChaos += combatMetric.CritScore;

        return true;
    }
}
