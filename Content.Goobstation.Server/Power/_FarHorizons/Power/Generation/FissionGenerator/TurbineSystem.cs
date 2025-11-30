using Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Administration.Logs;
using Content.Shared.Atmos;
using Content.Shared.Database;
using Content.Shared.Explosion.Components;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Random;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.Construction.Components;
using Robust.Shared.Physics;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

// Ported and modified from goonstation by Jhrushbe.
// CC-BY-NC-SA-3.0
// https://github.com/goonstation/goonstation/blob/ff86b044/code/obj/nuclearreactor/turbine.dm

public sealed class TurbineSystem : SharedTurbineSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = null!;
    [Dependency] private readonly DeviceLinkSystem _signal = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public event Action<string>? TurbineRepairMessage;

    private readonly float _threshold = 0.5f;
    private float _accumulator = 0f;

    private readonly List<string> _damageSoundList = [
        "/Audio/_FarHorizons/Effects/engine_grump1.ogg",
        "/Audio/_FarHorizons/Effects/engine_grump2.ogg",
        "/Audio/_FarHorizons/Effects/engine_grump3.ogg",
        "/Audio/Effects/metal_slam5.ogg",
        "/Audio/Effects/metal_scrape2.ogg"
    ];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TurbineComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<TurbineComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<TurbineComponent, AtmosDeviceUpdateEvent>(OnUpdate);
        SubscribeLocalEvent<TurbineComponent, GasAnalyzerScanEvent>(OnAnalyze);

        SubscribeLocalEvent<TurbineComponent, SignalReceivedEvent>(OnSignalReceived);

        SubscribeLocalEvent<TurbineComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<TurbineComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);
    }

    private void OnInit(EntityUid uid, TurbineComponent comp, ref ComponentInit args)
    {
        _signal.EnsureSourcePorts(uid, comp.SpeedHighPort, comp.SpeedLowPort);
        _signal.EnsureSinkPorts(uid, comp.StatorLoadIncreasePort, comp.StatorLoadDecreasePort);
    }

    private void OnAnalyze(EntityUid uid, TurbineComponent comp, ref GasAnalyzerScanEvent args)
    {
        if (!comp.InletEnt.HasValue || !comp.OutletEnt.HasValue)
            return;

        args.GasMixtures ??= [];

        if (_nodeContainer.TryGetNode(comp.InletEnt.Value, comp.PipeName, out PipeNode? inlet) && inlet.Air.Volume != 0f)
        {
            var inletAirLocal = inlet.Air.Clone();
            inletAirLocal.Multiply(inlet.Volume / inlet.Air.Volume);
            inletAirLocal.Volume = inlet.Volume;
            args.GasMixtures.Add((Loc.GetString("gas-analyzer-window-text-inlet"), inletAirLocal));
        }

        if (_nodeContainer.TryGetNode(comp.OutletEnt.Value, comp.PipeName, out PipeNode? outlet) && outlet.Air.Volume != 0f)
        {
            var outletAirLocal = outlet.Air.Clone();
            outletAirLocal.Multiply(outlet.Volume / outlet.Air.Volume);
            outletAirLocal.Volume = outlet.Volume;
            args.GasMixtures.Add((Loc.GetString("gas-analyzer-window-text-outlet"), outletAirLocal));
        }
    }

    private void OnShutdown(EntityUid uid, TurbineComponent comp, ref ComponentShutdown args)
    {
        QueueDel(comp.InletEnt);
        QueueDel(comp.OutletEnt);
    }

    #region Main Loop
    private void OnUpdate(EntityUid uid, TurbineComponent comp, ref AtmosDeviceUpdateEvent args)
    {
        var supplier = Comp<PowerSupplierComponent>(uid);
        comp.SupplierMaxSupply = supplier.MaxSupply;

        supplier.MaxSupply = comp.LastGen;

        if (!comp.InletEnt.HasValue || EntityManager.Deleted(comp.InletEnt.Value))
            comp.InletEnt = SpawnAttachedTo("TurbineGasPipe", new(uid, -1, -1), rotation: Angle.FromDegrees(-90));
        if (!comp.OutletEnt.HasValue || EntityManager.Deleted(comp.OutletEnt.Value))
            comp.OutletEnt = SpawnAttachedTo("TurbineGasPipe", new(uid, 1, -1), rotation: Angle.FromDegrees(90));

        CheckAnchoredPipes(uid, comp);

        if (!_nodeContainer.TryGetNode(comp.InletEnt.Value, comp.PipeName, out PipeNode? inlet))
            return;
        if (!_nodeContainer.TryGetNode(comp.OutletEnt.Value, comp.PipeName, out PipeNode? outlet))
            return;

        UpdateAppearance(uid, comp);

        var transferVolume = CalculateTransferVolume(comp, inlet, outlet, args.dt);

        var AirContents = inlet.Air.RemoveVolume(transferVolume) ?? new GasMixture();

        comp.LastVolumeTransfer = transferVolume;
        comp.LastGen = 0;
        comp.Overtemp = AirContents.Temperature >= comp.MaxTemp - 500;
        comp.Undertemp = AirContents.Temperature <= comp.MinTemp;

        // Dump gas into atmosphere
        if (comp.Ruined || AirContents.Temperature >= comp.MaxTemp)
        {
            var tile = _atmosphereSystem.GetTileMixture(uid, excite: true);

            if (tile != null)
            {
                _atmosphereSystem.Merge(tile, AirContents);
            }

            if (!comp.Ruined && !_audio.IsPlaying(comp.AlarmAudioOvertemp))
            {
                comp.AlarmAudioOvertemp = _audio.PlayPvs(new SoundPathSpecifier("/Audio/_FarHorizons/Machines/alarm_buzzer.ogg"), uid, AudioParams.Default.WithLoop(true))?.Entity;
                _popupSystem.PopupEntity(Loc.GetString("turbine-overheat", ("owner", uid)), uid, PopupType.LargeCaution);
            }

            // Prevent power from being generated by residual gasses
            AirContents.Clear();
        }
        else
        {
            comp.AlarmAudioOvertemp = _audio.Stop(comp.AlarmAudioOvertemp);
        }

        if (!comp.Ruined && AirContents != null)
        {
            var InputStartingEnergy = _atmosphereSystem.GetThermalEnergy(AirContents);
            var InputHeatCap = _atmosphereSystem.GetHeatCapacity(AirContents, true);

            // Prevents div by 0 if it would come up
            if (InputStartingEnergy <= 0)
            {
                InputStartingEnergy = 1;
            }
            if (InputHeatCap <= 0)
            {
                InputHeatCap = 1;
            }

            if (AirContents.Temperature > comp.MinTemp)
            {
                AirContents.Temperature = (float)Math.Max((InputStartingEnergy - ((InputStartingEnergy - (InputHeatCap * Atmospherics.T20C)) * 0.8)) / InputHeatCap, Atmospherics.T20C);
            }

            var OutputStartingEnergy = _atmosphereSystem.GetThermalEnergy(AirContents);
            var EnergyGenerated = comp.StatorLoad * (comp.RPM / 60);

            var DeltaE = InputStartingEnergy - OutputStartingEnergy;
            float NewRPM;

            if (DeltaE - EnergyGenerated > 0)
            {
                NewRPM = comp.RPM + (float)Math.Sqrt(2 * (Math.Max(DeltaE - EnergyGenerated, 0) / comp.TurbineMass));
            }
            else
            {
                NewRPM = comp.RPM - (float)Math.Sqrt(2 * (Math.Max(EnergyGenerated - DeltaE, 0) / comp.TurbineMass));
            }

            var NextGen = comp.StatorLoad * (Math.Max(NewRPM, 0) / 60);
            float NextRPM;

            if (DeltaE - NextGen > 0)
            {
                NextRPM = comp.RPM + (float)Math.Sqrt(2 * (Math.Max(DeltaE - NextGen, 0) / comp.TurbineMass));
            }
            else
            {
                NextRPM = comp.RPM - (float)Math.Sqrt(2 * (Math.Max(NextGen - DeltaE, 0) / comp.TurbineMass));
            }

            if (NewRPM < 0 || NextRPM < 0)
            {
                // Stator load is too high
                comp.Stalling = true;
                comp.RPM = 0;
            }
            else
            {
                comp.Stalling = false;
                comp.RPM = NextRPM;
            }

            if (!_audio.IsPlaying(comp.AlarmAudioUnderspeed) && !comp.Undertemp && comp.FlowRate > 0 && comp.Stalling)
                 PlayAudio(new SoundPathSpecifier("/Audio/_FarHorizons/Machines/alarm_beep.ogg"), uid, out comp.AlarmAudioUnderspeed, AudioParams.Default.WithLoop(true).WithVolume(-4));
            else if (_audio.IsPlaying(comp.AlarmAudioUnderspeed) && (comp.FlowRate <= 0 || comp.Undertemp || comp.RPM > 10))
                comp.AlarmAudioUnderspeed = _audio.Stop(comp.AlarmAudioUnderspeed);

            if (comp.RPM > 10)
            {
                // Sacrifices must be made to have a smooth ramp up:
                // This will generate 2 audio streams every second with up to 4 of them playing at once... surely this can't go wrong :clueless:
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/_FarHorizons/Ambience/Objects/turbine_room.ogg"), uid, AudioParams.Default.WithPitchScale(comp.RPM / comp.BestRPM).WithVolume(-2));
            }

            // Calculate power generation
            comp.LastGen = comp.PowerMultiplier * comp.StatorLoad * (comp.RPM / 30) * (float)(1 / Math.Cosh(0.01 * (comp.RPM - comp.BestRPM)));

            if (float.IsNaN(comp.LastGen))
                throw new NotFiniteNumberException("Turbine made NaN power");

            comp.Overspeed = comp.RPM > comp.BestRPM * 1.2;

            // Damage the turbines during overspeed, linear increase from 18% to 45% then stays at 45%
            if (comp.Overspeed && _random.NextFloat() < 0.15 * Math.Min(comp.RPM / comp.BestRPM, 3))
            {
                // TODO: damage flash
                _audio.PlayPvs(new SoundPathSpecifier(_damageSoundList[_random.Next(0, _damageSoundList.Count - 1)]), uid, AudioParams.Default.WithVariation(0.25f).WithVolume(-1));
                comp.BladeHealth--;
                UpdateHealthIndicators(uid, comp);
            }

            _atmosphereSystem.Merge(outlet.Air, AirContents);
        }

        // Explode
        if (!comp.Ruined && (comp.BladeHealth <= 0|| comp.RPM>comp.BestRPM*4))
        {
            TearApart(uid, comp);
        }

        // Send signals to device network
        if (comp.RPM > comp.BestRPM*1.05)
            _signal.InvokePort(uid, comp.SpeedHighPort);
        else if (comp.RPM < comp.BestRPM*0.95)
            _signal.InvokePort(uid, comp.SpeedLowPort);

        Dirty(uid, comp);
    }

    private float CalculateTransferVolume(TurbineComponent comp, PipeNode inlet, PipeNode outlet, float dt)
    {
        var wantToTransfer = comp.FlowRate * _atmosphereSystem.PumpSpeedup() * dt;
        var transferVolume = Math.Min(inlet.Air.Volume, wantToTransfer);
        var transferMoles = inlet.Air.Pressure * transferVolume / (inlet.Air.Temperature * Atmospherics.R);
        var molesSpaceLeft = (comp.OutputPressure - outlet.Air.Pressure) * outlet.Air.Volume / (outlet.Air.Temperature * Atmospherics.R);
        var actualMolesTransfered = Math.Clamp(transferMoles, 0, Math.Max(0, molesSpaceLeft));
        return Math.Max(0, actualMolesTransfered * inlet.Air.Temperature * Atmospherics.R / inlet.Air.Pressure);
    }

    private void TearApart(EntityUid uid, TurbineComponent comp)
    {
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/metal_break5.ogg"), uid, AudioParams.Default);
        _popupSystem.PopupEntity(Loc.GetString("turbine-explode", ("owner", uid)), uid, PopupType.LargeCaution);
        _explosion.TriggerExplosive(uid, Comp<ExplosiveComponent>(uid), false, comp.RPM/10, 5);
        ShootShrapnel(uid);
        _adminLogger.Add(LogType.Explosion, LogImpact.High, $"{ToPrettyString(uid)} destroyed by overspeeding for too long");
        comp.Ruined = true;
        comp.RPM = 0;
        UpdateAppearance(uid, comp);
    }

    private void ShootShrapnel(EntityUid uid)
    {
        var ShrapnelCount = _random.Next(5, 20);
        for (var i=0;i< ShrapnelCount; i++)
        {
            _gun.ShootProjectile(Spawn("TurbineBladeShrapnel", _transformSystem.GetMapCoordinates(uid)), _random.NextAngle().ToVec().Normalized(), _random.NextVector2(2, 6), uid, uid);
        }
    }
    #endregion

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
        var query = EntityQueryEnumerator<TurbineComponent>();

        while (query.MoveNext(out var uid, out var turbine))
        {
            UpdateUI(uid, turbine);
        }
    }

    protected override void UpdateUI(EntityUid uid, TurbineComponent turbine)
    {
        if (!_uiSystem.IsUiOpen(uid, TurbineUiKey.Key))
            return;

        _uiSystem.SetUiState(uid, TurbineUiKey.Key,
           new TurbineBuiState
           {
               Overspeed = turbine.Overspeed,
               Stalling = turbine.Stalling,
               Overtemp = turbine.Overtemp,
               Undertemp = turbine.Undertemp,

               RPM = turbine.RPM,
               BestRPM = turbine.BestRPM,

               FlowRateMin = 0,
               FlowRateMax = turbine.FlowRateMax,
               FlowRate = turbine.FlowRate,

               StatorLoadMin = 1000,
               StatorLoadMax = 500000,
               StatorLoad = turbine.StatorLoad,
           });
    }
    #endregion

    private void OnSignalReceived(EntityUid uid, TurbineComponent comp, ref SignalReceivedEvent args)
    {
        if (args.Port == comp.StatorLoadIncreasePort)
            AdjustStatorLoad(comp, 1000);
        else if (args.Port == comp.StatorLoadDecreasePort)
            AdjustStatorLoad(comp, -1000);

        _adminLogger.Add(LogType.Action, $"{ToPrettyString(args.Trigger):trigger} set the stator load on {ToPrettyString(uid):target} to {comp.StatorLoad}");
    }

    private void OnAnchorChanged(EntityUid uid, TurbineComponent comp, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            CleanUp(comp);
    }

    private void OnUnanchorAttempt(EntityUid uid, TurbineComponent comp, ref UnanchorAttemptEvent args)
    {
        if (comp.RPM>1)
        {
            _popupSystem.PopupEntity(Loc.GetString("turbine-unanchor-warning"), args.User, args.User, PopupType.LargeCaution);
            args.Cancel();
        }
    }

    private void CheckAnchoredPipes(EntityUid uid, TurbineComponent comp)
    {
        if (comp.InletEnt == null || comp.OutletEnt == null)
            return;

        if (!Transform(comp.InletEnt.Value).Anchored || !Transform(comp.OutletEnt.Value).Anchored)
        {
            _popupSystem.PopupEntity(Loc.GetString("turbine-anchor-warning"), uid, PopupType.MediumCaution);
            CleanUp(comp);
            _transform.Unanchor(uid);
        }
    }

    private void CleanUp(TurbineComponent comp)
    {
        QueueDel(comp.InletEnt);
        QueueDel(comp.OutletEnt);
    }
}
