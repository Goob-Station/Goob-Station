using Content.Server.DoAfter;
using Content.Shared._White.Actions.Events;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Actions;

public sealed class ActionsSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpawnTileEntityActionEvent>(OnSpawnTileEntityAction);
        SubscribeLocalEvent<PlaceTileEntityEvent>(OnPlaceTileEntityEvent);

        SubscribeLocalEvent<PlaceTileEntityDoAfterEvent>(OnPlaceTileEntityDoAfter);
    }

    private void OnSpawnTileEntityAction(SpawnTileEntityActionEvent args)
    {
        if (args.Handled || !CreationTileEntity(args.Performer, args.Performer.ToCoordinates(), args.TileId, args.Entity, args.Audio, args.BlockedCollision))
            return;

        args.Handled = true;
    }

    private void OnPlaceTileEntityEvent(PlaceTileEntityEvent args)
    {
        if (args.Handled)
            return;

        if (args.Length != 0)
        {
            if (CheckTileBlocked(args.Target, args.BlockedCollision))
                return;

            var ev = new PlaceTileEntityDoAfterEvent
            {
                Target = GetNetCoordinates(args.Target),
                Entity = args.Entity,
                TileId = args.TileId,
                Audio = args.Audio
            };

            var doAfter = new DoAfterArgs(EntityManager, args.Performer, args.Length, ev, null)
            {
                BlockDuplicate = true,
                BreakOnDamage = true,
                CancelDuplicate = true,
                BreakOnMove = true,
                Broadcast = true
            };

            _doAfter.TryStartDoAfter(doAfter);
            return;
        }

        if (!CreationTileEntity(args.Performer, args.Target, args.TileId, args.Entity, args.Audio, args.BlockedCollision))
            return;

        args.Handled = true;
    }

    private void OnPlaceTileEntityDoAfter(PlaceTileEntityDoAfterEvent args)
    {
        if (args.Handled || !CreationTileEntity(args.User, GetCoordinates(args.Target), args.TileId, args.Entity, args.Audio, null))
            return;

        args.Handled = true;
    }

    #region Helpers

    private bool CreationTileEntity(EntityUid user, EntityCoordinates coordinates, string? tileId, EntProtoId? entProtoId, SoundSpecifier? audio, CollisionGroup? blockedCollision)
    {
        if (_container.IsEntityOrParentInContainer(user))
            return false;

        if (tileId != null)
        {
            if (_transform.GetGrid(coordinates) is not { } grid || !TryComp(grid, out MapGridComponent? mapGrid))
                return false;

            var tileDef = _tileDef[tileId];
            var tile = new Tile(tileDef.TileId);

            _mapSystem.SetTile(grid, mapGrid, coordinates, tile);
        }

        _audio.PlayPvs(audio, coordinates);

        if (entProtoId == null || CheckTileBlocked(coordinates, blockedCollision))
            return false;

        Spawn(entProtoId, coordinates);

        return true;
    }

    private bool CheckTileBlocked(EntityCoordinates coordinates, CollisionGroup? blockedCollision)
    {
        var tileRef = coordinates.GetTileRef(EntityManager, _mapManager);

        return tileRef.HasValue && blockedCollision.HasValue && _turf.IsTileBlocked(tileRef.Value, blockedCollision.Value);
    }

    #endregion
}
