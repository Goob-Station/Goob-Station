using Content.Server._FarHorizons.NodeContainer.Nodes;
using Content.Server.Administration.Logs;
using Content.Server.AlertLevel;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Atmos;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Content.Shared.Radiation.Components;
using Content.Shared.Radio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using Content.Shared.Atmos.Piping.Components;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

public sealed class NuclearReactorSystem : SharedNuclearReactorSystem
{
    // The great wall of dependencies
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly NodeGroupSystem _nodeGroupSystem = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;
    [Dependency] private readonly ReactorPartSystem _partSystem = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _soundSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = null!;

    private static readonly int _gridWidth = NuclearReactorComponent.ReactorGridWidth;
    private static readonly int _gridHeight = NuclearReactorComponent.ReactorGridHeight;
    private RadioChannelPrototype? _engi;

    private readonly float _threshold = 0.5f;
    private float _accumulator = 0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NuclearReactorComponent, AtmosDeviceUpdateEvent>(OnUpdate);
        SubscribeLocalEvent<NuclearReactorComponent, AtmosDeviceEnabledEvent>(OnEnable);

        // Item events
        SubscribeLocalEvent<NuclearReactorComponent, EntInsertedIntoContainerMessage>(OnPartChanged);
        SubscribeLocalEvent<NuclearReactorComponent, EntRemovedFromContainerMessage>(OnPartChanged);

