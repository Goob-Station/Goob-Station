using Content.Shared._Lavaland.Elevator;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;

namespace Content.Server.Elevator;

/// <summary>
/// Teleport system that is basically a reskinned portal but that loads new grids.
/// </summary>
public sealed class ElevatorSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ElevatorComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<ElevatorComponent, StepTriggeredOffEvent>(OnStepTriggered);
    }

    private void OnStepTriggerAttempt(Entity<ElevatorComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnStepTriggered(EntityUid uid, ElevatorComponent comp, ref StepTriggeredOffEvent args)
    {
        if (TryGetBeaconCoords(comp.DestinationId, out var beaconCoords))
        {
            _transform.SetCoordinates(args.Tripper, beaconCoords);
            return;
        }

        if (comp.MapPath is null)
        {
            Log.Error($"Elevator {ToPrettyString(uid)} couldn't find beacon '{comp.DestinationId}' and has no MapPath to load.");
            return;
        }

        if (!_mapLoader.TryLoadMap(comp.MapPath.Value, out var map, out var roots, options: new DeserializationOptions { InitializeMaps = true }))
        {
            Log.Error($"ElevatorSystem didn't manage to load {comp.MapPath}");

            if (map is not null)
                QueueDel(map);

            return;
        }

        if (!TryGetBeaconCoords(comp.DestinationId, out beaconCoords))
        {
            Log.Error($"ElevatorSystem loaded {comp.MapPath} but still couldn't find beacon '{comp.DestinationId}'");
            return;
        }

        _transform.SetCoordinates(args.Tripper, beaconCoords);
    }

    private bool TryGetBeaconCoords(string destinationId, out EntityCoordinates coords)
    {
        coords = default;

        var query = EntityQueryEnumerator<ElevatorBeaconComponent, TransformComponent>();
        while (query.MoveNext(out _, out var beacon, out var xform))
        {
            if (beacon.Id != destinationId)
                continue;

            coords = xform.Coordinates;
            return true;
        }

        return false;
    }
}
