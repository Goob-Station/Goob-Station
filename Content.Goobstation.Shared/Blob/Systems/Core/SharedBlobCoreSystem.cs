using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.Events;
using Content.Goobstation.Shared.Blob.Prototypes;
using Content.Goobstation.Shared.Blob.Systems.Observer;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Threading;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Blob.Systems.Core;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedBlobCoreSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBlobTileSystem _blobTile = default!;
    [Dependency] private readonly SharedBlobObserverSystem _observerSys = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;

    private EntityQuery<BlobTileComponent> _tile;
    private EntityQuery<BlobCoreComponent> _core;
    private EntityQuery<BlobFactoryComponent> _factory;
    private EntityQuery<BlobNodeComponent> _node;

    public override void Initialize()
    {
        SubscribeLocalEvent<BlobCoreComponent, DamageChangedEvent>(OnDamaged);
        SubscribeLocalEvent<BlobCoreComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<BlobCoreComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlobCoreComponent, MobStateChangedEvent>(OnStateChanged);
        SubscribeLocalEvent<BlobCoreComponent, EntityTerminatingEvent>(OnTerminating);

        _tile = GetEntityQuery<BlobTileComponent>();
        _core = GetEntityQuery<BlobCoreComponent>();
        _factory = GetEntityQuery<BlobFactoryComponent>();
        _node = GetEntityQuery<BlobNodeComponent>();

        InitializeActions();
        InitializeInteraction();
    }

    private static readonly EntProtoId BlobCaptureObjective = "BlobCaptureObjective";

    private void OnPlayerAttached(Entity<BlobCoreComponent> ent, ref PlayerAttachedEvent args)
    {
        _observerSys.AttachToObserver(ent);
        if (!_mind.TryGetMind(ent, out var mindId, out var mindComp))
            return;

        _mind.TryAddObjective(mindId, mindComp, BlobCaptureObjective);
        _role.MindAddRole(mindId, ent.Comp.MindRoleBlobPrototypeId, mindComp);
    }

    private void OnDamaged(EntityUid uid, BlobCoreComponent component, DamageChangedEvent args)
    {
        UpdateAllAlerts((uid, component));
        _popup.PopupCoordinates(Loc.GetString("blob-core-under-attack"), Transform(uid).Coordinates, component.Projection ?? uid, PopupType.LargeCaution);
    }

    public void UpdateAllAlerts(Entity<BlobCoreComponent> core)
    {
        var component = core.Comp;

        // This one for points
        var pointsSeverity = (short) Math.Clamp(Math.Round(component.Points.Float() / 10f), 0, 51);
        _alerts.ShowAlert(core.Owner, component.ResourceAlert, pointsSeverity);

        // And this one for health.
        if (!TryComp<DamageableComponent>(core.Owner, out var damageComp)
            || !_mobThreshold.TryGetDeadThreshold(core.Owner, out var totalHp))
            return;

        var currentHealth = totalHp - damageComp.TotalDamage;
        var healthSeverity = (short) Math.Clamp((currentHealth.Value / 20).Float(), 0, 20);

        _alerts.ShowAlert(core.Owner, component.HealthAlert, healthSeverity);
    }

    private const double KillCoreJobTime = 0.5;
    private readonly JobQueue _killCoreJobQueue = new(KillCoreJobTime);

    private sealed class KillBlobCore(
        SharedBlobCoreSystem system,
        Entity<BlobCoreComponent> ent,
        double maxTime,
        CancellationToken cancellation = default)
        : Job<object>(maxTime, cancellation)
    {
        protected override Task<object?> Process()
        {
            system.DestroyBlobCore(ent);
            return Task.FromResult<object?>(null);
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

        _blobTile.ConnectBlobTile((uid, blobTileComp), ent, (uid, nodeComp));
        _observerSys.CreateProjection(ent);
        UpdateAllAlerts(ent);
        ChangeChem(ent, comp.StartingChemical);

        // TODO BLOB try to remove this shitcode
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

    #endregion

    public void ChangeChem(Entity<BlobCoreComponent> core, ProtoId<BlobChemPrototype> newChem)
    {
        var comp = core.Comp;
        if (newChem == comp.CurrentChemical)
            return;

        comp.CurrentChemical = newChem;
        foreach (var blobTile in comp.BlobTiles)
        {
            if (!_tile.HasComp(blobTile))
                continue;

            _blobTile.ChangeBlobEntChem(blobTile, newChem);

            // TODO BLOB rework factories
            if (!_factory.TryGetComponent(blobTile, out var blobFactoryComponent)
                || blobFactoryComponent.Blobbernaut == null)
                continue;

            _blobTile.ChangeBlobEntChem(blobFactoryComponent.Blobbernaut.Value, newChem);
        }
    }

    /// <summary>
    /// Destroys the blob core and kills all its tiles.
    /// </summary>
    private void DestroyBlobCore(Entity<BlobCoreComponent> core)
    {
        QueueDel(core.Comp.Projection);
        QueueDel(core);
        RaiseLocalEvent(core, new BlobCoreDestroyedEvent());

        foreach (var blobTile in core.Comp.BlobTiles.AsParallel())
        {
            if (!_tile.TryGetComponent(blobTile, out var blobTileComponent))
                continue;

            _blobTile.KillBlobTile((blobTile, blobTileComponent));
            Dirty(blobTile, blobTileComponent);
        }
    }

    private void CreateKillBlobCoreJob(Entity<BlobCoreComponent> core)
    {
        if (_net.IsClient)
            return;

        var job = new KillBlobCore(this, core, KillCoreJobTime);
        _killCoreJobQueue.EnqueueJob(job);
    }

    public void ChangeBlobPoint(Entity<BlobCoreComponent> core, FixedPoint2 amount)
    {
        var comp = core.Comp;

        // You can't have more points than your max amount
        if (core.Comp.MaxPoints < comp.Points + amount)
            amount = core.Comp.MaxPoints - comp.Points;

        comp.Points += amount;
        UpdateAllAlerts(core);
    }

    /// <summary>
    /// Writes off points for some blob core and creates popup on observer or specified coordinates.
    /// </summary>
    /// <param name="core">Blob core that is going to lose points.</param>
    /// <param name="abilityCost">Cost of the ability.</param>
    /// <param name="coordinates">If not null, coordinates for popup to appear.</param>
    public bool TryUseAbility(Entity<BlobCoreComponent> core, FixedPoint2 abilityCost, EntityCoordinates? coordinates = null)
    {
        var observer = core.Comp.Projection;
        var money = core.Comp.Points;

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
    /// <returns>Nearest blob node with its component, null if wasn't found.</returns>
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
