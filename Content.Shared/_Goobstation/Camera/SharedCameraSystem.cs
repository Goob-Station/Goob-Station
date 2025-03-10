using System.Numerics;
using Content.Shared.Coordinates;
using Content.Shared.Interaction.Events;
using Robust.Shared.Map;
using Robust.Shared.Physics;

namespace Content.Shared._Goobstation.Camera;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedCameraSystem : EntitySystem
{

    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<CameraComponent, UseInHandEvent>(OnCameraUseInHand);
    }

    private void OnCameraUseInHand(Entity<CameraComponent> ent, ref UseInHandEvent args)
    {
        var user = args.User;
        var userPos = user.ToCoordinates();
        var userRotation = _transformSystem.GetWorldRotation(user);
        var userCardinalDir = userRotation.GetCardinalDir();

        Vector2i centerOffset;

        switch (userCardinalDir)
        {
            case Direction.North:
                centerOffset = new Vector2i(0, 2);
                break;
            case (Direction.East):
                centerOffset = new Vector2i(2, 0);
                break;
            case Direction.South:
                centerOffset = new Vector2i(0, -2);
                break;
            case Direction.West:
                centerOffset = new Vector2i(-2, 0);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var centerTile = new EntityCoordinates(user, userPos.Position + centerOffset);

        var newMap = _mapSystem.CreateMap();
        var newMapId = new MapId();
        foreach (var map in _mapSystem.GetAllMapIds()) // i couldnt find a getmap id function lol
        {
            if (_mapSystem.GetMap(map) != newMap)
                continue;

            newMapId = map;
            break;
        }
        var newMapGrid = _mapManager.CreateGrid(newMapId);



        var copiedEntities = new List<EntityUid>();

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                var tileCoords = centerTile.Offset(new Vector2i(x, y));
                var entities = _lookupSystem.GetEntitiesIntersecting(tileCoords);

                foreach (var entity in entities)
                {
                    var newEntity = Spawn(Comp<MetaDataComponent>(entity).EntityPrototype.ID, newMapGrid.ToCoordinates(new Vector2(tileCoords.X, tileCoords.Y)));
                    copiedEntities.Add(newEntity);
                }
            }
        }

        foreach (var entity in copiedEntities)
        {
            var allComps = _entityManager.GetAllComponents(copiedEntities);
            foreach (var comp in allComps)
            {
                if(comp.Component is SpriteComponent spriteComponent)
            }
        }

    }
}
