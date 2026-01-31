using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Content.Server.Administration.Logs;
using Content.Server.DeviceLinking.Systems;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Database;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Robust.Server.GameObjects;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

public sealed partial class NuclearReactorMonitorSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly NuclearReactorSystem _reactorSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly DeviceLinkSystem _signal = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = null!;

    private readonly float _threshold = 0.5f;
    private float _accumulator = 0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NuclearReactorMonitorComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<NuclearReactorMonitorComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<NuclearReactorMonitorComponent, PortDisconnectedEvent>(OnPortDisconnected);

        SubscribeLocalEvent<NuclearReactorMonitorComponent, ReactorControlRodModifyMessage>(OnControlRodMessage);

        SubscribeLocalEvent<NuclearReactorMonitorComponent, AnchorStateChangedEvent>(OnAnchorChanged);
    }

    private void OnMapInit(EntityUid uid, NuclearReactorMonitorComponent comp, ref MapInitEvent args)
    {
        if (!_entityManager.TryGetComponent<DeviceLinkSinkComponent>(uid, out var sink))
            return;

        foreach(var source in sink.LinkedSources)
        {
            if (!HasComp<NuclearReactorComponent>(source))
                continue;

            comp.reactor = GetNetEntity(source);
            Dirty(uid, comp);
            return; // The return is to make it behave such that the first connetion that's a reactor is the one chosen
        }
    }

    private void OnNewLink(EntityUid uid, NuclearReactorMonitorComponent comp, ref NewLinkEvent args)
    {
        if (!HasComp<NuclearReactorComponent>(args.Source))
            return;

        comp.reactor = GetNetEntity(args.Source);
        Dirty(uid, comp);
    }

    private void OnPortDisconnected(EntityUid uid, NuclearReactorMonitorComponent comp, ref PortDisconnectedEvent args)
    {
        if (args.Port != comp.LinkingPort)
            return;

        comp.reactor = null;
        Dirty(uid, comp);
    }

    public bool TryGetReactorComp(NuclearReactorMonitorComponent reactorMonitor, [NotNullWhen(true)] out NuclearReactorComponent? reactorComponent)
    {
        reactorComponent = null;
        if (!_entityManager.TryGetEntity(reactorMonitor.reactor, out var reactorEnt) || reactorEnt == null)
            return false;

        if (!_entityManager.TryGetComponent<NuclearReactorComponent>(reactorEnt, out var reactor))
            return false;

        reactorComponent = reactor;
        return true;
    }

    #region BUI
    public override void Update(float frameTime)
    {
        _accumulator += frameTime;
        if (_accumulator > _threshold)
        {
            AccUpdate();
            _accumulator = 0;
        }
    }
    private void AccUpdate()
    {
        var query = EntityQueryEnumerator<NuclearReactorMonitorComponent>();

        while (query.MoveNext(out var uid, out var reactorMonitor))
        {
            CheckRange(uid, reactorMonitor);
            if (!TryGetReactorComp(reactorMonitor, out var reactor))
                continue;

            _reactorSystem.UpdateUI(uid, reactor);
        }
    }

    private void OnControlRodMessage(EntityUid uid, NuclearReactorMonitorComponent comp, ref ReactorControlRodModifyMessage args)
    {
        if (!TryGetReactorComp(comp, out var reactor))
            return;

        if (SharedNuclearReactorSystem.AdjustControlRods(reactor, args.Change))
            _adminLog.Add(LogType.Action, $"{ToPrettyString(args.Actor):actor} set control rod insertion of {ToPrettyString(comp.reactor):target} to {reactor.ControlRodInsertion} through {ToPrettyString(uid):monitor}");
        _reactorSystem.UpdateUI(uid, reactor);
    }
    #endregion

    private void OnAnchorChanged(EntityUid uid, NuclearReactorMonitorComponent comp, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            return;

        CheckRange(uid, comp);
    }

    private void CheckRange(EntityUid uid, NuclearReactorMonitorComponent comp)
    {
        if (!_entityManager.TryGetComponent<DeviceLinkSinkComponent>(uid, out var sink) || sink.LinkedSources.Count < 1)
            return;

        if (!_entityManager.TryGetEntity(comp.reactor, out var uidReactor))
            return;

        if (!_entityManager.TryGetComponent<DeviceLinkSourceComponent>(uidReactor, out var source))
            return;

        var xformMonitor = Transform(uid);
        var xformReactor = Transform(uidReactor.Value);
        var posMonitor = _transformSystem.GetWorldPosition(xformMonitor);
        var posReactor = _transformSystem.GetWorldPosition(xformReactor);

        if (xformMonitor.MapID == xformReactor.MapID && (posMonitor - posReactor).Length() <= source.Range)
            return;

        _uiSystem.CloseUi(uid, NuclearReactorUiKey.Key);
        comp.reactor = null;
        _signal.RemoveSinkFromSource(uidReactor.Value, uid, source, sink);
        Dirty(uid, comp);
    }
}
