using Content.Shared._RMC14.Tracker;
using Content.Shared.Alert;
using Content.Shared.Station;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Tracker.StationTracker;

public sealed partial class StationPointerAlertSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alert = default!;
    [Dependency] private readonly IPrototypeManager _proto  = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TrackerSystem _tracker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StationPointerAlertComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StationPointerAlertComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnMapInit(Entity<StationPointerAlertComponent> entity, ref MapInitEvent args)
    {
        entity.Comp.Station = TryGetStation(entity);
        Dirty(entity);
        UpdateDirection(entity);
    }

    private void OnComponentShutdown(Entity<StationPointerAlertComponent> entity, ref ComponentShutdown args)
    {
        _alert.ClearAlert(entity.Owner, entity.Comp.TrackerAlertProto);
    }

    private void UpdateDirection(Entity<StationPointerAlertComponent> entity, MapCoordinates? coordinates = null)
    {
        _proto.TryIndex(entity.Comp.TrackerAlertProto, out var alertProto);
        if (alertProto == null)
            return;

        var severity = TrackerSystem.CenterSeverity;
        if (coordinates != null)
            severity = _tracker.GetAlertSeverity(entity.Owner, coordinates.Value);

        _alert.ShowAlert(entity.Owner, entity.Comp.TrackerAlertProto, severity);
    }
    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<StationPointerAlertComponent>();
        while (query.MoveNext(out var uid, out var tracker))
        {
            if (curTime < tracker.NextUpdate)
                continue;

            tracker.Station = TryGetStation((uid, tracker));

            if (tracker.Station != null)
            {
                UpdateDirection((uid, tracker), _transform.GetMapCoordinates(tracker.Station.Value));
                continue;
            }

            tracker.NextUpdate = curTime + tracker.UpdateInterval;
        }
    }

    private EntityUid? TryGetStation(Entity<StationPointerAlertComponent> entity)
    {
        var xform = Transform(entity);

        EntityUid? stationGrid = null;
        if (_station.GetStationInMap(xform.MapID) is { } station)
            stationGrid = _station.GetLargestGrid(station);

        return stationGrid;
    }
}
