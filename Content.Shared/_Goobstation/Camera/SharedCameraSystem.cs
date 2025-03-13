using System.Numerics;
using Content.Shared.Coordinates;
using Content.Shared.Directions;
using Content.Shared.Interaction.Events;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;

namespace Content.Shared._Goobstation.Camera;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedCameraSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly INetManager _netManager = default!;
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
        if (args.Handled)
            return;

        CopyEntitiesInFrontOfPlayer(args.User);
        args.Handled = true;
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
        var boxSize = new Vector2(3f, 3f); // 3×3 area
        var boxOffset = new Vector2(2f, 0f); // Offset in front of the player

        var rotation = _transformSystem.GetWorldRotation(playerEntity).ToWorldVec();
        var boxCenter = _transformSystem.GetWorldPosition(playerEntity) + (boxOffset * rotation);
        var grid = _transformSystem.GetGrid(playerEntity);
        if(grid == null)
            return;
        var collisionBox = new Box2(boxCenter - (boxSize / 2), boxCenter + (boxSize / 2));
        var newMapGridId = _mapManager.CreateGrid(mapId);
        CopyTiles(grid.Value, boxCenter - (boxSize / 2), boxCenter + (boxSize / 2), newMapGridId);
        var intersecting = _lookupSystem.GetEntitiesIntersecting(grid.Value, collisionBox);
        foreach (var entity in intersecting)
        {
            Log.Info(entity.ToString());
            areaEntities.Add(entity);
        }

        foreach (var entity in areaEntities)
        {
            var relativePos = _transformSystem.GetWorldPosition(entity) - originTile + new Vector2(1, 1);

            var prototype = GetEntityData(GetNetEntity(entity)).Item2.EntityPrototype;
            if (prototype != null)
            {
                var newEntity = Spawn(prototype.ID, new EntityCoordinates(newMapUId, relativePos));

                // need to strip components.
                // depends on exactly what needs to be saved.
            }

        }
    }
    private void CopyTiles(EntityUid gridUid, Vector2 areaStart, Vector2 areaEnd, EntityUid newGrid)
    {
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        if (!TryComp<MapGridComponent>(newGrid, out var newGridComp))
            return;

        for (var x = areaStart.X; x <= areaEnd.X; x++)
        {
            for (var y = areaStart.Y; y <= areaEnd.Y; y++)
            {
                var tilePos = new Vector2i(x, y);
                var tileRef = _mapSystem.GetTileRef((gridUid, grid), tilePos);

                if (tileRef.Tile.IsEmpty)
                    continue; // Skip empty tiles

                // Copy tile (including lattices, floors, and walls)
                _mapSystem.SetTile((newGrid, newGridComp), tilePos, tileRef.Tile);
            }
        }
    }


}
