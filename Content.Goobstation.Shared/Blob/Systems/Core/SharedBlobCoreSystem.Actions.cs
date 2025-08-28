using System.Linq;
using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.Events;
using Content.Shared.Actions;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Popups;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Blob.Systems.Core;

public abstract partial class SharedBlobCoreSystem
{
    private void InitializeActions()
    {
        SubscribeLocalEvent<BlobCoreComponent, BlobTransformTileActionEvent>(OnTileTransform);
        SubscribeLocalEvent<BlobCoreComponent, BlobToCoreActionEvent>(OnBlobToCore);
        SubscribeLocalEvent<BlobCoreComponent, BlobSwapCoreActionEvent>(OnSwapCore);
        SubscribeLocalEvent<BlobCoreComponent, BlobSplitCoreActionEvent>(OnSplitCore);
        SubscribeLocalEvent<BlobCoreComponent, BlobSwapChemActionEvent>(OnBlobSwapChem);

        SubscribeLocalEvent<BlobCoreComponent, BlobChemSwapPrototypeSelectedMessage>(OnChemSelected);
    }

    private void OnTileTransform(Entity<BlobCoreComponent> ent, ref BlobTransformTileActionEvent args)
    {
        if (!TryGetTargetBlobTile(args, out var blobTile) || blobTile?.Comp.Core == null)
            return;

        var coords = Transform(blobTile.Value).Coordinates;
        var tileType = args.TileType;
        var cost = _protoMan.Index(tileType).Cost;

        if (!CheckValidBlobTile(blobTile.Value, args, out var nearNode)
            || !TryUseAbility(ent, cost, coords))
            return;

        _blobTile.TransformBlobTile(blobTile, ent, nearNode, tileType, coords);
    }

    public bool TryGetTargetBlobTile(WorldTargetActionEvent args, out Entity<BlobTileComponent>? blobTile)
    {
        blobTile = null;

        var gridUid = _transform.GetGrid(args.Target);

        if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
            return false;

        Entity<MapGridComponent> grid = (gridUid.Value, gridComp);

        var centerTile = _mapSystem.GetLocalTilesIntersecting(grid,
                grid,
                new Box2(args.Target.Position, args.Target.Position))
            .ToArray();

        foreach (var tileRef in centerTile)
        {
            foreach (var ent in _mapSystem.GetAnchoredEntities(grid, grid, tileRef.GridIndices))
            {
                if (!_tile.TryComp(ent, out var blobTileComponent))
                    continue;

                blobTile = (ent, blobTileComponent);
                return true;
            }
        }

        return false;
    }

    private bool CheckValidBlobTile(
        Entity<BlobTileComponent> tile,
        BlobTransformTileActionEvent args,
        out Entity<BlobNodeComponent>? node)
    {
        node = null;

        var coords = Transform(tile).Coordinates;
        var newTile = args.TileType;
        var checkTile = args.TransformFrom;
        var performer = args.Performer;

        if (args.NodeSearchRadius != null
            && !TryGetNearNode(coords, out node))
            return false;

        // Base checks
        if (tile.Comp.Core == null ||
            tile.Comp.TilePrototype == newTile ||
            _protoMan.Index(tile.Comp.TilePrototype).IsSpecial)
            return false;

        if (tile.Comp.TilePrototype != checkTile)
        {
            _popup.PopupCoordinates(Loc.GetString("blob-target-normal-blob-invalid"), coords, performer, PopupType.Large);
            return false;
        }

        // Handle Tile search
        if (args.TileSearchRadius != null)
        {
            if (GetNearTile(newTile, coords, args.TileSearchRadius.Value) == null)
                return true;

            _popup.PopupCoordinates(Loc.GetString("blob-target-close-to-tile"), coords, performer, PopupType.Large);
            return false;
        }

        // Handle Node search
        if (node == null && args.NodeSearchRadius != null)
        {
            _popup.PopupCoordinates(Loc.GetString("blob-target-nearby-not-node"),
                coords,
                performer,
                PopupType.Large);
            return false;
        }

        if (node == null
            || node.Value.Comp.PlacedSpecials.ContainsKey(newTile))
            return true;

        _popup.PopupCoordinates(Loc.GetString("blob-target-already-connected"),
            coords,
            performer,
            PopupType.Large);
        return false;

    }

