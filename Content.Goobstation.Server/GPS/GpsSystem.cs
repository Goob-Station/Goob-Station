using Content.Shared.Pinpointer;
using Content.Goobstation.Shared.GPS;
using Content.Goobstation.Shared.GPS.Components;
using System.Linq;
using Content.Goobstation.Common.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.GPS;

public sealed class GpsSystem : SharedGpsSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public float UpdateRate { get; private set; } = 1f;

    private float _updateTimer;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GPSComponent, MapInitEvent>(OnGpsInit);
        Subs.CVar(_config, GoobCVars.GpsUpdateRate, f => UpdateRate = f, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;
        if (_updateTimer < UpdateRate)
            return;

        _updateTimer -= UpdateRate;

        var allEntries = GetGpsEntries();
        var activeGpsQuery = AllEntityQuery<GPSComponent, ActiveUserInterfaceComponent, TransformComponent>();

        while (activeGpsQuery.MoveNext(out var uid, out var gps, out _, out _))
        {
            gps.GpsEntries = allEntries.Where(e => e.NetEntity != GetNetEntity(uid)).ToList();
            DirtyField(uid, gps, nameof(GPSComponent.GpsEntries));

            if (gps.TrackedEntity is not { } tracked)
                continue;

            gps.TrackedEntity = tracked;
            DirtyField(uid, gps, nameof(GPSComponent.TrackedEntity));
        }
    }

    private void OnGpsInit(EntityUid uid, GPSComponent component, MapInitEvent args)
    {
        if (string.IsNullOrWhiteSpace(component.GpsName))
            component.GpsName = "GPS-" + _random.Next(1000, 9999);
    }

    private List<GpsEntry> GetGpsEntries()
    {
        var entries = new List<GpsEntry>();
        var gpsQuery = EntityQueryEnumerator<GPSComponent, TransformComponent>();
        while (gpsQuery.MoveNext(out var otherUid, out var otherGps, out var otherTransform))
        {
            if (!otherGps.Enabled)
                continue;

            var displayName = string.IsNullOrEmpty(otherGps.GpsName) ? "Unknown GPS" : otherGps.GpsName;
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
