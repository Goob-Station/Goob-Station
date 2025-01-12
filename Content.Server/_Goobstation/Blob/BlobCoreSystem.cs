using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Actions;
using Content.Server.AlertLevel;
using Content.Server._Goobstation.Blob.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Objectives;
using Content.Server.Explosion.EntitySystems;
using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Explosion.Components;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Popups;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Weapons.Melee;
using Robust.Server.GameObjects;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._Goobstation.Blob;

public sealed class BlobCoreSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly BlobTileSystem _blobTile = default!;

    private EntityQuery<BlobTileComponent> _tile;
    private EntityQuery<BlobFactoryComponent> _factory;
    private EntityQuery<BlobNodeComponent> _node;

    [ValidatePrototypeId<AlertPrototype>]
    private const string BlobHealth = "BlobHealth";
    [ValidatePrototypeId<AlertPrototype>]
    private const string BlobResource = "BlobResource";
    [ValidatePrototypeId<CurrencyPrototype>]
    private const string BlobMoney = "BlobPoint";

    private readonly ReaderWriterLockSlim _pointsChange = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobCoreComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlobCoreComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BlobCoreComponent, DamageChangedEvent>(OnDamaged);
        SubscribeLocalEvent<BlobCoreComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeLocalEvent<BlobCoreComponent, BlobTransformTileActionEvent>(OnTileTransform);

        SubscribeLocalEvent<BlobCoreComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveGetProgressEvent>(OnBlobCaptureProgress);
        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveAfterAssignEvent>(OnBlobCaptureInfo);
        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveAssignedEvent>(OnBlobCaptureInfoAdd);


        _tile = GetEntityQuery<BlobTileComponent>();
        _factory = GetEntityQuery<BlobFactoryComponent>();
        _node = GetEntityQuery<BlobNodeComponent>();
    }

    private const double KillCoreJobTime = 0.5;
    private readonly JobQueue _killCoreJobQueue = new(KillCoreJobTime);

    public sealed class KillBlobCore(
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

    private void OnStartup(EntityUid uid, BlobCoreComponent component, ComponentStartup args)
    {
        if (!_tile.TryGetComponent(uid, out var blobTileComponent))
        {
            return;
        }

        if (!_node.TryGetComponent(uid, out var nodeComponent))
        {
            return;
        }

        ConnectBlobTile((uid, blobTileComponent), (uid, component), (uid, nodeComponent));

        var store = EnsureComp<StoreComponent>(uid);
        store.CurrencyWhitelist.Add(BlobMoney);

        UpdateAllAlerts((uid, component));
        ChangeChem(uid, component.DefaultChem, component);

        foreach (var action in component.ActionPrototypes)
        {
            EntityUid? actionUid = null;
            _action.AddAction(uid, ref actionUid, action);

            if (actionUid != null)
                component.Actions.Add(actionUid.Value);
        }
    }

    private void OnTerminating(EntityUid uid, BlobCoreComponent component, ref EntityTerminatingEvent args)
    {
        CreateKillBlobCoreJob((uid, component));
    }

    private void OnDestruction(EntityUid uid, BlobCoreComponent component, DestructionEventArgs args)
    {
        CreateKillBlobCoreJob((uid, component));
    }

    private void OnPlayerAttached(EntityUid uid, BlobCoreComponent component, PlayerAttachedEvent args)
    {
        var xform = Transform(uid);

        if (!HasComp<MapGridComponent>(xform.GridUid))
            return;

        if (!TerminatingOrDeleted(component.Observer))
            return;

        CreateBlobObserver(uid, args.Player.UserId, component);
    }

    private void OnDamaged(EntityUid uid, BlobCoreComponent component, DamageChangedEvent args)
    {
        UpdateAllAlerts((uid, component));
    }

    private void OnTileTransform(EntityUid uid, BlobCoreComponent blobCoreComponent, BlobTransformTileActionEvent args)
    {
        TransformSpecialTile((uid, blobCoreComponent), args);
    }

    #endregion

    #region Objective

    private void OnBlobCaptureInfoAdd(Entity<BlobCaptureConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        if (args.Mind.OwnedEntity == null)
        {
            args.Cancelled = true;
            return;
        }
        if (!TryComp<BlobObserverComponent>(args.Mind.OwnedEntity, out var blobObserverComponent)
            || !HasComp<BlobCoreComponent>(blobObserverComponent.Core))
        {
            args.Cancelled = true;
            return;
        }

        var station = _stationSystem.GetOwningStation(blobObserverComponent.Core);
        if (station == null)
        {
            args.Cancelled = true;
            return;
        }

        ent.Comp.Target = CompOrNull<StationBlobConfigComponent>(station)?.StageTheEnd ?? StationBlobConfigComponent.DefaultStageEnd;
    }

    private void OnBlobCaptureInfo(EntityUid uid, BlobCaptureConditionComponent component, ref ObjectiveAfterAssignEvent args)
    {
        _metaDataSystem.SetEntityName(uid,Loc.GetString("objective-condition-blob-capture-title"));
        _metaDataSystem.SetEntityDescription(uid,Loc.GetString("objective-condition-blob-capture-description", ("count", component.Target)));
    }

    private void OnBlobCaptureProgress(EntityUid uid, BlobCaptureConditionComponent component, ref ObjectiveGetProgressEvent args)
    {
        if (!TryComp<BlobObserverComponent>(args.Mind.OwnedEntity, out var blobObserverComponent)
            || !TryComp<BlobCoreComponent>(blobObserverComponent.Core, out var blobCoreComponent))
        {
            args.Progress = 0;
            return;
        }

        var target = component.Target;
        args.Progress = 0;

        if (target != 0)
            args.Progress = MathF.Min((float) blobCoreComponent.BlobTiles.Count / target, 1f);
        else
            args.Progress = 1f;
    }
    #endregion

    public void UpdateAllAlerts(Entity<BlobCoreComponent> core, StoreComponent? store = null)
    {
        if (!Resolve(core, ref store))
            return;

        var component = core.Comp;

        if (component.Observer == null)
            return;

        // This one for points
        var pt = store.Balance.GetValueOrDefault(BlobMoney);
        var pointsSeverity = (short) Math.Clamp(Math.Round(pt.Float() / 10f), 0, 51);
        _alerts.ShowAlert(component.Observer.Value, BlobResource, pointsSeverity);

        // And this one for health.
        if (!TryComp<DamageableComponent>(core.Owner, out var damageComp))
            return;

        var currentHealth = component.CoreBlobTotalHealth - damageComp.TotalDamage;
        var healthSeverity = (short) Math.Clamp(Math.Round(currentHealth.Float() / 20f), 0, 20);

        _alerts.ShowAlert(component.Observer.Value, BlobHealth, healthSeverity);
    }

    public bool CreateBlobObserver(EntityUid blobCoreUid, NetUserId userId, BlobCoreComponent? core = null)
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
    }

    public void ChangeChem(EntityUid uid, BlobChemType newChem, BlobCoreComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (newChem == component.CurrentChem)
            return;

        component.CurrentChem = newChem;
        foreach (var blobTile in component.BlobTiles)
        {
            if (!_tile.TryGetComponent(blobTile, out var blobTileComponent))
                continue;

            blobTileComponent.Color = component.ChemСolors[newChem];
            Dirty(blobTile, blobTileComponent);

            ChangeBlobEntChem(blobTile, newChem);

            if (!_factory.TryGetComponent(blobTile, out var blobFactoryComponent))
                continue;

            if (!TryComp<BlobbernautComponent>(blobFactoryComponent.Blobbernaut, out var blobbernautComponent))
                continue;

            blobbernautComponent.Color = component.ChemСolors[newChem];
            Dirty(blobFactoryComponent.Blobbernaut.Value, blobbernautComponent);

            if (TryComp<MeleeWeaponComponent>(blobFactoryComponent.Blobbernaut, out var meleeWeaponComponent))
            {
                var blobbernautDamage = new DamageSpecifier();
                foreach (var keyValuePair in component.ChemDamageDict[component.CurrentChem].DamageDict)
                {
                    blobbernautDamage.DamageDict.Add(keyValuePair.Key, keyValuePair.Value * 0.8f);
                }
                meleeWeaponComponent.Damage = blobbernautDamage;
            }

            ChangeBlobEntChem(blobFactoryComponent.Blobbernaut.Value, newChem);
        }
    }

    private void ChangeBlobEntChem(EntityUid uid, BlobChemType newChem)
    {
        switch (newChem)
        {
            case BlobChemType.ExplosiveLattice:
                _damageable.SetDamageModifierSetId(uid, "ExplosiveLatticeBlob");
                _explosionSystem.SetExplosionResistance(uid, 0f, EnsureComp<ExplosionResistanceComponent>(uid));
                break;
            case BlobChemType.ElectromagneticWeb:
                _damageable.SetDamageModifierSetId(uid, "ElectromagneticWebBlob");
                break;
            default:
                _damageable.SetDamageModifierSetId(uid, "BaseBlob");
                break;
        }
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
        BlobTileType newBlobTile,
        EntityCoordinates coordinates)
    {
        if (oldTileUid != null)
        {
            if (oldTileUid.Value.Comp.Core != blobCore)
                return false;

            RemoveBlobTile(oldTileUid.Value, blobCore);
        }

        var blobCoreComp = blobCore.Comp;
        var blobTileUid = EntityManager.SpawnEntity(blobCoreComp.TilePrototypes[newBlobTile], coordinates);

        if (!_tile.TryGetComponent(blobTileUid, out var blobTileComp))
        {
            // Blob somehow spawned not a blob tile?
            return false;
        }

        ConnectBlobTile((blobTileUid, blobTileComp), blobCore, nearNode);
        ChangeBlobEntChem(blobTileUid, blobCoreComp.CurrentChem);

        Dirty(blobTileUid, blobTileComp);

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

        tileComp.Color = coreComp.ChemСolors[coreComp.CurrentChem];
        tileComp.Core = core;
        Dirty(tile, tileComp);

        if (node == null)
            return;

        switch (tile.Comp.BlobTileType)
        {
            case BlobTileType.Factory:
                node.Value.Comp.BlobFactory = tile;
                Dirty(node.Value);
                break;
            case BlobTileType.Resource:
                node.Value.Comp.BlobResource = tile;
                Dirty(node.Value);
                break;
        }
    }

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

    public bool CheckValidBlobTile(
        Entity<BlobTileComponent> tile,
        Entity<BlobNodeComponent>? node,
        bool requireNode,
        BlobTransformTileActionEvent args)
    {
        var coords = Transform(tile).Coordinates;

        var newTile = args.TileType;
        var checkTile = args.TransformFrom;
        var performer = args.Performer;

        if (tile.Comp.Core == null ||
            tile.Comp.BlobTileType == newTile ||
            tile.Comp.BlobTileType == BlobTileType.Core ||
            tile.Comp.BlobTileType != checkTile)
        {
            _popup.PopupCoordinates(Loc.GetString("blob-target-normal-blob-invalid"), coords, performer, PopupType.Large);
            return false;
        }

        var core = tile.Comp.Core.Value;

        if (checkTile == BlobTileType.Invalid)
            return true;

        // Handle node spawn
        if (newTile == BlobTileType.Node)
        {
            if (GetNearNode(coords, core.Comp.NodeRadiusLimit) == null)
                return true;

            _popup.PopupCoordinates(Loc.GetString("blob-target-close-to-node"), coords, performer, PopupType.Large);
            return false;
        }

        if (!requireNode)
            return true;

        if (node == null)
        {
            _popup.PopupCoordinates(Loc.GetString("blob-target-nearby-not-node"),
                coords,
                performer,
                PopupType.Large);
            return false;
        }

        if (_blobTile.IsEmptySpecial(node.Value, newTile))
            return true;

        _popup.PopupCoordinates(Loc.GetString("blob-target-already-connected"),
            coords,
            performer,
            PopupType.Large);
        return false;
    }

    public void TransformSpecialTile(Entity<BlobCoreComponent> blobCore, BlobTransformTileActionEvent args)
    {
        if (!TryGetTargetBlobTile(args, out var blobTile) || blobTile?.Comp.Core == null)
            return;

        var coords = Transform(blobTile.Value).Coordinates;
        var tileType = args.TileType;
        var nearNode = GetNearNode(coords);

        if (!CheckValidBlobTile(blobTile.Value, nearNode, args.RequireNode, args))
            return;

        if (!TryUseAbility(blobCore, blobCore.Comp.BlobTileCosts[tileType], coords))
            return;

        TransformBlobTile(
            blobTile,
            blobCore,
            nearNode,
            tileType,
            coords);
    }

    public void RemoveBlobTile(Entity<BlobTileComponent> tile, Entity<BlobCoreComponent> core)
    {
        QueueDel(tile);
        core.Comp.BlobTiles.Remove(tile);
    }

    private void DestroyBlobCore(Entity<BlobCoreComponent> core, EntityUid? stationUid)
    {
        QueueDel(core.Comp.Observer);

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
            {
                continue;
            }
            aliveBlobs++;
        }

        if (aliveBlobs == 0)
        {
            var blobRuleQuery = EntityQueryEnumerator<BlobRuleComponent, ActiveGameRuleComponent>();
            while (blobRuleQuery.MoveNext(out _, out var blobRuleComp, out _))
            {
                if (blobRuleComp.Stage is BlobStage.TheEnd or BlobStage.Default)
                    continue;

                if(stationUid != null)
                    _alertLevelSystem.SetLevel(stationUid.Value, "green", true, true, true);

                _roundEndSystem.CancelRoundEndCountdown(null, false);
                blobRuleComp.Stage = BlobStage.Default;
            }
        }

        QueueDel(core);
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
        var tileComp = target.Comp;

        if (target.Comp.ReturnCost)
        {
            returnCost = core.Comp.BlobTileCosts[tileComp.BlobTileType];
        }

        if (returnCost <= 0)
            return;

        ChangeBlobPoint(core, returnCost);

        if (core.Comp.Observer == null)
            return;

        _popup.PopupCoordinates(Loc.GetString("blob-get-resource", ("point", returnCost)),
            Transform(target).Coordinates,
            core.Comp.Observer.Value,
            PopupType.Large);
    }

    public bool ChangeBlobPoint(Entity<BlobCoreComponent> core, FixedPoint2 amount, StoreComponent? store = null)
    {
        if (!Resolve(core, ref store))
            return false;

        if (!_pointsChange.TryEnterWriteLock(1000))
            return false;

        if (_storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2>
                {
                    { BlobMoney, amount }
                },
                core,
                store))
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

        var observer = core.Comp.Observer;
        var money = store.Balance.GetValueOrDefault(BlobMoney);

        if (observer == null)
            return false;

        if (money < abilityCost)
        {
            _popup.PopupEntity(Loc.GetString(
                "blob-not-enough-resources",
                ("point", abilityCost.Int() - money.Int())),
                observer.Value,
                observer.Value,
                PopupType.Large);
            return false;
        }

        coordinates ??= Transform(observer.Value).Coordinates;

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
    public Entity<BlobNodeComponent>? GetNearNode(
        EntityCoordinates coords,
        float radius = 3f)
    {
        var gridUid = _transform.GetGrid(coords)!.Value;

        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return null;

        var nearestDistance = float.MaxValue;
        var nodeComponent = new BlobNodeComponent();
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
                if (!_node.TryComp(ent, out var nodeComp))
                    continue;
                var tileCords = Transform(ent).Coordinates;
                var distance = Vector2.Distance(coords.Position, tileCords.Position);

                if (!(distance < nearestDistance))
                    continue;

                nearestDistance = distance;
                nearestEntityUid = ent;
                nodeComponent = nodeComp;
            }
        }

        return nearestDistance > radius ? null : (nearestEntityUid, nodeComponent);
    }
}
