using Content.Shared.Elevator;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Serilog;
using System.Linq;

namespace Content.Goobstation.Server.Elevator;

public sealed class ElevatorSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ElevatorControllerComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<ElevatorControllerComponent, ElevatorGoToFloorMessage>(OnGoToFloor);
    }

    private void OnUiOpened(
        EntityUid uid,
        ElevatorControllerComponent component,
        BoundUIOpenedEvent args)
    {
        var floors = component.Floors
        .Select(f => new ElevatorFloorUiData(f.Id, f.Name))
        .ToList();

        _uiSystem.SetUiState(
            uid,
            args.UiKey,
            new ElevatorBuiState(component.CurrentFloor, floors));
    }

    private void OnGoToFloor(
    EntityUid uid,
    ElevatorControllerComponent component,
    ref ElevatorGoToFloorMessage message)
    {
        var user = message.Actor;

        var floorId = message.FloorId;

        var floor = component.Floors.FirstOrDefault(f => f.Id == floorId);
        if (floor == null)
            return;

        if (!_mapLoader.TryLoadMap(
                floor.MapPath,
                out _,
                out var roots,
                new DeserializationOptions { InitializeMaps = true }))
            return;

        EntityCoordinates targetCoords = default;
        var hasTarget = false;

        if (floor.Target.AnchorEntity is { } anchor &&
            TryComp<TransformComponent>(anchor, out var anchorXform))
        {
            targetCoords = anchorXform.Coordinates;
            hasTarget = true;
        }
        else if (floor.Target.Coordinates is { } coords)
        {
            targetCoords = new EntityCoordinates(roots.First(), coords);
            hasTarget = true;
        }
        else if (floor.Target.Prototype is { } proto)
        {
            foreach (var (xform, meta) in EntityManager.EntityQuery<TransformComponent, MetaDataComponent>())
            {
                if (meta.EntityPrototype?.ID != proto.Id)
                    continue;

                targetCoords = xform.Coordinates;
                hasTarget = true;
                break;
            }
        }

        if (!hasTarget)
            return;

        _transform.SetCoordinates(user, targetCoords);
        component.CurrentFloor = floor.Id;
    }

}

