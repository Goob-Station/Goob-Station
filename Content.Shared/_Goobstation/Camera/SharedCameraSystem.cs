using System.Numerics;
using Content.Shared.Coordinates;
using Content.Shared.Directions;
using Content.Shared.Interaction.Events;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;

namespace Content.Shared._Goobstation.Camera;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedCameraSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CameraComponent, UseInHandEvent>(OnUseCameraInHand);
    }

    private void OnUseCameraInHand(Entity<CameraComponent> ent, ref UseInHandEvent args)
    {
        CopyEntitiesInFrontOfPlayer(args.User);
    }


    public void CopyEntitiesInFrontOfPlayer(EntityUid playerEntity)
    {
        var playerPos = _transformSystem.GetWorldPosition(playerEntity);
        var playerRot = _transformSystem.GetWorldPosition(playerEntity);
        var direction = playerRot.GetDir();

        var originTile = playerPos + direction.ToVec() * 1.5f;
        var areaEntities = new List<EntityUid>();

        var newMapUId = _mapSystem.CreateMap();
        if(!TryComp<MapComponent>(newMapUId, out var mapComponent))
            return;
        var mapId = mapComponent.MapId;
        Log.Info("Map ID: " + mapId);


        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                var checkPos = originTile + new Vector2(x, y);
                var entitiesAtPos =
                    _lookupSystem.GetEntitiesIntersecting(new EntityCoordinates(playerEntity, checkPos));

                foreach (var entity in entitiesAtPos)
                {
                    // Skip the player if they're somehow in this area
                    if (entity == playerEntity)
                        continue;

                    areaEntities.Add(entity);
                }
            }
        }

        var newMapGridId = _mapManager.CreateGrid(mapId);
        foreach (var entity in areaEntities)
        {
            var entityTransform = _entityManager.GetComponent<TransformComponent>(entity);
            var relativePos = entityTransform.WorldPosition - originTile + new Vector2(1, 1);

            var prototype = GetEntityData(GetNetEntity(entity)).Item2.EntityPrototype;
            if (prototype != null)
            {
                var newEntity = Spawn(prototype.ID, new EntityCoordinates(newMapUId, relativePos));

                // need to strip components.
                // depends on exactly what needs to be saved.
            }

        }
    }


}
