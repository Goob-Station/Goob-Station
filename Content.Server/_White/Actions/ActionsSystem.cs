using Content.Server.DoAfter;
using Content.Shared._White.Actions.Events;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Actions;

public sealed class ActionsSystem : EntitySystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SpawnTileEntityActionEvent>(OnSpawnTileEntityAction);
        SubscribeLocalEvent<PlaceTileEntityEvent>(OnPlaceTileEntityEvent);

        SubscribeLocalEvent<PlaceTileEntityDoAfterEvent>(OnPlaceTileEntityDoAfter);
    }

    private void OnSpawnTileEntityAction(SpawnTileEntityActionEvent args)
    {
        if (args.Handled || !CreationTileEntity(args.Performer.ToCoordinates(), args.TileId, args.Entity, args.Audio))
            return;

        args.Handled = true;
    }

    private void OnPlaceTileEntityEvent(PlaceTileEntityEvent args)
    {
        if (args.Handled)
            return;

        if (args.Length != 0)
        {
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

        if (!CreationTileEntity(args.Target, args.TileId, args.Entity, args.Audio))
            return;

        args.Handled = true;
    }

    private void OnPlaceTileEntityDoAfter(PlaceTileEntityDoAfterEvent args)
    {
        if (args.Handled || !CreationTileEntity(GetCoordinates(args.Target), args.TileId, args.Entity, args.Audio))
            return;

        args.Handled = true;
    }

    #region Helpers

    private bool CreationTileEntity(EntityCoordinates coordinates, string? tileId, EntProtoId? entProtoId, SoundSpecifier? audio)
    {
        if (tileId != null)
        {
            if (_transform.GetGrid(coordinates) is not { } grid || !TryComp(grid, out MapGridComponent? mapGrid))
                return false;

            var tileDef = _tileDef[tileId];
            var tile = new Tile(tileDef.TileId);

            _mapSystem.SetTile(grid, mapGrid, coordinates, tile);
        }

        if (entProtoId != null)
            Spawn(entProtoId, coordinates);

        _audio.PlayPvs(audio, coordinates);

        return true;
    }

    #endregion
}