        // BUI events
        SubscribeLocalEvent<NuclearReactorComponent, ReactorItemActionMessage>(OnItemActionMessage);
        SubscribeLocalEvent<NuclearReactorComponent, ReactorControlRodModifyMessage>(OnControlRodMessage);
    }

    private void OnEnable(Entity<NuclearReactorComponent> ent, ref AtmosDeviceEnabledEvent args)
    {
        var comp = ent.Comp;
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                comp.FluxGrid[x, y] = [];
            }
        }
    }

    private void OnPartChanged(EntityUid uid, NuclearReactorComponent component, ContainerModifiedMessage args) => ReactorTryGetSlot(uid, "part_slot", out component.PartSlot!);

    #region Main Loop
    private void OnUpdate(Entity<NuclearReactorComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        var comp = ent.Comp;
        var uid = ent.Owner;

        _appearance.SetData(uid, ReactorVisuals.Sprite, comp.Melted ? Reactors.Melted : Reactors.Normal);

        ProcessCaseRadiation(ent);

        if (comp.Melted)
            return;

        if (comp.VisualGrid[0, 0].Id == 0)
        { InitGrid(ent); comp.ApplyPrefab = true; }

        if (comp.ApplyPrefab)
        {
            var prefab = SelectPrefab(comp.Prefab);
            for (var x = 0; x < _gridWidth; x++)
            {
                for (var y = 0; y < _gridHeight; y++)
                {
                    comp.ComponentGrid[x, y] = prefab[x, y] != null ? new ReactorPartComponent(prefab[x, y]!) : null;
                }
            }

            comp.ApplyPrefab = false;
            UpdateGridVisual(uid, comp);
        }

        if (!_nodeContainer.TryGetNodes(uid, comp.InletName, comp.OutletName, out OffsetPipeNode? inlet, out OffsetPipeNode? outlet))
            return;

        // Try to connect to a distant pipe
        // TODO: This is BAD and I HATE IT... and I'm too lazy to fix it
        if (inlet.ReachableNodes.Count == 0)
            _nodeGroupSystem.QueueReflood(inlet);
        if (outlet.ReachableNodes.Count == 0)
            _nodeGroupSystem.QueueReflood(outlet);

        _appearance.SetData(uid, ReactorVisuals.Input, inlet.Air.Moles.Sum() > 20);
        _appearance.SetData(uid, ReactorVisuals.Output, outlet.Air.Moles.Sum() > 20);

        var AirContents = new GasMixture();

        var TempRads = 0;
        var ControlRods = 0;
        var AvgControlRodInsertion = 0f;
        var TempChange = 0f;

        // Debug Vars
        var NeutronCount = 0;
        var MeltedComps = 0;
        var TotalNRads = 0f;
        var TotalRads = 0f;
        var TotalSpent = 0f;

        var GasInput = inlet.Air.RemoveVolume(inlet.Air.Volume);

        AirContents.Volume = inlet.Air.Volume;
        GasInput.Volume = AirContents.Volume;

        // Even though it's probably bad for performace, we have to do the for x, for y loops 3 times
        // to ensure the processes do not interfere with each other

        // Rod interactions
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                if (comp.ComponentGrid![x, y] != null)
                {
                    var ReactorComp = comp.ComponentGrid[x, y];
                    var gas = _partSystem.ProcessGas(ReactorComp!, ent, args, GasInput);
                    GasInput.Volume -= ReactorComp!.GasVolume;

                    if (gas != null)
                        _atmosphereSystem.Merge(AirContents, gas);

                    _partSystem.ProcessHeat(ReactorComp, ent, GetGridNeighbors(comp, x, y), this);
                    comp.TemperatureGrid[x, y] = ReactorComp.Temperature;

                    if (ReactorComp.RodType == (byte)ReactorPartComponent.RodTypes.Control && ReactorComp.IsControlRod)
                    {
                        AvgControlRodInsertion += ReactorComp.NeutronCrossSection;
                        ReactorComp.ConfiguredInsertionLevel = comp.ControlRodInsertion;
                        ControlRods++;
                    }

                    if (ReactorComp.Melted)
                        MeltedComps++;

                    comp.FluxGrid[x, y] = _partSystem.ProcessNeutrons(ReactorComp, comp.FluxGrid[x, y], uid, out var deltaT);
                    TempChange += deltaT;

                    TotalNRads += ReactorComp.NRadioactive;
                    TotalRads += ReactorComp.Radioactive;
                    TotalSpent += ReactorComp.SpentFuel;
                }
                else
                    comp.TemperatureGrid[x, y] = 0;
            }
        }

        // Snapshot of the flux grid that won't get messed up by the neutron calculations
        var flux = new List<ReactorNeutron>[_gridWidth, _gridHeight];
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                flux[x, y] = new List<ReactorNeutron>(comp.FluxGrid[x, y]);
                comp.NeutronGrid[x, y] = comp.FluxGrid[x, y].Count;
            }
        }

        // Move neutrons
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                foreach (var neutron in flux[x, y])
                {
                    NeutronCount++;

                    var dir = neutron.dir.AsFlag();
                    // Bit abuse
                    var xmod = (((byte)dir >> 1) % 2) - (((byte)dir >> 3) % 2);
                    var ymod = (((byte)dir >> 2) % 2) - ((byte)dir % 2);

                    if (x + xmod >= 0 && y + ymod >= 0 && x + xmod <= _gridWidth - 1
                        && y + ymod <= _gridHeight - 1)
                    {
                        comp.FluxGrid[x + xmod, y + ymod].Add(neutron);
                        comp.FluxGrid[x, y].Remove(neutron);
                    }
                    else
                    {
                        comp.FluxGrid[x, y].Remove(neutron);
                        TempRads++; // neutrons hitting the casing get blasted in to the room - have fun with that engineers!
                    }
                }
            }
        }

        var CasingGas = ProcessCasingGas(comp, args, GasInput);
        if (CasingGas != null)
            _atmosphereSystem.Merge(AirContents, CasingGas);

        // If there's still input gas left over
        _atmosphereSystem.Merge(AirContents, GasInput);

        UpdateTempIndicators(ent);

        comp.RadiationLevel = Math.Clamp(comp.RadiationLevel + TempRads, 0, 50);

        comp.NeutronCount = NeutronCount;
        comp.MeltedParts = MeltedComps;
        comp.DetectedControlRods = ControlRods;
        comp.AvgInsertion = AvgControlRodInsertion / ControlRods;
        comp.TotalNRads = TotalNRads;
        comp.TotalRads = TotalRads;
        comp.TotalSpent = TotalSpent;

        // Averaging my averages
        for (var i = 1; i < comp.ThermalPowerL1.Length; i++)
        {
            comp.ThermalPowerL1[i - 1] = comp.ThermalPowerL1[i];
        }
        comp.ThermalPowerL1[^1] = TempChange;
        for (var i = 1; i < comp.ThermalPowerL2.Length; i++)
        {
            comp.ThermalPowerL2[i - 1] = comp.ThermalPowerL2[i];
        }
        comp.ThermalPowerL2[^1] = comp.ThermalPowerL1.Average();
        comp.ThermalPower = comp.ThermalPowerL2.Average();

        if (comp.Temperature > comp.ReactorMeltdownTemp) // Disabled the explode if over 1000 rads thing, hope the server survives
        {
            CatastrophicOverload(ent);
        }

        _atmosphereSystem.Merge(outlet.Air, AirContents);

        UpdateVisuals(ent);
        UpdateAudio(ent);
    }

    private void ProcessCaseRadiation(Entity<NuclearReactorComponent> ent)
    {
        var comp = EnsureComp<RadiationSourceComponent>(ent.Owner);

        comp.Intensity = Math.Max(ent.Comp.RadiationLevel, ent.Comp.Melted ? 10 : 0);
        ent.Comp.RadiationLevel /= 2;
    }

    private void InitGrid(Entity<NuclearReactorComponent> ent)
    {
        var xspace = 18f / 32f;
        var yspace = 15f / 32f;

        var yoff = 5f / 32f;

        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                // ...48 entities stuck on the grid, spawn one more, pass it around, 49 entities stuck on the grid...
                ent.Comp.VisualGrid[x, y] = _entityManager.GetNetEntity(SpawnAttachedTo("ReactorComponent", new(ent.Owner, xspace * (y - 3), (-yspace * (x - 3)) - yoff)));
            }
        }
    }

    private static ReactorPartComponent?[,] SelectPrefab(string select) => select switch
    {
        "normal" => NuclearReactorPrefabs.Normal,
        "debug" => NuclearReactorPrefabs.Debug,
        "meltdown" => NuclearReactorPrefabs.Meltdown,
        "alignment" => NuclearReactorPrefabs.Alignment,
        _ => NuclearReactorPrefabs.Empty,
    };

    private static List<ReactorPartComponent?> GetGridNeighbors(NuclearReactorComponent reactor, int x, int y)
    {
        var neighbors = new List<ReactorPartComponent?>();
        if (x - 1 < 0)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x - 1, y]);
        if (x + 1 >= _gridWidth)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x + 1, y]);
        if (y - 1 < 0)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x, y - 1]);
        if (y + 1 >= _gridHeight)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x, y + 1]);
        return neighbors;
    }

    private GasMixture? ProcessCasingGas(NuclearReactorComponent reactor, AtmosDeviceUpdateEvent args, GasMixture inGas)
    {
        GasMixture? ProcessedGas = null;
        if (reactor.AirContents != null)
        {
            var DeltaT = reactor.Temperature - reactor.AirContents.Temperature;
            var DeltaTr = Math.Pow(reactor.Temperature, 4) - Math.Pow(reactor.AirContents.Temperature, 4);

            var k = (Math.Pow(10, 6 / 5) - 1) / 2;
            var A = 1 * (0.4 * 8);

            var ThermalEnergy = _atmosphereSystem.GetThermalEnergy(reactor.AirContents);

            var Hottest = Math.Max(reactor.AirContents.Temperature, reactor.Temperature);
            var Coldest = Math.Min(reactor.AirContents.Temperature, reactor.Temperature);

            var MaxDeltaE = Math.Clamp((k * A * DeltaT) + (5.67037442e-8 * A * DeltaTr),
                (reactor.Temperature * reactor.ThermalMass) - (Hottest * reactor.ThermalMass),
                (reactor.Temperature * reactor.ThermalMass) - (Coldest * reactor.ThermalMass));

            reactor.AirContents.Temperature = (float)Math.Clamp(reactor.AirContents.Temperature +
                (MaxDeltaE / _atmosphereSystem.GetHeatCapacity(reactor.AirContents, true)), Coldest, Hottest);

            reactor.Temperature = (float)Math.Clamp(reactor.Temperature -
                ((_atmosphereSystem.GetThermalEnergy(reactor.AirContents) - ThermalEnergy) / reactor.ThermalMass), Coldest, Hottest);

            if (reactor.AirContents.Temperature < 0 || reactor.Temperature < 0)
                throw new Exception("Reactor casing temperature calculation resulted in sub-zero value.");

            ProcessedGas = reactor.AirContents;
        }

        if (inGas != null && _atmosphereSystem.GetThermalEnergy(inGas) > 0)
        {
            reactor.AirContents = inGas.RemoveVolume(Math.Min(reactor.ReactorVesselGasVolume * _atmosphereSystem.PumpSpeedup() * args.dt, inGas.Volume));

            if (reactor.AirContents != null && reactor.AirContents.TotalMoles < 1)
            {
                if (ProcessedGas != null)
                {
                    _atmosphereSystem.Merge(ProcessedGas, reactor.AirContents);
                    reactor.AirContents.Clear();
                }
                else
                {
                    ProcessedGas = reactor.AirContents;
                    reactor.AirContents.Clear();
                }
            }
        }
        return ProcessedGas;
    }

    private void CatastrophicOverload(Entity<NuclearReactorComponent> ent)
    {
        var comp = ent.Comp;
        var uid = ent.Owner;

        var stationUid = _station.GetStationInMap(Transform(uid).MapID);
        if (stationUid != null)
            _alertLevel.SetLevel(stationUid.Value, comp.MeltdownAlertLevel, true, true, true);

        var announcement = Loc.GetString("reactor-meltdown-announcement");
        var sender = Loc.GetString("reactor-meltdown-announcement-sender");
        _chatSystem.DispatchStationAnnouncement(stationUid ?? uid, announcement, sender, false, null, Color.Orange);

        _soundSystem.PlayGlobalOnStation(uid, _audio.ResolveSound(comp.MeltdownSound));

        comp.Melted = true;
        var MeltdownBadness = 0f;
        comp.AirContents ??= new();

        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                if (comp.ComponentGrid[x, y] != null)
                {
                    var RC = comp.ComponentGrid[x, y];
                    if (RC == null)
                        return;
                    MeltdownBadness += ((RC.Radioactive * 2) + (RC.NRadioactive * 5) + (RC.SpentFuel * 10)) * (RC.Melted ? 2 : 1);
                    if (RC.RodType == (byte)ReactorPartComponent.RodTypes.GasChannel)
                        _atmosphereSystem.Merge(comp.AirContents, RC.AirContents ?? new());
                }
            }
        }
        comp.RadiationLevel = Math.Clamp(comp.RadiationLevel + MeltdownBadness, 0, 200);
        comp.AirContents.AdjustMoles(Gas.Tritium, MeltdownBadness * 15);
        comp.AirContents.Temperature = Math.Max(comp.Temperature, comp.AirContents.Temperature);

        var T = _atmosphereSystem.GetTileMixture(ent.Owner, excite: true);
        if (T != null)
            _atmosphereSystem.Merge(T, comp.AirContents);

        _adminLog.Add(LogType.Explosion, LogImpact.High, $"{ToPrettyString(ent):reactor} catastrophically overloads, meltdown badness: {MeltdownBadness}");

        // You did not see graphite on the roof. You're in shock. Report to medical.
        for (var i = 0; i < _random.Next(10, 30); i++)
            SpawnAtPosition("NuclearDebrisChunk", new(uid, _random.NextVector2(4)));

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/metal_break5.ogg"), uid);
        _explosionSystem.QueueExplosion(ent.Owner, "Default", Math.Max(100, MeltdownBadness * 5), 1, 4, 0, canCreateVacuum: false);

        // Reset grids
        Array.Clear(comp.ComponentGrid);
        Array.Clear(comp.NeutronGrid);
        Array.Clear(comp.TemperatureGrid);
        Array.Clear(comp.FluxGrid);

        UpdateGridVisual(ent.Owner, comp);
    }

    private void UpdateVisuals(Entity<NuclearReactorComponent> ent)
    {
        var comp = ent.Comp;
        var uid = ent.Owner;

        if (comp.Melted)
        {
            _appearance.SetData(uid, ReactorVisuals.Lights, ReactorWarningLights.LightsOff);
            _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Off);
            return;
        }

        // Temperature & radiation warning
        if (comp.Temperature >= comp.ReactorOverheatTemp || comp.RadiationLevel > 15)
            if (comp.Temperature >= comp.ReactorFireTemp || comp.RadiationLevel > 30)
                _appearance.SetData(uid, ReactorVisuals.Lights, ReactorWarningLights.LightsMeltdown);
            else
                _appearance.SetData(uid, ReactorVisuals.Lights, ReactorWarningLights.LightsWarning);
        else
            _appearance.SetData(uid, ReactorVisuals.Lights, ReactorWarningLights.LightsOff);

        // Status screen / side lights
        switch (comp.Temperature)
        {
            case float n when n is <= Atmospherics.T20C:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Off);
                break;
            case float n when n > Atmospherics.T20C && n <= comp.ReactorOverheatTemp:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Active);
                break;
            case float n when n > comp.ReactorOverheatTemp && n <= comp.ReactorFireTemp:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Overheat);
                break;
            case float n when n > comp.ReactorFireTemp && n <= comp.ReactorMeltdownTemp:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Meltdown);
                break;
            case float n when n > comp.ReactorMeltdownTemp && n <= float.PositiveInfinity:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Boom);
                break;
            default:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Off);
                break;
        }
    }

    private void UpdateAudio(Entity<NuclearReactorComponent> ent)
    {
        var comp = ent.Comp;
        var uid = ent.Owner;

        // Stop Alarms after meltdown
        if(comp.Melted)
        {
            if (_audio.IsPlaying(comp.AlarmAudioHighThermal))
                comp.AlarmAudioHighThermal = _audio.Stop(comp.AlarmAudioHighThermal);
            if (_audio.IsPlaying(comp.AlarmAudioHighTemp))
                comp.AlarmAudioHighTemp = _audio.Stop(comp.AlarmAudioHighTemp);
            if (_audio.IsPlaying(comp.AlarmAudioHighRads))
                comp.AlarmAudioHighRads = _audio.Stop(comp.AlarmAudioHighRads);
            return;
        }

        if (comp.ThermalPower > 10000000)
        {
            if (!_audio.IsPlaying(comp.AlarmAudioHighThermal))
                comp.AlarmAudioHighThermal = _audio.PlayPvs(new SoundPathSpecifier("/Audio/_FarHorizons/Machines/reactor_alarm_1.ogg"), uid, AudioParams.Default.WithLoop(true).WithVolume(-3))?.Entity;
        }
        else
            if (_audio.IsPlaying(comp.AlarmAudioHighThermal))
            comp.AlarmAudioHighThermal = _audio.Stop(comp.AlarmAudioHighThermal);

        if (comp.Temperature > comp.ReactorOverheatTemp)
        {
            if (!_audio.IsPlaying(comp.AlarmAudioHighTemp))
                comp.AlarmAudioHighTemp = _audio.PlayPvs(new SoundPathSpecifier("/Audio/_FarHorizons/Machines/reactor_alarm_2.ogg"), uid, AudioParams.Default.WithLoop(true).WithVolume(-3))?.Entity;
        }
        else
            if (_audio.IsPlaying(comp.AlarmAudioHighTemp))
            comp.AlarmAudioHighTemp = _audio.Stop(comp.AlarmAudioHighTemp);

        if (comp.RadiationLevel > 15)
        {
            if (!_audio.IsPlaying(comp.AlarmAudioHighRads))
                comp.AlarmAudioHighRads = _audio.PlayPvs(new SoundPathSpecifier("/Audio/_FarHorizons/Machines/reactor_alarm_3.ogg"), uid, AudioParams.Default.WithLoop(true).WithVolume(-3))?.Entity;
        }
        else
            if (_audio.IsPlaying(comp.AlarmAudioHighRads))
            comp.AlarmAudioHighRads = _audio.Stop(comp.AlarmAudioHighRads);
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
        var query = EntityQueryEnumerator<NuclearReactorComponent>();

        while (query.MoveNext(out var uid, out var reactor))
        {
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
                var pos = (x * _gridWidth) + y;
                temp[pos] = reactor.TemperatureGrid[x, y];
                neutron[pos] = reactor.NeutronGrid[x, y];
                icon[pos] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.IconStateInserted : "base";

                partName[pos] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.Name : "empty";
                partInfo[pos] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.NRadioactive : 0;
                partInfo[pos + zoff] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.Radioactive : 0;
                partInfo[pos + (zoff * 2)] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.SpentFuel : 0;
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

    private void OnItemActionMessage(Entity<NuclearReactorComponent> ent, ref ReactorItemActionMessage args)
    {
        var comp = ent.Comp;
        var pos = args.Position;
        var part = comp.ComponentGrid[(int)pos.X, (int)pos.Y];

        if (comp.PartSlot.Item == null == (part == null))
            return;

        if (comp.PartSlot.Item == null)
        {
            if (part!.Melted) // No removing a part if it's melted
            {
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg"), ent.Owner);
                return;
            }

            var item = SpawnInContainerOrDrop("BaseReactorItem", ent.Owner, "part_slot");
            _metaDataSystem.SetEntityName(item, part!.Name);
            _entityManager.AddComponent(item, new ReactorPartComponent(part!));

            _adminLog.Add(LogType.Action, $"{ToPrettyString(args.Actor):actor} removed {part!.Name} from position {args.Position} from {ToPrettyString(ent):target}");
            comp.ComponentGrid[(int)pos.X, (int)pos.Y] = null;
        }
        else
        {
            if (TryComp(comp.PartSlot.Item, out ReactorPartComponent? reactorPart))
                comp.ComponentGrid[(int)pos.X, (int)pos.Y] = new ReactorPartComponent(reactorPart);
            else
                return;

            _adminLog.Add(LogType.Action, $"{ToPrettyString(args.Actor):actor} added {ToPrettyString(comp.PartSlot.Item):item} to position {args.Position} in {ToPrettyString(ent):target}");
            comp.ComponentGrid[(int)pos.X, (int)pos.Y]!.Name = Identity.Name(comp.PartSlot.Item.Value, _entityManager);
            _entityManager.DeleteEntity(comp.PartSlot.Item);
        }

        UpdateGridVisual(ent.Owner, comp);
        UpdateUI(ent.Owner, comp);
    }

    private void OnControlRodMessage(Entity<NuclearReactorComponent> ent, ref ReactorControlRodModifyMessage args)
    {
        ent.Comp.ControlRodInsertion = Math.Clamp(ent.Comp.ControlRodInsertion + args.Change, 0, 2);
        _adminLog.Add(LogType.Action, $"{ToPrettyString(args.Actor):actor} set control rod insertion of {ToPrettyString(ent):target} to {ent.Comp.ControlRodInsertion}");
        UpdateUI(ent.Owner, ent.Comp);
    }
    #endregion

    protected override void SendEngiRadio(Entity<NuclearReactorComponent> ent, string message)
    {
        _engi ??= _prototypes.Index<RadioChannelPrototype>(ent.Comp.AlertChannel);

        _radioSystem.SendRadioMessage(ent.Owner, message, _engi, ent);
    }
}