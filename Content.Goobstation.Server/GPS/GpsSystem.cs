using Content.Shared.Interaction;
using Content.Shared.Pinpointer;
using Content.Goobstation.Shared.GPS;
using Content.Goobstation.Shared.GPS.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using System.Linq;
using Content.Server.UserInterface;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.GPS;

public sealed class GpsSystem : SharedGpsSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GPSComponent, ActivateInWorldEvent>(OnGpsActivate);
        SubscribeLocalEvent<GPSComponent, BoundUIOpenedEvent>(OnGpsBuiOpened);
        SubscribeLocalEvent<GPSComponent, GpsSetTrackedEntityMessage>(OnSetTrackedEntity);
        SubscribeLocalEvent<GPSComponent, GpsSetGpsNameMessage>(OnSetGpsName);
        SubscribeLocalEvent<GPSComponent, GpsSetInDistressMessage>(OnSetInDistress);
    }

    private float _updateTimer;
    private const float UpdateRate = 1f;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;
        if (_updateTimer < UpdateRate)
            return;

        _updateTimer -= UpdateRate;

        var allEntries = GetGpsEntries();
        var activeGpsQuery = AllEntityQuery<GPSComponent, ActiveUserInterfaceComponent, TransformComponent>();

        while (activeGpsQuery.MoveNext(out var uid, out var gps, out var _, out var xform))
        {
            var filteredEntries = allEntries.Where(e => e.NetEntity != GetNetEntity(uid)).ToList();
            var entriesMsg = new GpsEntriesChangedMessage(filteredEntries);
            _userInterfaceSystem.ServerSendUiMessage(uid, GpsUiKey.Key, entriesMsg);

            if (gps.TrackedEntity is { } tracked)
            {
                var trackedUid = GetEntity(tracked);
                if (Exists(trackedUid))
                {
                    var coords = _transform.GetMapCoordinates(trackedUid);
                    var coordMsg = new GpsUpdateTrackedCoordinatesMessage(GetNetEntity(trackedUid), coords);
                    _userInterfaceSystem.ServerSendUiMessage(uid, GpsUiKey.Key, coordMsg);
                }
            }
        }
    }

    private void OnGpsActivate(EntityUid uid, GPSComponent component, ActivateInWorldEvent args)
    {
        if (HasComp<ActiveUserInterfaceComponent>(uid))
            return;

        _userInterfaceSystem.TryOpenUi(uid, GpsUiKey.Key, args.User);
    }

    private void OnGpsBuiOpened(EntityUid uid, GPSComponent component, BoundUIOpenedEvent args)
    {
        var ownCoords = _transform.GetMapCoordinates(uid);
        var entries = GetGpsEntries();
        var state = new GpsBoundUserInterfaceState(
            component.GpsName,
            component.InDistress,
            component.TrackedEntity,
            ownCoords,
            entries.Where(e => e.NetEntity != GetNetEntity(uid)).ToList());

        _userInterfaceSystem.SetUiState(uid, GpsUiKey.Key, state);
    }

    private void OnSetTrackedEntity(EntityUid uid, GPSComponent component, GpsSetTrackedEntityMessage args)
    {
        component.TrackedEntity = args.NetEntity;
        Dirty(uid, component);
        _userInterfaceSystem.ServerSendUiMessage(uid, GpsUiKey.Key, new GpsTrackedEntityChangedMessage(args.NetEntity));
    }

    private void OnSetGpsName(EntityUid uid, GPSComponent component, GpsSetGpsNameMessage args)
    {
        component.GpsName = args.GpsName;
        Dirty(uid, component);
        _userInterfaceSystem.ServerSendUiMessage(uid, GpsUiKey.Key, new GpsNameChangedMessage(args.GpsName));
    }

    private void OnSetInDistress(EntityUid uid, GPSComponent component, GpsSetInDistressMessage args)
    {
        component.InDistress = args.InDistress;
        Dirty(uid, component);
        _userInterfaceSystem.ServerSendUiMessage(uid, GpsUiKey.Key, new GpsDistressChangedMessage(args.InDistress));
    }

    private List<GpsEntry> GetGpsEntries()
    {
        var entries = new List<GpsEntry>();
        var gpsQuery = EntityQueryEnumerator<GPSComponent, TransformComponent>();
        while (gpsQuery.MoveNext(out var otherUid, out var otherGps, out var otherTransform))
        {
            var displayName = string.IsNullOrEmpty(otherGps.GpsName) ? $"GPS ({GetNetEntity(otherUid)})" : otherGps.GpsName;
            entries.Add(new GpsEntry
            {
                NetEntity = GetNetEntity(otherUid),
                Name = displayName,
                IsDistress = otherGps.InDistress,
                Color = Color.White,
                PrototypeId = MetaData(otherUid).EntityPrototype?.ID,
                Coordinates = _transform.GetMapCoordinates(otherUid, otherTransform)
            });
        }

        var beaconQuery = EntityQueryEnumerator<NavMapBeaconComponent, TransformComponent>();
        while (beaconQuery.MoveNext(out var beaconUid, out var beacon, out var beaconTransform))
        {
            if (!beacon.Enabled)
                continue;

            entries.Add(new GpsEntry
            {
                NetEntity = GetNetEntity(beaconUid),
                Name = beacon.Text ?? "Beacon",
                IsDistress = false,
                Color = beacon.Color,
                PrototypeId = MetaData(beaconUid).EntityPrototype?.ID,
                Coordinates = _transform.GetMapCoordinates(beaconUid, beaconTransform)
            });
        }

        return entries;
    }

}
