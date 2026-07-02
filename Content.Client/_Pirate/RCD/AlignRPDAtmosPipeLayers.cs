using Content.Client.Gameplay;
using Content.Client.Hands.Systems;
using Content.Client._Pirate.Plumbing;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.RCD;
using Content.Shared.RCD.Components;
using Content.Shared.RCD.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Placement;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Numerics;
using static Robust.Client.Placement.PlacementManager;

namespace Content.Client._Pirate.RCD;

/// <summary>
/// Allows RPD/RPLD placement previews to select atmos/plumbing pipe layers by cursor position.
/// </summary>
public sealed class AlignRPDAtmosPipeLayers : PlacementMode
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IEntityNetworkManager _entityNetwork = default!;

    private readonly SharedMapSystem _mapSystem;
    private readonly SharedTransformSystem _transformSystem;
    private readonly SharedAtmosPipeLayersSystem _pipeLayersSystem;
    private readonly SpriteSystem _spriteSystem;
    private readonly RCDSystem _rcdSystem;
    private readonly HandsSystem _handsSystem;
    private readonly PlumbingConnectorAppearanceSystem _plumbingConnectorAppearanceSystem;

    private const float SearchBoxSize = 2f;
    private const float MouseDeadzoneRadius = 0.25f;
    private const float PlaceColorBaseAlpha = 0.5f;
    private const float GuideRadius = 0.1f;
    private const float GuideOffset = 0.21875f;

    private EntityCoordinates _mouseCoordsRaw = default;
    private AtmosPipeLayer _currentLayer = AtmosPipeLayer.Primary;
    private EntityUid? _lastLayerSyncEntity;
    private AtmosPipeLayer? _lastLayerSynced;
    private readonly Color _guideColor = new(0, 0, 0.5785f);

    public AlignRPDAtmosPipeLayers(PlacementManager pMan) : base(pMan)
    {
        IoCManager.InjectDependencies(this);
        _mapSystem = _entityManager.System<SharedMapSystem>();
        _transformSystem = _entityManager.System<SharedTransformSystem>();
        _pipeLayersSystem = _entityManager.System<SharedAtmosPipeLayersSystem>();
        _spriteSystem = _entityManager.System<SpriteSystem>();
        _rcdSystem = _entityManager.System<RCDSystem>();
        _handsSystem = _entityManager.System<HandsSystem>();
        _plumbingConnectorAppearanceSystem = _entityManager.System<PlumbingConnectorAppearanceSystem>();

        ValidPlaceColor = ValidPlaceColor.WithAlpha(PlaceColorBaseAlpha);
    }

    public override void Render(in OverlayDrawArgs args)
    {
        if (_playerManager.LocalSession?.AttachedEntity is not { } player ||
            !_entityManager.TryGetComponent<TransformComponent>(player, out var xform) ||
            !_transformSystem.InRange(xform.Coordinates, MouseCoords, SharedInteractionSystem.InteractionRange))
        {
            return;
        }

        var gridUid = _transformSystem.GetGrid(MouseCoords);

        if (gridUid == null || !_entityManager.TryGetComponent<MapGridComponent>(gridUid, out var grid))
            return;

        if (!_handsSystem.TryGetActiveItem(player, out var heldEntity) ||
            !_entityManager.TryGetComponent<RCDComponent>(heldEntity, out var rcd))
        {
            return;
        }

        if (rcd.CurrentMode == RpdMode.Free && pManager.PlacementType == PlacementTypes.None)
        {
            var gridRotation = _transformSystem.GetWorldRotation(gridUid.Value);
            var worldPosition = _mapSystem.LocalToWorld(gridUid.Value, grid, MouseCoords.Position);
            var direction = (_eyeManager.CurrentEye.Rotation + gridRotation + Math.PI / 2).GetCardinalDir();
            var multi = direction is Direction.North or Direction.South ? -1f : 1f;

            args.WorldHandle.DrawCircle(worldPosition, GuideRadius, _guideColor);
            args.WorldHandle.DrawCircle(worldPosition + gridRotation.RotateVec(new Vector2(multi * GuideOffset, GuideOffset)), GuideRadius, _guideColor);
            args.WorldHandle.DrawCircle(worldPosition - gridRotation.RotateVec(new Vector2(multi * GuideOffset, GuideOffset)), GuideRadius, _guideColor);
        }

        base.Render(args);
    }

    public override void AlignPlacementMode(ScreenCoordinates mouseScreen)
    {
        _mouseCoordsRaw = ScreenToCursorGrid(mouseScreen);
        MouseCoords = _mouseCoordsRaw.AlignWithClosestGridTile(SearchBoxSize, _entityManager, _mapManager);

        var gridId = _transformSystem.GetGrid(MouseCoords);

        if (gridId is not { } gridUid ||
            !_entityManager.TryGetComponent<MapGridComponent>(gridUid, out var mapGrid))
            return;

        CurrentTile = _mapSystem.GetTileRef(gridUid, mapGrid, MouseCoords);

        var tileSize = mapGrid.TileSize;
        GridDistancing = tileSize;

        var tileCenter = _mapSystem.GridTileToLocal(gridUid, mapGrid, CurrentTile.GridIndices);
        MouseCoords = pManager.CurrentPermission!.IsTile
            ? tileCenter
            : tileCenter.WithPosition(tileCenter.Position + new Vector2(pManager.PlacementOffset.X, pManager.PlacementOffset.Y));

        var player = _playerManager.LocalSession?.AttachedEntity;
        if (player == null)
            return;

        if (!_handsSystem.TryGetActiveItem(player.Value, out var heldEntity))
            return;

        if (!_entityManager.TryGetComponent<RCDComponent>(heldEntity, out var rcd) || (!rcd.IsRpd && !rcd.IsRPLD))
            return;

        if (!_entityManager.TryGetComponent<TransformComponent>(player.Value, out var playerXform))
            return;

        if (!_transformSystem.InRange(playerXform.Coordinates, MouseCoords, SharedInteractionSystem.InteractionRange))
            return;

        var newLayer = GetLayerForMode(rcd, gridUid);

        if (newLayer != _currentLayer)
            _currentLayer = newLayer;

        if (rcd.CurrentMode == RpdMode.Free)
            UpdateSelectedLayer(heldEntity.Value, _currentLayer);

        UpdatePlacer(_currentLayer);
    }

    private AtmosPipeLayer GetLayerForMode(RCDComponent rcd, EntityUid gridId)
    {
        return rcd.CurrentMode switch
        {
            RpdMode.Primary => AtmosPipeLayer.Primary,
            RpdMode.Secondary => AtmosPipeLayer.Secondary,
            RpdMode.Tertiary => AtmosPipeLayer.Tertiary,
            RpdMode.Quaternary => AtmosPipeLayer.Tertiary, // Pirate: local atmos/plumbing supports three pipe layers.
            RpdMode.Quinary => AtmosPipeLayer.Tertiary,
            RpdMode.Free => GetFreeModeLayer(gridId),
            _ => AtmosPipeLayer.Primary,
        };
    }

    private AtmosPipeLayer GetFreeModeLayer(EntityUid gridId)
    {
        var mouseCoordsDiff = _mouseCoordsRaw.Position - MouseCoords.Position;

        if (mouseCoordsDiff.Length() <= MouseDeadzoneRadius)
            return AtmosPipeLayer.Primary;

        var gridRotation = _transformSystem.GetWorldRotation(gridId);
        var direction = (new Angle(mouseCoordsDiff) + _eyeManager.CurrentEye.Rotation + gridRotation + Math.PI / 2).GetCardinalDir();

        return direction is Direction.North or Direction.East
            ? AtmosPipeLayer.Secondary
            : AtmosPipeLayer.Tertiary;
    }

    private void UpdateSelectedLayer(EntityUid heldEntity, AtmosPipeLayer layer)
    {
        if (_lastLayerSyncEntity == heldEntity && _lastLayerSynced == layer)
            return;

        _lastLayerSyncEntity = heldEntity;
        _lastLayerSynced = layer;
        _entityNetwork.SendSystemNetworkMessage(new RPDSelectedLayerEvent(_entityManager.GetNetEntity(heldEntity), (byte) layer));
    }

    private void UpdatePlacer(AtmosPipeLayer layer)
    {
        if (pManager.CurrentPermission?.EntityType == null)
            return;

        if (!_protoManager.TryIndex<EntityPrototype>(pManager.CurrentPermission.EntityType, out var currentProto))
            return;

        if (!currentProto.TryGetComponent<AtmosPipeLayersComponent>(out var atmosPipeLayers, _entityManager.ComponentFactory))
            return;

        if (!_pipeLayersSystem.TryGetAlternativePrototype(atmosPipeLayers, layer, out var newProtoId))
            return;

        if (!_protoManager.TryIndex<EntityPrototype>(newProtoId, out var newProto))
            return;

        pManager.CurrentPermission.EntityType = newProtoId;

        if (!newProto.TryGetComponent<SpriteComponent>(out var sprite, _entityManager.ComponentFactory))
            return;

        var textures = new List<IDirectionalTextureProvider>();

        foreach (var spriteLayer in sprite.AllLayers)
        {
            if (spriteLayer.ActualRsi?.Path != null && spriteLayer.RsiState.Name != null)
                textures.Add(_spriteSystem.RsiStateLike(new SpriteSpecifier.Rsi(spriteLayer.ActualRsi.Path, spriteLayer.RsiState.Name)));
        }

        pManager.CurrentTextures = textures;

        if (newProto.TryGetComponent<PlumbingConnectorAppearanceComponent>(out var plumbingConnector, _entityManager.ComponentFactory) &&
            plumbingConnector.PreviewNodeDirections != Content.Shared.Atmos.PipeDirection.None &&
            pManager.CurrentPlacementOverlayEntity is { } overlay &&
            _entityManager.TryGetComponent<SpriteComponent>(overlay, out var overlaySprite))
        {
            _plumbingConnectorAppearanceSystem.ApplyPlacementPreview(overlay, plumbingConnector, overlaySprite);
        }
    }

    public override bool IsValidPosition(EntityCoordinates position)
    {
        var player = _playerManager.LocalSession?.AttachedEntity;

        if (!_entityManager.TryGetComponent<TransformComponent>(player, out var xform))
            return false;

        if (!_transformSystem.InRange(xform.Coordinates, position, SharedInteractionSystem.InteractionRange))
        {
            InvalidPlaceColor = InvalidPlaceColor.WithAlpha(0);
            return false;
        }

        InvalidPlaceColor = InvalidPlaceColor.WithAlpha(PlaceColorBaseAlpha);

        if (!_handsSystem.TryGetActiveItem(player.Value, out var heldEntity))
            return false;

        if (!_entityManager.TryGetComponent<RCDComponent>(heldEntity, out var rcd))
            return false;

        var gridUid = _transformSystem.GetGrid(position);
        if (gridUid is not { } grid ||
            !_entityManager.TryGetComponent<MapGridComponent>(grid, out var mapGrid))
            return false;

        var tile = _mapSystem.GetTileRef(grid, mapGrid, position);
        var posVector = _mapSystem.TileIndicesFor(grid, mapGrid, position);

        if (_stateManager.CurrentState is not GameplayStateBase screen)
            return false;

        var target = screen.GetClickedEntity(_transformSystem.ToMapCoordinates(_mouseCoordsRaw));

        return _rcdSystem.IsRCDOperationStillValid(heldEntity.Value, rcd, grid, mapGrid, tile, posVector, target, player.Value, false);
    }
}
