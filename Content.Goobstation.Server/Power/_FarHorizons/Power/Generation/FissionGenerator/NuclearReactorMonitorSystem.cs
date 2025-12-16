using System.Diagnostics.CodeAnalysis;
using Content.Server.Administration.Logs;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Database;
using Content.Shared.DeviceLinking.Events;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

public sealed partial class NuclearReactorMonitorSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly NuclearReactorSystem _reactorSystem = default!;

    private readonly float _threshold = 0.5f;
    private float _accumulator = 0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NuclearReactorMonitorComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<NuclearReactorMonitorComponent, PortDisconnectedEvent>(OnPortDisconnected);

        SubscribeLocalEvent<NuclearReactorMonitorComponent, ReactorControlRodModifyMessage>(OnControlRodMessage);
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
}
