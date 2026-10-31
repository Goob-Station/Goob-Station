// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Prometheus;
using Content.Shared.Damage.Components;

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
///   Medical: 40 for crit + 0.2 * damage (so 20 for 100 damage),
/// </summary>
public sealed partial class CombatMetricSystem : ChaosMetricSystem<CombatMetricComponent>
{
    [Dependency] private SharedRoleSystem _roles = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedJobSystem _jobs = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private NpcFactionSystem _faction = default!;
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

    private static readonly Gauge SecurityStrengthTotal = Metrics.CreateGauge(
        "game_director_metric_combat_security_strength",
        "Combined strength of alive security members on station grids.");

    private static readonly Gauge SecurityChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_combat_security_chaos_calculated",
        "Hostile strength not matched by security.");

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
        var query = EntityQueryEnumerator<MobStateComponent, DamageableComponent, TransformComponent>();
        var mindQuery = GetEntityQuery<MindContainerComponent>();
        var npcFacQ = GetEntityQuery<NpcFactionMemberComponent>();
        var powerQ = GetEntityQuery<CombatPowerComponent>();
        double hostilesChaos = 0;
        double friendliesChaos = 0;
        double medicalChaos = 0;
        double deathChaos = 0;

        var hostileCount = 0;
        var friendlyCount = 0;
        var deadFriendlyCount = 0;
        var deadHostileCount = 0;
        var critFriendlyCount = 0;
        double hostileInventoryThreat = 0;
        double friendlyInventoryThreat = 0;
        double securityStrength = 0;

        var now = _timing.CurTime;
        var seenDead = new HashSet<EntityUid>();
        var stationGrids = GetEventStationGrids();
        while (query.MoveNext(out var uid, out var mobState, out var damage, out var transform))
        {
            if (!OnGrids(transform, stationGrids))
                continue;

            if (mobState.CurrentState != MobState.Alive && _container.IsEntityInContainer(uid))
            {
                if (mobState.CurrentState == MobState.Dead)
                    seenDead.Add(uid);
                continue;
            }

            bool isAntag;
            var isNpc = false;
            if (mindQuery.TryGetComponent(uid, out var mind) && mind.Mind != null)
                isAntag = _roles.MindIsAntagonist(mind.Mind);
            else if (IsHostileNpc(npcFacQ, uid, combatMetric))
                isAntag = isNpc = true;
            else
                continue;

            if (isAntag)
            {
                if (mobState.CurrentState != MobState.Alive)
                {
                    deadHostileCount++;
                    continue;
                }

                hostileCount++;
            }
            else if (mobState.CurrentState == MobState.Dead)
            {
                deadFriendlyCount++;
                seenDead.Add(uid);
                deathChaos += combatMetric.DeadScore * DeathFactor(combatMetric, uid, now);
                continue;
            }
            else
            {
                friendlyCount++;
                medicalChaos += damage.Damage.GetTotal().Double() * combatMetric.MedicalMultiplier;
                if (mobState.CurrentState == MobState.Critical)
                {
                    critFriendlyCount++;
                    medicalChaos += combatMetric.CritScore;
                }
            }

            var threatMultiple = powerQ.TryGetComponent(uid, out var power) ? power.Threat : 1.0;
            var entityThreat = InventoryPower(uid, combatMetric);

            if (isAntag)
            {
                hostileInventoryThreat += entityThreat;
                hostilesChaos += (entityThreat + (isNpc ? combatMetric.HostileNpcScore : combatMetric.HostileScore)) * threatMultiple;
            }
            else
            {
                var strength = (entityThreat + combatMetric.FriendlyScore) * threatMultiple;
                friendlyInventoryThreat += entityThreat;
                friendliesChaos += strength;

                if (mobState.CurrentState == MobState.Alive && IsSecurity(combatMetric, mind?.Mind))
                    securityStrength += strength;
            }
        }

        foreach (var tracked in combatMetric.DeathTimes.Keys.ToList())
        {
            if (!seenDead.Contains(tracked))
                combatMetric.DeathTimes.Remove(tracked);
        }

        var securityChaos = Math.Max(0, hostilesChaos - securityStrength);

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
        SecurityStrengthTotal.Set(securityStrength);
        SecurityChaosCalculated.Set(securityChaos);

        var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, double>()
        {
            {ChaosMetric.Friend, -friendliesChaos}, // Friendlies are good, so make a negative chaos score
            {ChaosMetric.Hostile, hostilesChaos},

            {ChaosMetric.Death, deathChaos},
            {ChaosMetric.Medical, medicalChaos},
            {ChaosMetric.Security, securityChaos},
        });
        return chaos;
    }

    protected override void ResetMetrics()
    {
        HostileEntitiesTotal.Set(0);
        FriendlyEntitiesTotal.Set(0);
        DeadFriendlyEntitiesTotal.Set(0);
        DeadHostileEntitiesTotal.Set(0);
        CritFriendlyEntitiesTotal.Set(0);
        HostileInventoryThreatTotal.Set(0);
        FriendlyInventoryThreatTotal.Set(0);
        HostileChaosCalculated.Set(0);
        FriendChaosCalculated.Set(0);
        MedicalChaosCalculated.Set(0);
        DeathChaosCalculated.Set(0);
        SecurityStrengthTotal.Set(0);
        SecurityChaosCalculated.Set(0);
    }

    private double DeathFactor(CombatMetricComponent combatMetric, EntityUid uid, TimeSpan now)
    {
        if (!combatMetric.DeathTimes.TryGetValue(uid, out var deathTime))
        {
            combatMetric.DeathTimes[uid] = now;
            return 1.0;
        }

        if (combatMetric.DeathFadeTime <= TimeSpan.Zero)
            return 1.0;

        var faded = 1.0 - (now - deathTime) / combatMetric.DeathFadeTime;
        return Math.Max(combatMetric.MinDeathFactor, faded);
    }

    private bool IsSecurity(CombatMetricComponent combatMetric, EntityUid? mindId)
    {
        return _jobs.MindTryGetJobId(mindId, out var jobId)
               && jobId is { } job
               && _jobs.TryGetPrimaryDepartment(job.Id, out var department)
               && department.ID == combatMetric.SecurityDepartment.Id;
    }

    private bool IsHostileNpc(EntityQuery<NpcFactionMemberComponent> npcFacQ, EntityUid uid, CombatMetricComponent combatMetric)
    {
        return npcFacQ.TryGetComponent(uid, out var fac)
               && _faction.IsFactionHostile(NanotrasenFactionId, (uid, fac))
               && !_faction.IsMemberOfAny((uid, fac), combatMetric.IgnoredHostileFactions);
    }
}
