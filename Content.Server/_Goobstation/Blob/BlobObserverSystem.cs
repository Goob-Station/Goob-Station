using System.Linq;
using Content.Server.Actions;
using Content.Server._Goobstation.Blob.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Chat.Managers;
using Content.Server.Hands.Systems;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Hands.Components;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Blob;

public sealed class BlobObserverSystem : SharedBlobObserverSystem
{
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly BlobCoreSystem _blobCoreSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ILogManager _logMan = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly ISharedPlayerManager _actorSystem = default!;
    [Dependency] private readonly ViewSubscriberSystem _viewSubscriberSystem = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly BlobTileSystem _blobTileSystem = default!;

    private EntityQuery<BlobTileComponent> _tileQuery;

    private const double MoverJobTime = 0.005;
    private readonly JobQueue _moveJobQueue = new(MoverJobTime);

    private ISawmill _logger = default!;

    [ValidatePrototypeId<EntityPrototype>] private const string BlobCaptureObjective = "BlobCaptureObjective";
    [ValidatePrototypeId<EntityPrototype>] private const string MobObserverBlobController = "MobObserverBlobController";
    [ValidatePrototypeId<AlertPrototype>] private const string BlobHealth = "BlobHealth";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobCoreComponent, CreateBlobObserverEvent>(OnCreateBlobObserver);

        SubscribeLocalEvent<BlobObserverComponent, PlayerAttachedEvent>(OnPlayerAttached, before: [typeof(ActionsSystem)]);
        SubscribeLocalEvent<BlobObserverComponent, PlayerDetachedEvent>(OnPlayerDetached, before: [typeof(ActionsSystem)]);

        SubscribeLocalEvent<BlobCoreComponent, BlobCreateBlobbernautActionEvent>(OnCreateBlobbernaut);
        SubscribeLocalEvent<BlobCoreComponent, BlobToCoreActionEvent>(OnBlobToCore);
        SubscribeLocalEvent<BlobCoreComponent, BlobSwapChemActionEvent>(OnBlobSwapChem);
        SubscribeLocalEvent<BlobCoreComponent, BlobSwapCoreActionEvent>(OnSwapCore);
        SubscribeLocalEvent<BlobCoreComponent, BlobSplitCoreActionEvent>(OnSplitCore);

        SubscribeLocalEvent<BlobObserverComponent, MoveEvent>(OnMoveEvent);
        SubscribeLocalEvent<BlobObserverComponent, BlobChemSwapPrototypeSelectedMessage>(OnChemSelected);

        SubscribeLocalEvent<BlobObserverComponent, ComponentStartup>(OnStartup);


