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

public sealed partial class GasTurbineMonitorSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly TurbineSystem _turbineSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly DeviceLinkSystem _signal = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = null!;

    private readonly float _threshold = 0.5f;
    private float _accumulator = 0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasTurbineMonitorComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<GasTurbineMonitorComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<GasTurbineMonitorComponent, PortDisconnectedEvent>(OnPortDisconnected);

        SubscribeLocalEvent<GasTurbineMonitorComponent, TurbineChangeFlowRateMessage>(OnTurbineFlowRateChanged);
        SubscribeLocalEvent<GasTurbineMonitorComponent, TurbineChangeStatorLoadMessage>(OnTurbineStatorLoadChanged);

        SubscribeLocalEvent<GasTurbineMonitorComponent, AnchorStateChangedEvent>(OnAnchorChanged);
    }

    private void OnMapInit(EntityUid uid, GasTurbineMonitorComponent comp, ref MapInitEvent args)
    {
        if (!_entityManager.TryGetComponent<DeviceLinkSinkComponent>(uid, out var sink))
            return;

        foreach (var source in sink.LinkedSources)
        {
            if (!HasComp<TurbineComponent>(source))
                continue;

            comp.turbine = GetNetEntity(source);
            Dirty(uid, comp);
            return; // The return is to make it behave such that the first connetion that's a turbine is the one chosen
        }
    }

    private void OnNewLink(EntityUid uid, GasTurbineMonitorComponent comp, ref NewLinkEvent args)
    {
        if (!HasComp<TurbineComponent>(args.Source))
            return;

        comp.turbine = GetNetEntity(args.Source);
        Dirty(uid, comp);
    }

    private void OnPortDisconnected(EntityUid uid, GasTurbineMonitorComponent comp, ref PortDisconnectedEvent args)
    {
        if (args.Port != comp.LinkingPort)
            return;

        comp.turbine = null;
        Dirty(uid, comp);
    }

    public bool TryGetTurbineComp(GasTurbineMonitorComponent turbineMonitor, [NotNullWhen(true)] out TurbineComponent? turbineComponent)
    {
        turbineComponent = null;
        if (!_entityManager.TryGetEntity(turbineMonitor.turbine, out var turbineUid) || turbineUid == null)
            return false;

        if (!_entityManager.TryGetComponent<TurbineComponent>(turbineUid, out var turbine))
            return false;

        turbineComponent = turbine;
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
        var query = EntityQueryEnumerator<GasTurbineMonitorComponent>();

        while (query.MoveNext(out var uid, out var turbineMonitor))
        {
            CheckRange(uid, turbineMonitor);
            if (!TryGetTurbineComp(turbineMonitor, out var turbine))
                continue;

            _turbineSystem.UpdateUI(uid, turbine);
        }
    }

    private void OnTurbineFlowRateChanged(EntityUid uid, GasTurbineMonitorComponent comp, TurbineChangeFlowRateMessage args)
    {
        if (!TryGetTurbineComp(comp, out var turbine) || !_entityManager.TryGetEntity(comp.turbine, out var turbineUid))
            return;

        turbine.FlowRate = Math.Clamp(args.FlowRate, 0f, turbine.FlowRateMax);
        Dirty(turbineUid.Value, turbine);
        _turbineSystem.UpdateUI(uid, turbine);
        _adminLog.Add(LogType.AtmosVolumeChanged, LogImpact.Medium,
            $"{ToPrettyString(args.Actor):player} set the flow rate on {ToPrettyString(uid):device} to {args.FlowRate} through {ToPrettyString(uid):monitor}");
    }

    private void OnTurbineStatorLoadChanged(EntityUid uid, GasTurbineMonitorComponent comp, TurbineChangeStatorLoadMessage args)
    {
        if (!TryGetTurbineComp(comp, out var turbine) || !_entityManager.TryGetEntity(comp.turbine, out var turbineUid))
            return;

        turbine.StatorLoad = Math.Clamp(args.StatorLoad, 1000f, turbine.StatorLoadMax);
        Dirty(turbineUid.Value, turbine);
        _turbineSystem.UpdateUI(uid, turbine);
        _adminLog.Add(LogType.AtmosDeviceSetting, LogImpact.Medium,
            $"{ToPrettyString(args.Actor):player} set the stator load on {ToPrettyString(uid):device} to {args.StatorLoad} through {ToPrettyString(uid):monitor}");
    }
    #endregion

    private void OnAnchorChanged(EntityUid uid, GasTurbineMonitorComponent comp, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            return;

        CheckRange(uid, comp);
    }

    private void CheckRange(EntityUid uid, GasTurbineMonitorComponent comp)
    {
        if (!_entityManager.TryGetComponent<DeviceLinkSinkComponent>(uid, out var sink) || sink.LinkedSources.Count < 1)
            return;

        if (!_entityManager.TryGetEntity(comp.turbine, out var uidTurbine))
            return;

        if (!_entityManager.TryGetComponent<DeviceLinkSourceComponent>(uidTurbine, out var source))
            return;

        var xformMonitor = Transform(uid);
        var xformReactor = Transform(uidTurbine.Value);
        var posMonitor = _transformSystem.GetWorldPosition(xformMonitor);
        var posReactor = _transformSystem.GetWorldPosition(xformReactor);

        if (xformMonitor.MapID == xformReactor.MapID && (posMonitor - posReactor).Length() <= source.Range)
            return;

        _uiSystem.CloseUi(uid, TurbineUiKey.Key);
        comp.turbine = null;
        _signal.RemoveSinkFromSource(uidTurbine.Value, uid, source, sink);
        Dirty(uid, comp);
    }
}
