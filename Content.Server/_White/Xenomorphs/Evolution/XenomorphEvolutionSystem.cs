using System.Linq;
using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.DoAfter;
using Content.Server.Jittering;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._White.RadialSelector;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._White.Xenomorphs.Evolution;

public sealed class XenomorphEvolutionSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly JitteringSystem _jitter = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphEvolutionComponent, MapInitEvent>(OnXenomorphEvolutionMapInit);
        SubscribeLocalEvent<XenomorphEvolutionComponent, ComponentShutdown>(OnXenomorphEvolutionShutdown);
        SubscribeLocalEvent<XenomorphEvolutionComponent, EvolutionsActionEvent>(OnEvolutionsAction);
        SubscribeLocalEvent<XenomorphEvolutionComponent, RadialSelectorSelectedMessage>(OnEvolutionRecieved);
        SubscribeLocalEvent<XenomorphEvolutionComponent, XenomorphEvolutionDoAfterEvent>(OnXenomorphEvolutionDoAfter);
    }

    private void OnXenomorphEvolutionMapInit(EntityUid uid, XenomorphEvolutionComponent component, MapInitEvent args) =>
        _actions.AddAction(uid, ref component.EvolutionAction, component.EvolutionActionId);

    private void OnXenomorphEvolutionShutdown(EntityUid uid, XenomorphEvolutionComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.EvolutionAction);

    private void OnEvolutionsAction(EntityUid uid, XenomorphEvolutionComponent component, ref EvolutionsActionEvent args)
    {
        if (args.Handled)
            return;

        if (component.EvolvesTo.Count == 1)
        {
            if (component.Points < component.Max)
            {
                _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-not-enough-points", ("seconds", (component.Max - component.Points) / component.PointsPerSecond)), uid, uid);
                return;
            }

            args.Handled = Evolve(uid, component.EvolvesTo.First().Prototype, component.EvolutionDelay);
            return;
        }

        _ui.TryToggleUi(uid, RadialSelectorUiKey.Key, uid);
        _ui.SetUiState(uid, RadialSelectorUiKey.Key, new TrackedRadialSelectorState(component.EvolvesTo));

        args.Handled = true;
    }

    private void OnEvolutionRecieved(EntityUid uid, XenomorphEvolutionComponent component, RadialSelectorSelectedMessage args)
    {
        if (component.Points < component.Max)
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-not-enough-points", ("seconds", (component.Max - component.Points) / component.PointsPerSecond)), uid, uid);
            return;
        }

        if (Evolve(uid, args.SelectedItem, component.EvolutionDelay))
            return;

        var actor = args.Actor;
        _ui.CloseUi(uid, RadialSelectorUiKey.Key, actor);
    }

    private void OnXenomorphEvolutionDoAfter(EntityUid uid, XenomorphEvolutionComponent component, ref XenomorphEvolutionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !_mind.TryGetMind(uid, out var mindUid, out var mind))
            return;

        var ev = new BeforeXenomorphEvolutionEvent(args.Caste);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return;

        args.Handled = true;

        var coordinates = _transform.GetMoverCoordinates(uid);
        var newXeno = Spawn(args.Choice, coordinates);

        _mind.TransferTo(mindUid, newXeno, mind:mind);
        _mind.UnVisit(mindUid, mind);

        var dropHandItemsEvent = new DropHandItemsEvent();
        RaiseLocalEvent(uid, ref dropHandItemsEvent);
        RaiseLocalEvent(uid, new AfterXenomorphEvolutionEvent(newXeno, mindUid, args.Caste));

        _adminLog.Add(LogType.Mind, $"{ToPrettyString(uid)} evolved into {ToPrettyString(newXeno)}");

        Del(uid);

        _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-end"), newXeno, newXeno);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<XenomorphEvolutionComponent>();
        while (query.MoveNext(out var uid, out var alienEvolution))
        {
            if (alienEvolution.Points == alienEvolution.Max || time < alienEvolution.NextPointsAt || _container.IsEntityInContainer(uid))
                continue;

            alienEvolution.NextPointsAt = time + TimeSpan.FromSeconds(1);
            alienEvolution.Points += alienEvolution.PointsPerSecond;

            if (alienEvolution.Points != alienEvolution.Max)
                continue;

            _popup.PopupEntity(Loc.GetString("xenomorphs-evolution-ready"), uid, uid, PopupType.Large);
        }
    }

    public bool Evolve(EntityUid uid, string? evolveTo, TimeSpan evolutionDelay, bool checkNeedCasteDeath = true)
    {
        if (evolveTo == null
            || !_protoManager.TryIndex(evolveTo, out var xenomorphPrototype)
            || !xenomorphPrototype.TryGetComponent<XenomorphComponent>(out var xenomorph, _componentFactory)
            || !_mind.TryGetMind(uid, out _, out _))
            return false;

        var ev = new BeforeXenomorphEvolutionEvent(xenomorph.Caste, checkNeedCasteDeath);
        RaiseLocalEvent(uid, ev);

        if (ev.Cancelled)
            return false;

        var doAfterEvent = new XenomorphEvolutionDoAfterEvent(evolveTo, xenomorph.Caste, checkNeedCasteDeath);
        var doAfter = new DoAfterArgs(EntityManager, uid, evolutionDelay, doAfterEvent, uid);

        if (!_doAfter.TryStartDoAfter(doAfter))
            return false;

        _jitter.DoJitter(uid, evolutionDelay, true, 80, 8, true);

        var popupOthers = Loc.GetString("xenomorphs-evolution-start-others", ("uid", uid));
        _popup.PopupEntity(popupOthers, uid, Filter.PvsExcept(uid), true, PopupType.Medium);

        var popupSelf = Loc.GetString("xenomorphs-evolution-start-self");
        _popup.PopupEntity(popupSelf, uid, uid, PopupType.Medium);

        return true;
    }
}