        _logger = _logMan.GetSawmill("blob.core");
        _tileQuery = GetEntityQuery<BlobTileComponent>();
    }

    private void OnStartup(Entity<BlobObserverComponent> ent, ref ComponentStartup args)
    {
        _hands.AddHand(ent,"BlobHand",HandLocation.Middle);

        ent.Comp.VirtualItem = Spawn(MobObserverBlobController, Transform(ent).Coordinates);
        var comp = EnsureComp<BlobObserverControllerComponent>(ent.Comp.VirtualItem);
        comp.Blob = ent;
        Dirty(ent);

        if (!_hands.TryPickup(ent, ent.Comp.VirtualItem, "BlobHand", false, false, false))
        {
            QueueDel(ent);
        }
    }

    private void SendBlobBriefing(EntityUid mind)
    {
        if (_mindSystem.TryGetSession(mind, out var session))
        {
            _chatManager.DispatchServerMessage(session, Loc.GetString("blob-role-greeting"));
        }
    }

    private void OnCreateBlobObserver(EntityUid blobCoreUid, BlobCoreComponent core, CreateBlobObserverEvent args)
    {
        var observer = Spawn(core.ObserverBlobPrototype, Transform(blobCoreUid).Coordinates);

        core.Observer = observer;

        if (!TryComp<BlobObserverComponent>(observer, out var blobObserverComponent))
        {
            args.Cancel();
            return;
        }

        blobObserverComponent.Core = (blobCoreUid, core);
        Dirty(observer,blobObserverComponent);


        var isNewMind = false;
        if (!_mindSystem.TryGetMind(blobCoreUid, out var mindId, out var mind))
        {
            if (
                !_playerManager.TryGetSessionById(args.UserId, out var playerSession) ||
                playerSession.AttachedEntity == null ||
                !_mindSystem.TryGetMind(playerSession.AttachedEntity.Value, out mindId, out mind))
            {
                mindId = _mindSystem.CreateMind(args.UserId, "Blob Player");
                mind = Comp<MindComponent>(mindId);
                isNewMind = true;
            }
        }

        if (!isNewMind)
        {
            var name = mind.Session?.Name ?? "???";
            _mindSystem.WipeMind(mindId, mind);
            mindId = _mindSystem.CreateMind(args.UserId, $"Blob Player ({name})");
            mind = Comp<MindComponent>(mindId);
        }

        _roleSystem.MindAddRole(mindId, core.MindRoleBlobPrototypeId.Id);
        SendBlobBriefing(mindId);

        var blobRule = EntityQuery<BlobRuleComponent>().FirstOrDefault();
        blobRule?.Blobs.Add((mindId,mind));

        _mindSystem.TransferTo(mindId, observer, true, mind: mind);
        if (_actorSystem.TryGetSessionById(args.UserId, out var session))
        {
            _actorSystem.SetAttachedEntity(session, observer, true);
        }

        _mindSystem.TryAddObjective(mindId, mind, BlobCaptureObjective);

        UpdateUi(observer, core);
    }

    private void UpdateActions(ICommonSession playerSession, EntityUid uid, BlobObserverComponent? component = null)
    {
        if (!Resolve(uid, ref component))
        {
            return;
        }

        if (component.Core == null || TerminatingOrDeleted(component.Core.Value))
        {
            _logger.Error("It is not possible to find a core for the observer!");
            return;
        }

        _action.GrantActions(uid, component.Core.Value.Comp.Actions, component.Core.Value);
        _viewSubscriberSystem.AddViewSubscriber(component.Core.Value, playerSession); // GrantActions require keep in pvs
    }

    private void OnPlayerAttached(EntityUid uid, BlobObserverComponent component, PlayerAttachedEvent args)
    {
        UpdateActions(args.Player, uid, component);
        _blobCoreSystem.UpdateAllAlerts(component.Core!.Value);
    }

    private void OnPlayerDetached(EntityUid uid, BlobObserverComponent component, PlayerDetachedEvent args)
    {
        if (component.Core.HasValue && !TerminatingOrDeleted(component.Core.Value))
        {
            _viewSubscriberSystem.RemoveViewSubscriber(component.Core.Value, args.Player);
        }
    }

    private void OnBlobSwapChem(EntityUid uid,
        BlobCoreComponent blobCoreComponent,
        BlobSwapChemActionEvent args)
    {
        if (!TryComp<BlobObserverComponent>(args.Performer, out var observerComponent))
            return;

        TryOpenUi(args.Performer, args.Performer, observerComponent);
        args.Handled = true;
    }

    private void OnChemSelected(EntityUid uid, BlobObserverComponent component, BlobChemSwapPrototypeSelectedMessage args)
    {
        if (component.Core == null || !TryComp<BlobCoreComponent>(component.Core.Value, out var blobCoreComponent))
            return;

        if (component.SelectedChemId == args.SelectedId)
            return;

        if (!_blobCoreSystem.TryUseAbility(component.Core.Value, blobCoreComponent.SwapChemCost))
            return;

        if (!ChangeChem(uid, args.SelectedId, component))
            return;
    }

    private bool ChangeChem(EntityUid uid, BlobChemType newChem, BlobObserverComponent component)
    {
        if (component.Core == null || !TryComp<BlobCoreComponent>(component.Core.Value, out var blobCoreComponent))
            return false;

        var core = component.Core.Value;

        component.SelectedChemId = newChem;

        _blobCoreSystem.ChangeChem(core, newChem, blobCoreComponent);
        UpdateUi(uid, blobCoreComponent);

        return true;
    }

    private void TryOpenUi(EntityUid uid, EntityUid user, BlobObserverComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!TryComp(user, out ActorComponent? actor))
            return;

        _uiSystem.TryToggleUi(uid, BlobChemSwapUiKey.Key, actor.PlayerSession);
    }

    private void UpdateUi(EntityUid uid, BlobCoreComponent blobCoreComponent)
    {
        if (!TryComp<BlobObserverComponent>(uid, out var observerComponent))
        {
            return;
        }
        var state = new BlobChemSwapBoundUserInterfaceState(blobCoreComponent.ChemСolors, observerComponent.SelectedChemId);

        _uiSystem.SetUiState(uid, BlobChemSwapUiKey.Key, state);
    }

    // TODO: This is very bad, but it is clearly better than invisible walls, let someone do better.
    private void OnMoveEvent(EntityUid uid, BlobObserverComponent observerComponent, ref MoveEvent args)
    {
        if (observerComponent.IsProcessingMoveEvent)
            return;

        observerComponent.IsProcessingMoveEvent = true;

        var job = new BlobObserverMover(EntityManager, _blocker, _transform,this, MoverJobTime)
        {
            Observer = (uid,observerComponent),
            NewPosition = args.NewPosition
        };

        _moveJobQueue.EnqueueJob(job);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _moveJobQueue.Process();
    }

    private void OnSplitCore(EntityUid uid,
        BlobCoreComponent blobCoreComponent,
        BlobSplitCoreActionEvent args)
    {
        if (args.Handled)
            return;

        if (!blobCoreComponent.CanSplit)
        {
            _popup.PopupEntity(Loc.GetString("blob-cant-split"), args.Performer, args.Performer, PopupType.Large);
            return;
        }

        var gridUid = _transform.GetGrid(args.Target);

        if (!TryComp<MapGridComponent>(gridUid, out var grid))
        {
            return;
        }
        var centerTile = _mapSystem.GetLocalTilesIntersecting(gridUid.Value,
            grid,
            new Box2(args.Target.Position, args.Target.Position))
            .ToArray();

        EntityUid? blobTile = null;

        foreach (var tileref in centerTile)
        {
            foreach (var ent in _mapSystem.GetAnchoredEntities(gridUid.Value, grid,tileref.GridIndices))
            {
                if (!_tileQuery.HasComponent(ent))
                    continue;
                blobTile = ent;
                break;
            }
        }

        if (blobTile == null || !HasComp<BlobNodeComponent>(blobTile))
        {
            _popup.PopupEntity(Loc.GetString("blob-target-node-blob-invalid"), args.Performer, args.Performer, PopupType.Large);
            args.Handled = true;
            return;
        }

        if (!_blobCoreSystem.TryUseAbility((uid, blobCoreComponent), blobCoreComponent.SplitCoreCost))
        {
            args.Handled = true;
            return;
        }

        QueueDel(blobTile.Value);
        var newCore = Spawn(blobCoreComponent.TilePrototypes[BlobTileType.Core], args.Target);

        blobCoreComponent.CanSplit = false;
        _action.RemoveAction(args.Action);

        if (TryComp<BlobCoreComponent>(newCore, out var newBlobCoreComponent))
        {
            newBlobCoreComponent.CanSplit = false;
            newBlobCoreComponent.BlobTiles.Add(newCore);
        }

        args.Handled = true;
    }

    private void OnSwapCore(EntityUid uid,
        BlobCoreComponent blobCoreComponent,
        BlobSwapCoreActionEvent args)
    {
        if (args.Handled)
            return;

        var gridUid = _transform.GetGrid(args.Target);

        if (!TryComp<MapGridComponent>(gridUid, out var grid))
        {
            return;
        }

        var centerTile = _mapSystem.GetLocalTilesIntersecting(gridUid.Value,
            grid,
            new Box2(args.Target.Position, args.Target.Position))
            .ToArray();

        EntityUid? blobTile = null;

        foreach (var tileRef in centerTile)
        {
            foreach (var ent in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, tileRef.GridIndices))
            {
                if (!_tileQuery.HasComponent(ent))
                    continue;
                blobTile = ent;
                break;
            }
        }

        if (blobTile == null || !HasComp<BlobNodeComponent>(blobTile))
        {
            _popup.PopupEntity(Loc.GetString("blob-target-node-blob-invalid"), args.Performer, args.Performer, PopupType.Large);
            args.Handled = true;
            return;
        }

        if (!_blobCoreSystem.TryUseAbility((uid, blobCoreComponent), blobCoreComponent.SwapCoreCost))
        {
            args.Handled = true;
            return;
        }

        // Swap positions of blob's core and node.
        var nodePos = Transform(blobTile.Value).Coordinates;
        var corePos = Transform(uid).Coordinates;
        _transform.SetCoordinates(uid, nodePos.SnapToGrid());
        _transform.SetCoordinates(blobTile.Value, corePos.SnapToGrid());
        var xformCore = Transform(uid);
        if (!xformCore.Anchored)
        {
            _transform.AnchorEntity(uid, xformCore);
        }
        var xformNode = Transform(blobTile.Value);
        if (!xformNode.Anchored)
        {
            _transform.AnchorEntity(blobTile.Value, xformNode);
        }

        // And then swap their BlobNodeComponents, so they will work properly.

        _blobTileSystem.SwapSpecials(
            (blobTile.Value, EnsureComp<BlobNodeComponent>(blobTile.Value)),
            (uid, EnsureComp<BlobNodeComponent>(uid)));

        args.Handled = true;
    }

    private void OnCreateBlobbernaut(EntityUid uid,
        BlobCoreComponent blobCoreComponent,
        BlobCreateBlobbernautActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_blobCoreSystem.TryGetTargetBlobTile(args, out var blobTile))
            return;

        if (blobTile == null || !TryComp<BlobFactoryComponent>(blobTile, out var blobFactoryComponent))
        {
            _popup.PopupEntity(Loc.GetString("blob-target-factory-blob-invalid"), args.Performer, args.Performer, PopupType.LargeCaution);
            return;
        }

        if (blobFactoryComponent.Blobbernaut != null)
        {
            _popup.PopupEntity(Loc.GetString("blob-target-already-produce-blobbernaut"), args.Performer, args.Performer, PopupType.LargeCaution);
            return;
        }

        if (!_blobCoreSystem.TryUseAbility((uid, blobCoreComponent), blobCoreComponent.BlobbernautCost, args.Target.AlignWithClosestGridTile()))
            return;

        var ev = new ProduceBlobbernautEvent();
        RaiseLocalEvent(blobTile.Value, ev);

        _popup.PopupEntity(Loc.GetString("blob-spent-resource", ("point", blobCoreComponent.BlobbernautCost)),
            blobTile.Value,
            uid,
            PopupType.LargeCaution);

        args.Handled = true;
    }

    private void OnBlobToCore(EntityUid uid,
        BlobCoreComponent blobCoreComponent,
        BlobToCoreActionEvent args)
    {
        if (args.Handled)
            return;

        _transform.SetCoordinates(args.Performer, Transform(uid).Coordinates);
    }
}
