using System.Linq;
using Content.Server.Construction.Completions;
using Content.Server.Popups;
using Content.Shared.VentCrawler.Tube.Components;
using Content.Shared._Starlight.VentCrawling.Components;
using Content.Shared.Tools.Components;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Systems;
using Content.Shared._Starlight.VentCrawling;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Server.Inventory;
using Content.Shared.Hands.EntitySystems;

namespace Content.Server._Starlight.VentCrawling;
public sealed class VentCrawlerTubeSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedVentCrawableSystem _ventCrawableSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly VentCrawlerTubeSystem _VentCrawlerTubeSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VentCrawlerTubeComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<VentCrawlerTubeComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<VentCrawlerTubeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<VentCrawlerTubeComponent, AnchorStateChangedEvent>(OnAnchorChange);
        SubscribeLocalEvent<VentCrawlerTubeComponent, BreakageEventArgs>(OnBreak);
        SubscribeLocalEvent<VentCrawlerTubeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<VentCrawlerTubeComponent, ConstructionBeforeDeleteEvent>(OnDeconstruct);
        SubscribeLocalEvent<VentCrawlerBendComponent, GetVentCrawlingsConnectableDirectionsEvent>(OnGetBendConnectableDirections);
        SubscribeLocalEvent<VentCrawlerEntryComponent, GetVentCrawlingsConnectableDirectionsEvent>(OnGetEntryConnectableDirections);
        SubscribeLocalEvent<VentCrawlerJunctionComponent, GetVentCrawlingsConnectableDirectionsEvent>(OnGetJunctionConnectableDirections);
        SubscribeLocalEvent<VentCrawlerTransitComponent, GetVentCrawlingsConnectableDirectionsEvent>(OnGetTransitConnectableDirections);
        SubscribeLocalEvent<VentCrawlerEntryComponent, GetVerbsEvent<AlternativeVerb>>(AddClimbedVerb);
        SubscribeLocalEvent<VentCrawlerComponent, EnterVentDoAfterEvent>(OnDoAfterEnterTube);
    }

    private void AddClimbedVerb(EntityUid uid, VentCrawlerEntryComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!TryComp<VentCrawlerComponent>(args.User, out var ventCrawlerComponent) || HasComp<BeingVentCrawlerComponent>(args.User))
            return;

        if (TryComp<TransformComponent>(uid, out var transformComponent) && !transformComponent.Anchored)
            return;

        AlternativeVerb verb = new()
        {
            Act = () => TryEnter(uid, args.User, ventCrawlerComponent),
            Text = Loc.GetString("ventcrawling-enter-pipe-network")
        };
        args.Verbs.Add(verb);
    }

    private void OnDoAfterEnterTube(EntityUid uid, VentCrawlerComponent component, EnterVentDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target == null || args.Args.Used == null)
            return;

        if (!component.AllowInventory && IsHoldingItems(args.Args.Used.Value))
            return;

        TryInsert(args.Args.Target.Value, args.Args.Used.Value);

        args.Handled = true;
    }

    private void TryEnter(EntityUid uid, EntityUid user, VentCrawlerComponent crawler)
    {
        if (TryComp<WeldableComponent>(uid, out var weldableComponent) && weldableComponent.IsWelded)
            {
                _popup.PopupEntity(Loc.GetString("entity-storage-component-welded-shut-message"), user);
                return;
            }

        if (!crawler.AllowInventory && IsHoldingItems(user))
            return;

        var args = new DoAfterArgs(EntityManager, user, crawler.EnterDelay, new EnterVentDoAfterEvent(), user, uid, user)
        {
            BreakOnMove = true,
            BreakOnDamage = false
        };

        _doAfterSystem.TryStartDoAfter(args);
    }

    private void OnComponentInit(EntityUid uid, VentCrawlerTubeComponent tube, ComponentInit args)
        => tube.Contents = _containerSystem.EnsureContainer<Container>(uid, tube.ContainerId);

    private void OnComponentRemove(EntityUid uid, VentCrawlerTubeComponent tube, ComponentRemove args)
        => DisconnectTube(uid, tube);

    private void OnShutdown(EntityUid uid, VentCrawlerTubeComponent tube, ComponentShutdown args)
        => DisconnectTube(uid, tube);

    private void OnGetBendConnectableDirections(EntityUid uid, VentCrawlerBendComponent component, ref GetVentCrawlingsConnectableDirectionsEvent args)
    {
        var direction = Transform(uid).LocalRotation;
        var side = new Angle(MathHelper.DegreesToRadians(direction.Degrees - 90));

        args.Connectable = new[] { direction.GetDir(), side.GetDir() };
    }

    private void OnGetEntryConnectableDirections(EntityUid uid, VentCrawlerEntryComponent component, ref GetVentCrawlingsConnectableDirectionsEvent args)
        => args.Connectable = new[] { Transform(uid).LocalRotation.GetDir() };

    private void OnGetJunctionConnectableDirections(EntityUid uid, VentCrawlerJunctionComponent component, ref GetVentCrawlingsConnectableDirectionsEvent args)
    {
        var direction = Transform(uid).LocalRotation;

        args.Connectable = component.Degrees
            .Select(degree => new Angle(degree.Theta + direction.Theta).GetDir())
            .ToArray();
    }

    private void OnGetTransitConnectableDirections(EntityUid uid, VentCrawlerTransitComponent component, ref GetVentCrawlingsConnectableDirectionsEvent args)
    {
        var rotation = Transform(uid).LocalRotation;
        var opposite = new Angle(rotation.Theta + Math.PI);

        args.Connectable = new[] { rotation.GetDir(), opposite.GetDir() };
    }

    private void OnDeconstruct(EntityUid uid, VentCrawlerTubeComponent component, ConstructionBeforeDeleteEvent args)
        => DisconnectTube(uid, component);

    private void OnMapInit(EntityUid uid, VentCrawlerTubeComponent component, MapInitEvent args)
        => UpdateAnchored(uid, component, Transform(uid).Anchored);

    private void OnBreak(EntityUid uid, VentCrawlerTubeComponent component, BreakageEventArgs args)
        => DisconnectTube(uid, component);

    private void OnAnchorChange(EntityUid uid, VentCrawlerTubeComponent component, ref AnchorStateChangedEvent args)
        => UpdateAnchored(uid, component, args.Anchored);

    private void UpdateAnchored(EntityUid uid, VentCrawlerTubeComponent component, bool anchored)
    {
        if (anchored)
            ConnectTube(uid, component);
        else
            DisconnectTube(uid, component);
    }

    private static void ConnectTube(EntityUid _, VentCrawlerTubeComponent tube)
    {
        if (tube.Connected)
            return;

        tube.Connected = true;
    }


    private void DisconnectTube(EntityUid _, VentCrawlerTubeComponent tube)
    {
        if (!tube.Connected)
            return;

        tube.Connected = false;

        var query = GetEntityQuery<VentCrawlerHolderComponent>();
        foreach (var entity in tube.Contents.ContainedEntities.ToArray())
        {
            if (query.TryGetComponent(entity, out var holder))
            {
                var Exitev = new VentCrawlingExitEvent();
                RaiseLocalEvent(entity, ref Exitev);
            }
        }
    }

    private bool TryInsert(EntityUid uid, EntityUid entity, VentCrawlerEntryComponent? entry = null)
    {
        if (!Resolve(uid, ref entry))
            return false;

        if (!TryComp<VentCrawlerComponent>(entity, out var ventCrawlerComponent))
            return false;

        var holder = Spawn(entry.HolderPrototypeId, Transform(uid).Coordinates);
        var holderComponent = Comp<VentCrawlerHolderComponent>(holder);

        _ventCrawableSystem.TryInsert(holder, entity, holderComponent);

        _mover.ResetCamera(entity);
        _mover.SetRelay(entity, holder);
        ventCrawlerComponent.InTube = true;
        Dirty(entity, ventCrawlerComponent);

        return _ventCrawableSystem.EnterTube(holder, uid, holderComponent);
    }

    private bool IsHoldingItems (EntityUid uid)
    {
        if (_inventory.TryGetSlotEntity(uid, "outerClothing", out var suit) || _inventory.TryGetSlotEntity(uid, "back", out var backpack))
        {
            _popup.PopupEntity(Loc.GetString("ventcrawling-block-enter-reson-equiptment"), uid);
            return true;
        }
        if (_hands.TryGetHand(uid, "body_part_slot_right hand", out var rhand) && !rhand.IsEmpty)
        {
            _popup.PopupEntity(Loc.GetString("ventcrawling-block-enter-reson-hand"), uid);
            return true;
        }
        if (_hands.TryGetHand(uid, "body_part_slot_left hand", out var lhand) && !lhand.IsEmpty)
        {
            _popup.PopupEntity(Loc.GetString("ventcrawling-block-enter-reson-hand"), uid);
            return true;
        }

        return false;
    }
}
