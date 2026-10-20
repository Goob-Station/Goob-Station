using Content.Shared.Ghost;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Movement.Systems;
using Content.Shared.Silicons.StationAi;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Client.Silicons.StationAi;

public sealed partial class StationAiSystem : SharedStationAiSystem
{
    [Dependency] private readonly IOverlayManager _overlayMgr = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private StationAiOverlay? _overlay;
    private StationAiCrewMonitoringOverlay? _crewMonitoringOverlay;

    public List<SuitSensorStatus> ActiveCrewSensors = new();

    public override void Initialize()
    {
        base.Initialize();
        InitializeAirlock();
        InitializePowerToggle();

        UpdatesOutsidePrediction = true;
        CommandBinds.Builder
            .Bind(EngineKeyFunctions.Use, new PointerInputCmdHandler(OnUse, outsidePrediction: true))
            .Register<StationAiSystem>();

        SubscribeLocalEvent<StationAiOverlayComponent, LocalPlayerAttachedEvent>(OnAiAttached);
        SubscribeLocalEvent<StationAiOverlayComponent, LocalPlayerDetachedEvent>(OnAiDetached);
        SubscribeLocalEvent<StationAiOverlayComponent, ComponentInit>(OnAiOverlayInit);
        SubscribeLocalEvent<StationAiOverlayComponent, ComponentRemove>(OnAiOverlayRemove);
        SubscribeLocalEvent<StationAiCoreComponent, AppearanceChangeEvent>(OnAppearanceChange);

        SubscribeNetworkEvent<StationAiCrewMonitoringSensorsEvent>(OnCrewSensorsReceived);
    }

    private bool OnUse(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        if (args.State != BoundKeyState.Down)
            return false;

        if (_player.LocalEntity is not { } playerEnt)
            return false;

        if (!HasComp<StationAiHeldComponent>(playerEnt))
            return false;

        if (!TryComp<EyeComponent>(playerEnt, out var eye) || eye.Zoom.X <= StationAiCrewMonitoringOverlay.CrewMonitoringZoomThreshold || eye.Zoom.X > StationAiCrewMonitoringOverlay.MassScannerZoomThreshold)
            return false;

        var clickMapPos = _transform.ToMapCoordinates(args.Coordinates);
        if (clickMapPos.MapId == MapId.Nullspace)
            return false;

        SuitSensorStatus? closestSensor = null;
        var closestDistSq = 3.0f * 3.0f; // Within 3 tiles radius on screen

        foreach (var sensor in ActiveCrewSensors)
        {
            if (sensor.Coordinates is not { } netCoords)
                continue;

            var sensorMapCoords = _transform.ToMapCoordinates(netCoords);
            if (sensorMapCoords.MapId != clickMapPos.MapId)
                continue;

            var distSq = (sensorMapCoords.Position - clickMapPos.Position).LengthSquared();
            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closestSensor = sensor;
            }
        }

        if (closestSensor != null)
        {
            TeleportToTarget(closestSensor.OwnerUid);
            return true;
        }

        return false;
    }

    private void OnCrewSensorsReceived(StationAiCrewMonitoringSensorsEvent ev)
    {
        ActiveCrewSensors = ev.Sensors;
    }

    protected override void OnHeldZoomChanged(Entity<StationAiHeldComponent> ent, ref EyeZoomChangedEvent args)
    {
        base.OnHeldZoomChanged(ent, ref args);

        if (args.Zoom.X > StationAiCrewMonitoringOverlay.CrewMonitoringZoomThreshold && args.Zoom.X <= StationAiCrewMonitoringOverlay.MassScannerZoomThreshold)
        {
            RaiseNetworkEvent(new StationAiRequestCrewMonitoringSensorsEvent());
        }
    }

    public void TeleportToTarget(NetEntity target)
    {
        if (_player.LocalEntity is not { } local || (!HasComp<StationAiHeldComponent>(local) && !HasComp<GhostComponent>(local)))
            return;

        RaiseNetworkEvent(new StationAiTeleportToTargetEvent(target));
    }

    public void TeleportToSpeaker(NetEntity speaker) => TeleportToTarget(speaker);

    private void OnAiOverlayInit(Entity<StationAiOverlayComponent> ent, ref ComponentInit args)
    {
        var attachedEnt = _player.LocalEntity;

        if (attachedEnt != ent.Owner)
            return;

        AddOverlay();
    }

    private void OnAiOverlayRemove(Entity<StationAiOverlayComponent> ent, ref ComponentRemove args)
    {
        var attachedEnt = _player.LocalEntity;

        if (attachedEnt != ent.Owner)
            return;

        RemoveOverlay();
    }

    private void AddOverlay()
    {
        if (_overlay == null)
        {
            _overlay = new StationAiOverlay();
            _overlayMgr.AddOverlay(_overlay);
        }

        if (_crewMonitoringOverlay == null)
        {
            _crewMonitoringOverlay = new StationAiCrewMonitoringOverlay();
            _overlayMgr.AddOverlay(_crewMonitoringOverlay);
        }
    }

    private void RemoveOverlay()
    {
        if (_overlay != null)
        {
            _overlayMgr.RemoveOverlay(_overlay);
            _overlay = null;
        }

        if (_crewMonitoringOverlay != null)
        {
            _overlayMgr.RemoveOverlay(_crewMonitoringOverlay);
            _crewMonitoringOverlay = null;
        }
    }

    private void OnAiAttached(Entity<StationAiOverlayComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        AddOverlay();
    }

    private void OnAiDetached(Entity<StationAiOverlayComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        RemoveOverlay();
    }

    private void OnAppearanceChange(Entity<StationAiCoreComponent> entity, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_appearance.TryGetData<PrototypeLayerData>(entity.Owner, StationAiVisualLayers.Icon, out var layerData, args.Component))
            _sprite.LayerSetData((entity.Owner, args.Sprite), StationAiVisualLayers.Icon, layerData);

        _sprite.LayerSetVisible((entity.Owner, args.Sprite), StationAiVisualLayers.Icon, layerData != null);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<StationAiSystem>();
        _overlayMgr.RemoveOverlay<StationAiOverlay>();
        _overlayMgr.RemoveOverlay<StationAiCrewMonitoringOverlay>();
    }
}