    private void OnSplitCore(EntityUid uid, BlobCoreComponent blobCoreComponent, BlobSplitCoreActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!blobCoreComponent.CanSplit)
        {
            _popup.PopupEntity(Loc.GetString("blob-cant-split"), args.Performer, args.Performer, PopupType.Large);
            return;
        }

        if (!HasComp<BlobNodeComponent>(args.Target)
            || HasComp<BlobCoreComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("blob-target-node-blob-invalid"), args.Performer, args.Performer, PopupType.Large);
            return;
        }

        if (!TryUseAbility((uid, blobCoreComponent), blobCoreComponent.SplitCoreCost))
            return;

        QueueDel(args.Target);
        var newCore = Spawn(args.CoreProtoId, Transform(args.Target).Coordinates);

        blobCoreComponent.CanSplit = false;
        _action.RemoveAction(args.Action.Owner);

        if (TryComp<BlobCoreComponent>(newCore, out var newBlobCoreComponent))
            newBlobCoreComponent.CanSplit = false;
    }

    private void OnSwapCore(EntityUid uid, BlobCoreComponent blobCoreComponent, BlobSwapCoreActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        var blobTile = args.Target;

        if (!HasComp<BlobNodeComponent>(blobTile))
        {
            _popup.PopupEntity(Loc.GetString("blob-target-node-blob-invalid"), args.Performer, args.Performer, PopupType.Large);
            args.Handled = true;
            return;
        }

        if (!TryUseAbility((uid, blobCoreComponent), blobCoreComponent.SwapCoreCost))
            return;

        // Swap positions of blob's core and node.
        var nodePos = Transform(blobTile).Coordinates;
        var corePos = Transform(uid).Coordinates;
        _transform.SetCoordinates(uid, nodePos.SnapToGrid(EntityManager, _mapMan));
        _transform.SetCoordinates(blobTile, corePos.SnapToGrid(EntityManager, _mapMan));

        var xformCore = Transform(uid);
        if (!xformCore.Anchored)
            _transform.AnchorEntity(uid, xformCore);
        var xformNode = Transform(blobTile);
        if (!xformNode.Anchored)
            _transform.AnchorEntity(blobTile, xformNode);

        // And then swap their BlobNodeComponents, so they will work properly.
        _blobTile.SwapSpecials(
            (blobTile, EnsureComp<BlobNodeComponent>(blobTile)),
            (uid, EnsureComp<BlobNodeComponent>(uid)));

        args.Handled = true;
    }

    private void OnBlobToCore(Entity<BlobCoreComponent> uid, ref BlobToCoreActionEvent args)
    {
        if (args.Handled)
            return;

        _transform.SetCoordinates(args.Performer, Transform(uid).Coordinates);
        args.Handled = true;
    }

    private void OnBlobSwapChem(Entity<BlobCoreComponent> ent, ref BlobSwapChemActionEvent args)
    {
        if (args.Handled
            || !TryComp(ent.Owner, out ActorComponent? actor))
            return;

        _uiSystem.TryToggleUi(ent.Owner, BlobChemSwapUiKey.Key, actor.PlayerSession);
        args.Handled = true;
    }

    private void OnChemSelected(Entity<BlobCoreComponent> ent, ref BlobChemSwapPrototypeSelectedMessage args)
    {
        if (ent.Comp.CurrentChemical == args.SelectedId)
            return;

        var cost = _protoMan.Index(args.SelectedId).Cost;
        if (!TryUseAbility(ent, cost))
            return;

        ChangeChem(ent, args.SelectedId);
        UpdateUi(ent);
    }

    private void UpdateUi(Entity<BlobCoreComponent> ent)
    {
        var state = new BlobChemSwapBoundUserInterfaceState(ent.Comp.AvailableChemicals, ent.Comp.CurrentChemical);
        _uiSystem.SetUiState(ent.Owner, BlobChemSwapUiKey.Key, state);
    }
}
