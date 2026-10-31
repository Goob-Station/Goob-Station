// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Station.Components;
using Content.Shared.GameTicking;
using Content.Shared.Station.Components;

namespace Content.Goobstation.Server.StationEvents.Metric;

/// <summary>
///   Base class for systems which measure chaos.
///   Chaos (in ChaosMetrics) is used by the GameDirector to decide which event should run next
///   Subclasses can either calculate chaos in that instant or subscribe to events to track state
///   over time in their component.
/// </summary>
public abstract class ChaosMetricSystem<T> : EntitySystem where T : Component
{
    protected abstract ChaosMetrics CalculateChaos(EntityUid uid, T component, CalculateChaosEvent args);

    protected virtual void ResetMetrics()
    {
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<T, CalculateChaosEvent>(OnCalculateChaos);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => ResetMetrics());
    }

    private void OnCalculateChaos(EntityUid uid, T component, ref CalculateChaosEvent args)
    {
        var ourChaos = CalculateChaos(uid, component, args);

        args.Metrics += ourChaos;
    }

    protected HashSet<EntityUid> GetEventStationGrids()
    {
        var grids = new HashSet<EntityUid>();
        var query = EntityQueryEnumerator<StationEventEligibleComponent, StationDataComponent>();
        while (query.MoveNext(out _, out _, out var data))
            grids.UnionWith(data.Grids);

        return grids;
    }

    protected static bool OnGrids(TransformComponent xform, HashSet<EntityUid> grids)
    {
        return xform.GridUid is { } grid && grids.Contains(grid);
    }
}
