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


        args.Handled = TryCopy(args.User);
    }

    private bool TryCopy(EntityUid user)
    {
        return CopyTilesInFrontOfPlayer(user);
    }


    private bool CopyTilesInFrontOfPlayer(EntityUid user)
    {
        var transform = EntityManager.GetComponent<TransformComponent>(user);
        var direction = transform.LocalRotation.ToWorldVec();
        var origin = transform.Coordinates.Offset(direction);

        if (!_mapManager.TryFindGridAt(_transformSystem.GetMapCoordinates(user, transform), out var gridUid, out var gridComp) && gridComp == null)
        {
            Logger.Warning("No grid found at player location.");
            return false;
        }

        List<(Vector2i, Tile)> copiedTiles = [];

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                var tileCoords = new Vector2i((int) origin.Position.X + x, (int) origin.Position.Y + y);
                var tileRef = _mapSystem.GetTileRef((gridUid, gridComp), tileCoords);
                copiedTiles.Add((tileCoords, tileRef.Tile));
            }
        }

        EntityUid map = default!;
        if(_netManager.IsServer)
            map = _mapSystem.CreateMap();
        if (!TryComp<MapComponent>(map, out var mapComponent))
            return false;

        var photoGrid = _mapManager.CreateGridEntity(map);

        foreach (var (pos, tile) in copiedTiles)
        {
            _mapSystem.SetTile(photoGrid, pos, tile);
        }
        return true;
    }
}
