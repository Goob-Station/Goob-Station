using System.Diagnostics.CodeAnalysis;
using Content.Server.Administration.Logs;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Database;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.IdentityManagement;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

public sealed partial class NuclearReactorMonitorSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = null!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly ReactorPartSystem _partSystem = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;

    private readonly float _threshold = 0.5f;
    private float _accumulator = 0f;

    private static readonly int _gridWidth = NuclearReactorComponent.ReactorGridWidth;
    private static readonly int _gridHeight = NuclearReactorComponent.ReactorGridHeight;

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
                return;

            UpdateUI(uid, reactor);
        }
    }

    private void UpdateUI(EntityUid uid, NuclearReactorComponent reactor)
    {
        if (!_uiSystem.IsUiOpen(uid, NuclearReactorUiKey.Key))
            return;

        var zoff = _gridWidth * _gridHeight;

        var temp = new double[_gridWidth * _gridHeight];
        var neutron = new int[_gridWidth * _gridHeight];
        var icon = new string[_gridWidth * _gridHeight];
        var partName = new string[_gridWidth * _gridHeight];
        var partInfo = new double[_gridWidth * _gridHeight * 3];

        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                var reactorPart = reactor.ComponentGrid[x, y];

                if (reactorPart != null && reactorPart.Properties == null)
                    _partSystem.SetProperties(reactorPart, out reactorPart.Properties);

                var pos = (x * _gridWidth) + y;
                temp[pos] = reactor.TemperatureGrid[x, y];
                neutron[pos] = reactor.NeutronGrid[x, y];
                icon[pos] = reactorPart != null ? reactorPart.IconStateInserted : "base";

                partName[pos] = reactorPart != null ? _prototypes.Index(reactorPart.ProtoId).Name : "empty";
                partInfo[pos] = reactorPart != null ? reactorPart.Properties!.NeutronRadioactivity : 0;
                partInfo[pos + zoff] = reactorPart != null ? reactorPart.Properties!.Radioactivity : 0;
                partInfo[pos + (zoff * 2)] = reactorPart != null ? reactorPart.Properties!.FissileIsotopes : 0;
            }
        }

        // This is transmitting close to 2.3KB of data every time it's called... ouch
        _uiSystem.SetUiState(uid, NuclearReactorUiKey.Key,
           new NuclearReactorBuiState
           {
               TemperatureGrid = temp,
               NeutronGrid = neutron,
               IconGrid = icon,
               PartInfo = partInfo,
               PartName = partName,
               ItemName = reactor.PartSlot.Item != null ? Identity.Name(reactor.PartSlot.Item.Value, _entityManager) : null,

               ReactorTemp = reactor.Temperature,
               ReactorRads = reactor.RadiationLevel,
               ReactorTherm = reactor.ThermalPower,

               ControlRodActual = reactor.AvgInsertion,
               ControlRodSet = reactor.ControlRodInsertion,
           });
    }
    private void OnControlRodMessage(EntityUid uid, NuclearReactorMonitorComponent comp, ref ReactorControlRodModifyMessage args)
    {
        if (!TryGetReactorComp(comp, out var reactor))
            return;

        if (AdjustControlRods(reactor, args.Change))
            _adminLog.Add(LogType.Action, $"{ToPrettyString(args.Actor):actor} set control rod insertion of {ToPrettyString(comp.reactor):target} to {reactor.ControlRodInsertion} through {ToPrettyString(uid):monitor}");
        UpdateUI(uid, reactor);
    }

    private static bool AdjustControlRods(NuclearReactorComponent comp, float change)
    {
        var newSet = Math.Clamp(comp.ControlRodInsertion + change, 0, 2);
        if (comp.ControlRodInsertion != newSet)
        {
            comp.ControlRodInsertion = newSet;
            return true;
        }
        return false;
    }
    #endregion
}
