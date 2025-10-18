// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Server.Blob.GameTicking;
using Content.Goobstation.Server.Blob.GameTicking.Rules;
using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.Events;
using Content.Goobstation.Shared.Blob.Prototypes;
using Content.Goobstation.Shared.Blob.Systems.Core;
using Content.Goobstation.Shared.Blob.Systems.Observer;
using Content.Server.AlertLevel;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.GameTicking.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Store.Components;
using Content.Shared.Weapons.Melee;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Blob.Systems;

public sealed class BlobCoreSystem : SharedBlobCoreSystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly BlobTileSystem _blobTile = default!;
    [Dependency] private readonly SharedBlobObserverSystem _observerSys = default!;

    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    private EntityQuery<BlobTileComponent> _tile;
    private EntityQuery<BlobFactoryComponent> _factory;
    private EntityQuery<BlobNodeComponent> _node;

    private readonly ReaderWriterLockSlim _pointsChange = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobCoreComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlobCoreComponent, MobStateChangedEvent>(OnStateChanged);

        SubscribeLocalEvent<BlobCoreComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeLocalEvent<BlobCoreComponent, BlobTransformTileActionEvent>(OnTileTransform);


        _tile = GetEntityQuery<BlobTileComponent>();
        _factory = GetEntityQuery<BlobFactoryComponent>();
        _node = GetEntityQuery<BlobNodeComponent>();
    }

    private const double KillCoreJobTime = 0.5;
    private readonly JobQueue _killCoreJobQueue = new(KillCoreJobTime);

    private sealed class KillBlobCore(
        BlobCoreSystem system,
        EntityUid? station,
        Entity<BlobCoreComponent> ent,
        double maxTime,
        CancellationToken cancellation = default)
        : Job<object>(maxTime, cancellation)
    {
        protected override async Task<object?> Process()
        {
            system.DestroyBlobCore(ent, station);
            return null;
        }
    }

    #region Events

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _killCoreJobQueue.Process();
    }

    private static readonly EntProtoId MobObserverBlobController = "MobObserverBlobController";

    private void OnStartup(Entity<BlobCoreComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        var blobTileComp = EnsureComp<BlobTileComponent>(uid);
        var nodeComp = EnsureComp<BlobNodeComponent>(uid);

        ConnectBlobTile((uid, blobTileComp), ent, (uid, nodeComp));

        UpdateAllAlerts(ent);
        ChangeChem(uid, comp.StartingChemical, comp);

        _observerSys.CreateProjection(ent);

        _hands.AddHand(ent, "BlobHand", HandLocation.Middle);

        comp.Controller = Spawn(MobObserverBlobController, Transform(ent).Coordinates);
        var controllerComp = EnsureComp<BlobObserverControllerComponent>(comp.Controller.Value);
        controllerComp.BlobCore = ent.Owner;
        Dirty(comp.Controller.Value, controllerComp);

        _hands.TryPickup(ent, comp.Controller.Value, "BlobHand", false, false, false);
    }

    private void OnTerminating(Entity<BlobCoreComponent> ent, ref EntityTerminatingEvent args)
    {
        CreateKillBlobCoreJob(ent);
    }

    private void OnStateChanged(Entity<BlobCoreComponent> ent, ref MobStateChangedEvent args)
    {
        if (_mobState.IsDead(ent))
            CreateKillBlobCoreJob(ent);
    }

    private void OnTileTransform(EntityUid uid, BlobCoreComponent blobCoreComponent, BlobTransformTileActionEvent args)
    {
        TransformSpecialTile((uid, blobCoreComponent), args);
    }

    #endregion



    /*public bool CreateBlobObserver(EntityUid blobCoreUid, NetUserId userId, BlobCoreComponent? core = null)
    {
        if (!Resolve(blobCoreUid, ref core))
            return false;

        var blobRule = EntityQuery<BlobRuleComponent>().FirstOrDefault();
        if (blobRule == null)
        {
            _gameTicker.StartGameRule("BlobRule", out _);
        }

        var ev = new CreateBlobObserverEvent(userId);
        RaiseLocalEvent(blobCoreUid, ev, true);

        return !ev.Cancelled;
    }*/

    public void ChangeChem(EntityUid uid, ProtoId<BlobChemPrototype> newChem, BlobCoreComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || newChem == component.CurrentChemical)
            return;

        component.CurrentChemical = newChem;
        var proto = _protoMan.Index(newChem);
        foreach (var blobTile in component.BlobTiles)
        {
            if (!_tile.TryGetComponent(blobTile, out var blobTileComponent))
                continue;

            blobTileComponent.Color = proto.Color;
            Dirty(blobTile, blobTileComponent);

            ChangeBlobEntChem(blobTile, newChem);

            // TODO BLOB rework everything below
            if (!_factory.TryGetComponent(blobTile, out var blobFactoryComponent)
                || !TryComp<BlobbernautComponent>(blobFactoryComponent.Blobbernaut, out var blobbernautComponent))
                continue;

            blobTileComponent.Color = proto.Color;
            Dirty(blobFactoryComponent.Blobbernaut.Value, blobbernautComponent);

            if (TryComp<MeleeWeaponComponent>(blobFactoryComponent.Blobbernaut, out var meleeWeaponComponent))
            {
                var blobbernautDamage = new DamageSpecifier();
                foreach (var keyValuePair in proto.Damage.DamageDict)
                {
                    blobbernautDamage.DamageDict.Add(keyValuePair.Key, keyValuePair.Value * 0.8f);
                }
                meleeWeaponComponent.Damage = blobbernautDamage;
            }

            ChangeBlobEntChem(blobFactoryComponent.Blobbernaut.Value, newChem);
        }
    }

    public void ChangeBlobEntChem(EntityUid uid, ProtoId<BlobChemPrototype> newChem)
    {

    }

    /// <summary>
    /// Transforms one blob tile in another type or creates a new one from scratch.
    /// </summary>
    /// <param name="oldTileUid">Uid of the ols tile that's going to get deleted.</param>
    /// <param name="blobCore">Blob core that preformed the transformation. Make sure it isn't came from the BlobTileComponent of the target!</param>
    /// <param name="nearNode">Node will be used in ConnectBlobTile method.</param>
    /// <param name="newBlobTile">Type of a new blob tile.</param>
    /// <param name="coordinates">Coordinates of a new tile.</param>
    /// <seealso cref="ConnectBlobTile"/>
    /// <seealso cref="BlobCoreComponent"/>
    public bool TransformBlobTile(
        Entity<BlobTileComponent>? oldTileUid,
        Entity<BlobCoreComponent> blobCore,
        Entity<BlobNodeComponent>? nearNode,
        ProtoId<BlobTilePrototype> newBlobTile,
        EntityCoordinates coordinates)
    {
        if (oldTileUid != null)
        {
            if (oldTileUid.Value.Comp.Core != blobCore.Owner)
                return false;

            RemoveBlobTile(oldTileUid.Value, blobCore);
        }

        var blobCoreComp = blobCore.Comp;
        var proto = _protoMan.Index(newBlobTile);
        var blobTileUid = Spawn(proto.SpawnId, coordinates);

        if (!_tile.TryGetComponent(blobTileUid, out var blobTileComp))
        {
            Log.Error($"Spawned blob tile {ToPrettyString(blobTileUid)} doesn't have BlobTileComponent!");
            return false;
        }

        ConnectBlobTile((blobTileUid, blobTileComp), blobCore, nearNode);
        ChangeBlobEntChem(blobTileUid, blobCoreComp.CurrentChemical);
        Dirty(blobTileUid, blobTileComp);

        var ev = new BlobTransformTileEvent();
        RaiseLocalEvent(blobTileUid, ev);

        return true;
    }

    /// <summary>
    /// Adds BlobTile to blob core and node, if specified.
    /// </summary>
    /// <param name="tile">Entity of the blob tile.</param>
    /// <param name="core">Entity of the blob core.</param>
    /// <param name="node">If not null, tries to connect tile to the node by checking if their BlobTileType is presented in dictionary.</param>
    public void ConnectBlobTile(
        Entity<BlobTileComponent> tile,
        Entity<BlobCoreComponent> core,
        Entity<BlobNodeComponent>? node)
    {
        var coreComp = core.Comp;
        var tileComp = tile.Comp;

        coreComp.BlobTiles.Add(tile);
        tileComp.Core = core;
        Dirty(tile, tileComp);

        var tileProto = _protoMan.Index(tile.Comp.TilePrototype);
        if (node == null
            || !tileProto.IsSpecial)
            return;

        node.Value.Comp.PlacedSpecials.Add(tile.Comp.TilePrototype, tile.Owner);
    }

    #region Transform Tile Event

    public bool TryGetTargetBlobTile(WorldTargetActionEvent args, out Entity<BlobTileComponent>? blobTile)
    {
        blobTile = null;

        var gridUid = _transform.GetGrid(args.Target);

        if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
        {
            return false;
        }

        Entity<MapGridComponent> grid = (gridUid.Value, gridComp);

        var centerTile = _mapSystem.GetLocalTilesIntersecting(grid,
                grid,
                new Box2(args.Target.Position, args.Target.Position))
            .ToArray();

        foreach (var tileRef in centerTile)
        {
            foreach (var ent in _mapSystem.GetAnchoredEntities(grid, grid, tileRef.GridIndices))
            {
                if (!_tile.TryGetComponent(ent, out var blobTileComponent))
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

    private void TransformSpecialTile(Entity<BlobCoreComponent> blobCore, BlobTransformTileActionEvent args)
    {
        if (!TryGetTargetBlobTile(args, out var blobTile) || blobTile?.Comp.Core == null)
            return;

        var coords = Transform(blobTile.Value).Coordinates;
        var tileType = args.TileType;
        var cost = _protoMan.Index(tileType).Cost;

        if (!CheckValidBlobTile(blobTile.Value, args, out var nearNode)
            || !TryUseAbility(blobCore, cost, coords))
            return;

        TransformBlobTile(blobTile, blobCore, nearNode, tileType, coords);
    }

    #endregion

    public void RemoveBlobTile(Entity<BlobTileComponent> tile, Entity<BlobCoreComponent> core)
    {
        QueueDel(tile);
        core.Comp.BlobTiles.Remove(tile);
    }

    /// <summary>
    /// Destroys the blob core and kills all its tiles.
    /// </summary>
    private void DestroyBlobCore(Entity<BlobCoreComponent> core, EntityUid? stationUid)
    {
        QueueDel(core.Comp.Projection);
        QueueDel(core);

        foreach (var blobTile in core.Comp.BlobTiles.AsParallel())
        {
            if (!_tile.TryGetComponent(blobTile, out var blobTileComponent))
                continue;

            blobTileComponent.Core = null;
            blobTileComponent.Color = Color.White;
            Dirty(blobTile, blobTileComponent);
        }

        var blobCoreQuery = EntityQueryEnumerator<BlobCoreComponent, MetaDataComponent>();
        var aliveBlobs = 0;
        while (blobCoreQuery.MoveNext(out var ent, out _, out var md))
        {
            if (TerminatingOrDeleted(ent, md))
                continue;

            aliveBlobs++;
        }

        if (aliveBlobs != 0)
            return;

        var blobRuleQuery = EntityQueryEnumerator<BlobRuleComponent, ActiveGameRuleComponent>();
        while (blobRuleQuery.MoveNext(out _, out var blobRuleComp, out _))
        {
            if (blobRuleComp.Stage is BlobStage.TheEnd or BlobStage.Default)
                continue;

            if (stationUid != null)
                _alertLevelSystem.SetLevel(stationUid.Value, "green", true, true, true);

            blobRuleComp.Stage = BlobStage.Default;
        }
    }

    private void CreateKillBlobCoreJob(Entity<BlobCoreComponent> core)
    {
        var station = _stationSystem.GetOwningStation(core);
        var job = new KillBlobCore(this, station, core, KillCoreJobTime);
        _killCoreJobQueue.EnqueueJob(job);
    }

    public void RemoveTileWithReturnCost(Entity<BlobTileComponent> target, Entity<BlobCoreComponent> core)
    {
        RemoveBlobTile(target, core);

        FixedPoint2 returnCost = 0;
        if (target.Comp.Refudable)
        {
            returnCost = _protoMan.Index(target.Comp.TilePrototype).Cost;
        }

        if (returnCost <= 0)
            return;

        ChangeBlobPoint(core, returnCost);

        if (core.Comp.Projection == null)
            return;

        _popup.PopupCoordinates(Loc.GetString("blob-get-resource", ("point", returnCost)),
            Transform(target).Coordinates,
            core.Comp.Projection.Value,
            PopupType.Large);
    }

    public bool ChangeBlobPoint(Entity<BlobCoreComponent> core, FixedPoint2 amount, StoreComponent? store = null)
    {
        if (!Resolve(core, ref store)
            || !_pointsChange.TryEnterWriteLock(1000))
            return false;

        // You can't have more points than your max amount
        if (core.Comp.MaxPoints < store.Balance[core.Comp.Currency] + amount)
            amount = core.Comp.MaxPoints - store.Balance[core.Comp.Currency];

        var toAdd = new Dictionary<string, FixedPoint2> { { core.Comp.Currency, amount } };
        if (_storeSystem.TryAddCurrency(toAdd, core, store))
        {
            UpdateAllAlerts(core);

            _pointsChange.ExitWriteLock();
            return true;
        }

        _pointsChange.ExitWriteLock();
        return false;
    }

    /// <summary>
    /// Writes off points for some blob core and creates popup on observer or specified coordinates.
    /// </summary>
    /// <param name="core">Blob core that is going to lose points.</param>
    /// <param name="abilityCost">Cost of the ability.</param>
    /// <param name="coordinates">If not null, coordinates for popup to appear.</param>
    /// <param name="store">StoreComponent</param>
    public bool TryUseAbility(Entity<BlobCoreComponent> core, FixedPoint2 abilityCost, EntityCoordinates? coordinates = null, StoreComponent? store = null)
    {
        if (!Resolve(core, ref store))
            return false;

        var observer = core.Comp.Projection;
        var money = store.Balance.GetValueOrDefault(core.Comp.Currency);

        if (observer == null)
            return false;

        coordinates ??= Transform(observer.Value).Coordinates;

        if (money < abilityCost)
        {
            _popup.PopupCoordinates(Loc.GetString(
                "blob-not-enough-resources",
                ("point", abilityCost.Int() - money.Int())),
                coordinates.Value,
                observer.Value,
                PopupType.Large);
            return false;
        }

        _popup.PopupCoordinates(
            Loc.GetString("blob-spent-resource", ("point", abilityCost.Int())),
            coordinates.Value,
            observer.Value,
            PopupType.LargeCaution);

        ChangeBlobPoint(core, -abilityCost);
        return true;
    }

    /// <summary>
    /// Gets the nearest Blob node from some EntityCoordinates.
    /// </summary>
    /// <param name="coords">The EntityCoordinates to check from.</param>
    /// <param name="radius">Radius to check from coords.</param>
    /// <returns>Nearest blob node with it's component, null if wasn't founded.</returns>
    public bool TryGetNearNode(
        EntityCoordinates coords,
        [NotNullWhen(true)] out Entity<BlobNodeComponent>? node,
        float radius = 3f)
    {
        node = null;
        var gridUid = _transform.GetGrid(coords)!.Value;

        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return false;

        var nearestDistance = float.MaxValue;
        EntityUid? nearestEntityUid = null;
        BlobNodeComponent? blobNodeComp = null;

        var innerTiles = _mapSystem.GetLocalTilesIntersecting(
                gridUid,
                grid,
                new Box2(coords.Position + new Vector2(-radius, -radius),
                    coords.Position + new Vector2(radius, radius)),
                false)
            .ToArray();

        foreach (var tileRef in innerTiles)
        {
            foreach (var ent in _mapSystem.GetAnchoredEntities(gridUid, grid, tileRef.GridIndices))
            {
                if (!_node.TryComp(ent, out var tileComp))
                    continue;

                var tileCords = Transform(ent).Coordinates;
                var distance = Vector2.Distance(coords.Position, tileCords.Position);

                if (!(distance < nearestDistance))
                    continue;

                nearestDistance = distance;
                nearestEntityUid = ent;
                blobNodeComp = tileComp;
            }
        }

        if (nearestEntityUid == null
            || blobNodeComp == null)
            return false;

        node = (nearestEntityUid.Value, blobNodeComp);
        return true;
    }

    public Entity<BlobTileComponent>? GetNearTile(
        ProtoId<BlobTilePrototype> tileType,
        EntityCoordinates coords,
        float radius = 3f)
    {
        var gridUid = _transform.GetGrid(coords)!.Value;

        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return null;

        var nearestDistance = float.MaxValue;
        var tileComponent = new BlobTileComponent();
        var nearestEntityUid = EntityUid.Invalid;

        var innerTiles = _mapSystem.GetLocalTilesIntersecting(
                gridUid,
                grid,
                new Box2(coords.Position + new Vector2(-radius, -radius),
                    coords.Position + new Vector2(radius, radius)),
                false)
            .ToArray();

        foreach (var tileRef in innerTiles)
        {
            foreach (var ent in _mapSystem.GetAnchoredEntities(gridUid, grid, tileRef.GridIndices))
            {
                if (!_tile.TryComp(ent, out var tileComp))
                    continue;

                if (tileComp.TilePrototype != tileType)
                    continue;

                var tileCords = Transform(ent).Coordinates;
                var distance = Vector2.Distance(coords.Position, tileCords.Position);

                if (!(distance < nearestDistance))
                    continue;

                nearestDistance = distance;
                nearestEntityUid = ent;
                tileComponent = tileComp;
            }
        }

        return nearestDistance > radius ? null : (nearestEntityUid, tileComponent);
    }
}
