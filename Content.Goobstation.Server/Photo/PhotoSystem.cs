using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Goobstation.Shared.Photo;
using Content.Server.GameTicking.Events;
using Content.Server.Hands.Systems;
using Content.Server.Humanoid;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.SSDIndicator;
using Content.Shared.Standing;
using Content.Shared.Tag;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Photo;

public sealed class PhotoSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ViewSubscriberSystem _viewSubscriber = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly StandingStateSystem _standingState = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private MapId _photoMap;
    private int _photosTaken = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);

        SubscribeLocalEvent<PhotoCameraComponent, AfterInteractEvent>(OnCameraInteract);

        SubscribeLocalEvent<PhotoComponent, BoundUIOpenedEvent>(OnPhotoOpened);
        SubscribeLocalEvent<PhotoComponent, UnsubscribePhotoVieverMessage>(OnPhotoCreated);

        SubscribeLocalEvent<CustomPhotoComponent, MapInitEvent>(OnCustomPhotoInit);
    }

    private void OnRoundStarting(RoundStartingEvent args)
    {
        _map.CreateMap(out var mapId, runMapInit: false);
        _photoMap = mapId;
        _photosTaken = 0;
    }

    private void OnCameraInteract(Entity<PhotoCameraComponent> ent, ref AfterInteractEvent args)
    {
        if (!BuildPhoto(args.User, args.ClickLocation, out var source, out var offset))
            return;

        var photo = Spawn("Photo", Transform(ent).ParentUid.ToCoordinates());
        var comp = EnsureComp<PhotoComponent>(photo);

        comp.SourceEntity = source.Value;
        comp.Offset = offset.Value;
    }

    private void OnPhotoOpened(Entity<PhotoComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (!_player.TryGetSessionByEntity(args.Actor, out var session))
            return;

        _viewSubscriber.AddViewSubscriber(ent.Comp.SourceEntity, session);
        _ui.ServerSendUiMessage(ent.Owner, ImageUiKey.Key, new PhotoUiOpenedMessage() { Map = _photoMap, Offset = ent.Comp.Offset }, args.Actor);
    }

    private void OnPhotoCreated(Entity<PhotoComponent> ent, ref UnsubscribePhotoVieverMessage args)
    {
        if (!_player.TryGetSessionByEntity(args.Actor, out var session))
            return;

        _viewSubscriber.RemoveViewSubscriber(ent.Comp.SourceEntity, session);
    }

    private void OnCustomPhotoInit(Entity<CustomPhotoComponent> ent, ref MapInitEvent args)
    {
        if (!AddPhotoByPath(ent.Comp.Photo, out var source, out var offset))
            return;

        var comp = EnsureComp<PhotoComponent>(ent.Owner);

        comp.SourceEntity = source.Value;
        comp.Offset = offset.Value;
    }

    /// <summary>
    /// Create a photo from certain entity
    /// </summary>
    /// <param name="user"></param>
    /// <param name="clickLocation"></param>
    /// <param name="source"></param>
    /// <param name="offset"></param>
    /// <returns>Whether photo was successfully created or not</returns>
    private bool BuildPhoto(EntityUid user, EntityCoordinates clickLocation, [NotNullWhen(true)] out EntityUid? source, [NotNullWhen(true)] out Vector2? offset)
    {
        source = null;
        offset = null;

        if (Transform(user).GridUid is not { Valid: true } grid)
            return false;

        var gridComp = Comp<MapGridComponent>(grid);

        offset = new Vector2(_photosTaken * 12, 0);

        if (!_mapLoader.TryLoadGrid(_photoMap, new ResPath("/Maps/_Goobstation/Nonstations/camera-pseudo-grid.yml"), out var pseudoGrid, offset: offset.Value))
            return false;

        _photosTaken++;

        var pseudoGridComp = Comp<MapGridComponent>(pseudoGrid.Value);
        var xform = Transform(user);
        var diff = ((clickLocation.Position - xform.Coordinates.Position).Normalized() + xform.Coordinates.Position).Floored();

        Box2 box = new(new Vector2i(-3, -3) + diff, new Vector2i(3, 3) + diff);

        var tileEnumerator = _map.GetLocalTilesEnumerator(grid, gridComp, box);
        while (tileEnumerator.MoveNext(out var tile))
        {
            List<EntityUid> copiedEnts = new();

            SetupTile((grid, gridComp), (pseudoGrid.Value, pseudoGridComp), tile, diff, copiedEnts, user, out var setupSource);

            if (setupSource.HasValue)
                source = setupSource.Value;
        }

        return source.HasValue;
    }

    /// <summary>
    /// Loads a grid to use it as photo
    /// </summary>
    /// <returns>Whether photo was successfully created or not</returns>
    private bool AddPhotoByPath(ResPath path, [NotNullWhen(true)] out EntityUid? source, [NotNullWhen(true)] out Vector2? offset)
    {
        source = null;
        offset = new Vector2(_photosTaken * 12, 0);

        if (!_mapLoader.TryLoadGrid(_photoMap, path, out var pseudoGrid, offset: offset.Value))
            return false;

        _photosTaken++;

        var children = Transform(pseudoGrid.Value.Owner).ChildEnumerator;
        while (children.MoveNext(out var uid))
        {
            if (_tag.HasTag(uid, (ProtoId<TagPrototype>) "CustomPhotoSource"))
            {
                source = uid;
                break;
            }
        }

        return source.HasValue;
    }

    private void SetupTile(Entity<MapGridComponent> grid, Entity<MapGridComponent> pseudoGrid, TileRef tileRef, Vector2i clickPosition, List<EntityUid> copied, EntityUid user, out EntityUid? source)
    {
        source = null;

        if (!_map.TryGetTileDef(Comp<MapGridComponent>(grid), tileRef.GridIndices, out var tileDef))
            return;

        if (!_map.TryGetTileRef(pseudoGrid, pseudoGrid, tileRef.GridIndices - clickPosition, out var pseudoTileRef))
            return;

        _tile.ReplaceTile(pseudoTileRef, _proto.Index<ContentTileDefinition>(tileDef.ID));

        var pos = new EntityCoordinates(grid, tileRef.GridIndices);

        SortedSet<EntityUid> ents = new(_lookup.GetEntitiesInRange(_transform.ToMapCoordinates(pos), 0.4f, LookupFlags.Uncontained));

        foreach (var item in ents)
        {
            if (copied.Contains(item))
                continue;

            if (!TryPrototype(item, out var proto))
                continue;

            copied.Add(item);

            var xform = Transform(item);

            if ((_transform.ToMapCoordinates(xform.Coordinates).Position - _transform.ToMapCoordinates(Transform(user).Coordinates).Position).Length() > 3.25f)
                continue;

            var entity = Spawn(proto.ID, new(pseudoGrid.Owner, xform.Coordinates.Position - clickPosition));
            Transform(entity).LocalRotation = xform.LocalRotation;

            RemComp<SSDIndicatorComponent>(entity);
            RemComp<DamageableComponent>(entity);

            _humanoid.CloneAppearance(item, entity);
            _appearance.CopyData(item, entity);

            if (_standingState.IsDown(item))
                _standingState.Down(entity, false, false, true, animate: false);

            if (item == user)
                source = entity;

            foreach (var clothing in GetInventoryEntities(item))
            {
                var ent = Spawn(clothing.Value);
                if (!_inventory.TryEquip(entity, ent, clothing.Key, true, true))
                    QueueDel(ent);
            }

            foreach (var handId in _hands.EnumerateHands(item))
            {

                if (!_hands.TryGetHeldItem(item, handId, out var held))
                    continue;

                if (!TryPrototype(held.Value, out var heldProto))
                    continue;

                var heldEnt = Spawn(heldProto.ID);
                _hands.TryForcePickup(entity, heldEnt, handId, false, false);
            }
        }
    }

    private Dictionary<string, string> GetInventoryEntities(EntityUid uid)
    {
        if (!TryComp<InventoryComponent>(uid, out var inventory))
            return new();

        Dictionary<string, string> result = new();

        foreach (var item in inventory.Containers)
        {
            if (!item.ContainedEntity.HasValue)
                continue;

            if (_inventory.TryGetContainingSlot(item.ContainedEntity.Value, out var slotDef) && TryPrototype(item.ContainedEntity.Value, out var proto))
                result.Add(slotDef.Name, proto.ID);
        }

        return result;
    }
}
