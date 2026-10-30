using Content.Goobstation.Server.StationEvents.Metric.Components;
using Content.Server.Spreader;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Prometheus;

namespace Content.Goobstation.Server.StationEvents.Metric;

public sealed partial class InfestationMetricSystem : ChaosMetricSystem<InfestationMetricComponent>
{
    [Dependency] private NpcFactionSystem _faction = default!;

    private static readonly Gauge KudzuTotal = Metrics.CreateGauge(
        "game_director_metric_kudzu_total",
        "Total number of kudzu patches on station grids.");

    private static readonly Gauge VerminTotal = Metrics.CreateGauge(
        "game_director_metric_vermin_total",
        "Alive mindless vermin on station grids.");

    private static readonly Gauge InfestationChaosCalculated = Metrics.CreateGauge(
        "game_director_metric_infestation_chaos_calculated",
        "Calculated chaos value contributed by kudzu and vermin.");

    protected override ChaosMetrics CalculateChaos(EntityUid uid, InfestationMetricComponent component, CalculateChaosEvent args)
    {
        var stationGrids = GetEventStationGrids();
        var kudzu = 0;
        var vermin = 0;

        var kudzuQuery = EntityQueryEnumerator<KudzuComponent, TransformComponent>();
        while (kudzuQuery.MoveNext(out _, out _, out var xform))
        {
            if (OnGrids(xform, stationGrids))
                kudzu++;
        }

        var mindQuery = GetEntityQuery<MindContainerComponent>();
        var mobQuery = EntityQueryEnumerator<NpcFactionMemberComponent, MobStateComponent, TransformComponent>();
        while (mobQuery.MoveNext(out var mob, out var faction, out var mobState, out var xform))
        {
            if (mobState.CurrentState != MobState.Alive
                || !OnGrids(xform, stationGrids)
                || mindQuery.TryGetComponent(mob, out var mind) && mind.Mind != null)
                continue;

            if (_faction.IsMemberOfAny((mob, faction), component.VerminFactions))
                vermin++;
        }

        var infestationChaos = kudzu * component.KudzuCost + vermin * component.VerminCost;

        KudzuTotal.Set(kudzu);
        VerminTotal.Set(vermin);
        InfestationChaosCalculated.Set(infestationChaos);

        return new ChaosMetrics(new Dictionary<ChaosMetric, double>
        {
            { ChaosMetric.Infestation, infestationChaos },
        });
    }

    protected override void ResetMetrics()
    {
        KudzuTotal.Set(0);
        VerminTotal.Set(0);
        InfestationChaosCalculated.Set(0);
    }
}
