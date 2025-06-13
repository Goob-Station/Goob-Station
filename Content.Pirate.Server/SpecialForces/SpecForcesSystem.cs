using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Content.Server.Actions;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Ghost.Roles.Components;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Content.Server.Maps;
using Content.Server.RandomMetadata;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Pirate.Server.SpecialForces;

public sealed class SpecialForcesSystem : EntitySystem
{
    // ReSharper disable once MemberCanBePrivate.Global
    [ViewVariables] public List<SpecialForcesHistory> CalledEvents { get; private set; } = new();
    // ReSharper disable once MemberCanBePrivate.Global
    [ViewVariables] public TimeSpan LastUsedTime { get; private set; } = TimeSpan.Zero;

    private readonly TimeSpan _delayUsage = TimeSpan.FromMinutes(2);
    private readonly ReaderWriterLockSlim _callLock = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpecialForceComponent, MapInitEvent>(OnMapInit, after: new[] { typeof(RandomMetadataSystem) });
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);
        SubscribeLocalEvent<SpecialForceComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SpecialForceComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(EntityUid uid, SpecialForceComponent component, ComponentShutdown args)
    {
            _actions.RemoveAction(uid, component.BssKey);
    }

    private void OnStartup(EntityUid uid, SpecialForceComponent component, ComponentStartup args)
    {
        if (component.ActionBssActionName != null)
            _actions.AddAction(uid, ref component.BssKey, component.ActionBssActionName);
    }

    private void OnMapInit(EntityUid uid, SpecialForceComponent component, MapInitEvent args)
    {
        if (component.Components != null)
        {
            foreach (var entry in component.Components.Values)
            {
                var comp = (Component) _serialization.CreateCopy(entry.Component, notNullableOverride: true);
                comp.Owner = uid;
                EntityManager.AddComponent(uid, comp);
            }
        }
    }

    public TimeSpan DelayTime
    {
        get
        {
            var ct = _gameTicker.RoundDuration();
            var lastUsedTime = LastUsedTime + _delayUsage;
            return ct > lastUsedTime ? TimeSpan.Zero : lastUsedTime - ct;
        }
    }

    public bool CallOps(SpecialForcesType ev, string source = "")
    {
        _callLock.EnterWriteLock();
        try
        {
            if (_gameTicker.RunLevel != GameRunLevel.InRound)
            {
                return false;
            }

            var currentTime = _gameTicker.RoundDuration();

#if !DEBUG
            if (LastUsedTime + _delayUsage > currentTime)
            {
                return false;
            }
#endif

            LastUsedTime = currentTime;

            CalledEvents.Add(new SpecialForcesHistory { Event = ev, RoundTime = currentTime, WhoCalled = source });

            var shuttle = SpawnShuttle(ev);
            if (shuttle == null)
            {
                return false;
            }

            SpawnGhostRole(ev, shuttle.Value);

            PlaySound(ev);

            return true;
        }
        finally
        {
            _callLock.ExitWriteLock();
        }
    }

    private EntityUid SpawnEntity(string? protoName, EntityCoordinates coordinates)
    {
        if (protoName == null)
        {
            return EntityUid.Invalid;
        }

        var uid = EntityManager.SpawnEntity(protoName, coordinates);

        if (!TryComp<GhostRoleMobSpawnerComponent>(uid, out var mobSpawnerComponent) ||
            mobSpawnerComponent.Prototype == null ||
            !_prototypes.TryIndex<EntityPrototype>(mobSpawnerComponent.Prototype, out var spawnObj))
        {
            return uid;
        }

        if (spawnObj.TryGetComponent<SpecialForceComponent>(out var tplSpecForceComponent))
        {
            var comp = (Component) _serialization.CreateCopy(tplSpecForceComponent, notNullableOverride: true);
            comp.Owner = uid;
            EntityManager.AddComponent(uid, comp);
        }

        EnsureComp<SpecialForceComponent>(uid);
        if (spawnObj.TryGetComponent<GhostRoleComponent>(out var tplGhostRoleComponent))
        {
            var comp = (Component) _serialization.CreateCopy(tplGhostRoleComponent, notNullableOverride: true);
            comp.Owner = uid;
            EntityManager.AddComponent(uid, comp);
        }

        return uid;
    }

    private void SpawnGhostRole(SpecialForcesType ev, EntityUid shuttle)
    {
        var spawns = new List<EntityCoordinates>();

        foreach (var (_, meta, xform) in EntityManager
                     .EntityQuery<SpawnPointComponent, MetaDataComponent, TransformComponent>(true))
        {
            if (meta.EntityPrototype?.ID != SpawnMarker)
                continue;

            if (xform.ParentUid != shuttle)
                continue;

            spawns.Add(xform.Coordinates);
            break;
        }

        if (spawns.Count == 0)
        {
            spawns.Add(Transform(shuttle).Coordinates);
        }

        // TODO: Cvar
        var countExtra = _playerManager.PlayerCount switch
        {
            >= 60 => 7,
            >= 50 => 6,
            >= 40 => 5,
            >= 30 => 4,
            >= 20 => 3,
            >= 10 => 2,
            _ => 1
        };

        switch (ev)
        {
            case SpecialForcesType.ERT:
                SpawnEntity(ErtLeader, _random.Pick(spawns));
                // SpawnEntity(ErtEngineer, _random.Pick(spawns));

                while (countExtra > 0)
                {
                    if (countExtra-- > 0)
                    {
                        SpawnEntity(ErtSecurity, _random.Pick(spawns));
                    }

                    if (countExtra-- > 0)
                    {
                        SpawnEntity(ErtEngineer, _random.Pick(spawns));
                    }

                    if (countExtra-- > 0)
                    {
                        SpawnEntity(ErtMedical, _random.Pick(spawns));
                    }

                    if (countExtra-- > 0)
                    {
                        SpawnEntity(ErtJanitor, _random.Pick(spawns));
                    }
                }

                break;
            case SpecialForcesType.CBURN:
                // SpawnEntity(CburnLeader, _random.Pick(spawns));
                SpawnEntity(CburnFlamer, _random.Pick(spawns));
                while (countExtra > 0)
                {
                    if (countExtra-- > 0)
                    {
                        SpawnEntity(Cburn, _random.Pick(spawns));
                    }
                }

                break;
            case SpecialForcesType.DeathSquad:
                SpawnEntity(DeadsquadLeader, _random.Pick(spawns));
                while (countExtra > 0)
                {
                    if (countExtra-- > 0)
                    {
                        SpawnEntity(Deadsquad, _random.Pick(spawns));
                    }
                }

                break;
            default:
                return;
        }
    }

    private EntityUid? SpawnShuttle(SpecialForcesType ev)
    {
        var shuttlePath = ev switch
        {
            SpecialForcesType.ERT => EtrShuttlePath,
            SpecialForcesType.CBURN => CburnShuttlePath,
            SpecialForcesType.DeathSquad => DeadsquadShuttlePath,
            _ => EtrShuttlePath
        };

        var mapId = _mapManager.CreateMap();

        if (!_mapLoader.TryLoadGrid(mapId, new ResPath(shuttlePath), out var gridUid))
        {
            _mapManager.DeleteMap(mapId);
            return null;
        }

        return gridUid;
    }

    private void PlaySound(SpecialForcesType ev)
    {
        var stations = _stationSystem.GetStations();
        if (stations.Count == 0)
        {
            return;
        }

        switch (ev)
        {
            case SpecialForcesType.ERT:
                foreach (var station in stations)
                {
                    _chatSystem.DispatchStationAnnouncement(station,
                        Loc.GetString("spec-forces-system-ertcall-annonce"),
                        Loc.GetString("spec-forces-system-ertcall-title"),
                        false, _ertAnnounce
                    );
                }

                break;
            case SpecialForcesType.CBURN:
                foreach (var station in stations)
                {
                    _chatSystem.DispatchStationAnnouncement(station,
                        Loc.GetString("spec-forces-system-CBURN-annonce"),
                        Loc.GetString("spec-forces-system-CBURN-title"),
                        true
                    );
                }

                break;
            default:
                return;
        }
    }

    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        foreach (var calledEvent in CalledEvents)
        {
            ev.AddLine(Loc.GetString("spec-forces-system-" + calledEvent.Event,
                ("time", calledEvent.RoundTime.ToString(@"hh\:mm\:ss")), ("who", calledEvent.WhoCalled)));
        }
    }

    private void OnCleanup(RoundRestartCleanupEvent ev)
    {
        CalledEvents.Clear();
        LastUsedTime = TimeSpan.Zero;

        if (_callLock.IsWriteLockHeld)
        {
            _callLock.ExitWriteLock();
        }
    }

    [ValidatePrototypeId<EntityPrototype>] private const string SpawnMarker = "MarkerSpecialforce";
    private const string EtrShuttlePath = "Maps/Shuttles/dart.yml";
    [ValidatePrototypeId<EntityPrototype>] private const string ErtLeader = "RandomHumanoidSpawnerERTLeaderEVA";
    [ValidatePrototypeId<EntityPrototype>] private const string ErtSecurity = "RandomHumanoidSpawnerERTSecurity";
    [ValidatePrototypeId<EntityPrototype>] private const string ErtEngineer = "RandomHumanoidSpawnerERTEngineer";
    [ValidatePrototypeId<EntityPrototype>] private const string ErtJanitor = "RandomHumanoidSpawnerERTJanitor";
    [ValidatePrototypeId<EntityPrototype>] private const string ErtMedical = "RandomHumanoidSpawnerERTMedical";

    private const string CburnShuttlePath = "Maps/Shuttles/dart.yml";
    [ValidatePrototypeId<EntityPrototype>] private const string CburnLeader = "RandomHumanoidSpawnerCBURNUnit";
    [ValidatePrototypeId<EntityPrototype>] private const string Cburn = "RandomHumanoidSpawnerCBURNUnit";
    [ValidatePrototypeId<EntityPrototype>] private const string CburnFlamer = "RandomHumanoidSpawnerCBURNUnit";

    private const string DeadsquadShuttlePath = "Maps/Shuttles/dart.yml";
    [ValidatePrototypeId<EntityPrototype>] private const string DeadsquadLeader = "RandomHumanoidSpawnerDeathSquad";
    [ValidatePrototypeId<EntityPrototype>] private const string Deadsquad = "RandomHumanoidSpawnerDeathSquad";

    private readonly SoundSpecifier _ertAnnounce = new SoundPathSpecifier("/Audio/Announcements/announce.ogg");

    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
}
