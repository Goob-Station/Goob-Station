using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Shared._Lavaland.Shuttles;
using Content.Shared._Lavaland.Shuttles.Components;
using Content.Shared._Lavaland.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Timing;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server._Lavaland.Shuttles.Systems;

public sealed class DockingConsoleSystem : SharedDockingConsoleSystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DockingConsoleComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<DockEvent>(OnDock);
        SubscribeLocalEvent<UndockEvent>(OnUndock);
        SubscribeLocalEvent<FTLCompletedEvent>(OnFTLCompleted);

        Subs.BuiEvents<DockingConsoleComponent>(DockingConsoleUiKey.Key,
            subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
            subs.Event<DockingConsoleFTLMessage>(OnFTL);
            subs.Event<DockingConsoleShuttleCheckMessage>(OnCallShuttle);
        });
    }

    private void OnMapInit(Entity<DockingConsoleComponent> ent, ref MapInitEvent args)
    {
        UpdateShuttle(ent);
        UpdateUI(ent);
        Dirty(ent);
    }

    private void OnDock(DockEvent args)
    {
        UpdateConsoles(args.GridAUid, args.GridBUid);
    }

    private void OnFTLCompleted(ref FTLCompletedEvent args)
    {
        // Update the state after the cooldown. Shitcode because
        // no events are raised on FTL cooldown completion
        var ent = args.Entity;
        if (!TryComp<FTLComponent>(ent, out var ftl))
            return; // how?

        Timer.Spawn(ftl.StateTime.Length + TimeSpan.FromSeconds(1), () => UpdateConsolesUsing(ent));
        Dirty(ent, ftl);
    }

    private void OnUndock(UndockEvent args)
    {
        UpdateConsoles(args.GridAUid, args.GridBUid);
    }

    private void OnOpened(Entity<DockingConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (TerminatingOrDeleted(ent.Comp.Shuttle))
            UpdateShuttle(ent);

        UpdateUI(ent);
    }

    private void UpdateConsoles(EntityUid gridA, EntityUid gridB)
    {
        UpdateConsolesUsing(gridA);
        UpdateConsolesUsing(gridB);
    }

    /// <summary>
    /// Update the UI of every console that is using a certain shuttle.
    /// </summary>
    public void UpdateConsolesUsing(EntityUid shuttle)
    {
        if (!HasComp<DockingShuttleComponent>(shuttle))
            return;

        var query = EntityQueryEnumerator<DockingConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Shuttle == shuttle)
                UpdateUI((uid, comp));
        }
    }

    public void UpdateUI(Entity<DockingConsoleComponent> ent)
    {
        if (ent.Comp.Shuttle is not {} shuttle)
            return;

        var ftlState = FTLState.Available;
        StartEndTime ftlTime = default;
        List<DockingDestination> destinations = new();

        if (TryComp<FTLComponent>(shuttle, out var ftl))
        {
            ftlState = ftl.State;
            ftlTime = _shuttle.GetStateTime(ftl);
        }

        if (TryComp<DockingShuttleComponent>(shuttle, out var docking))
        {
            destinations = docking.Destinations;
        }

        var state = new DockingConsoleState(ftlState, ftlTime, destinations);
        _ui.SetUiState(ent.Owner, DockingConsoleUiKey.Key, state);
    }

    private void OnFTL(Entity<DockingConsoleComponent> ent, ref DockingConsoleFTLMessage args)
    {
        if (ent.Comp.Shuttle is not {} shuttle || !TryComp<DockingShuttleComponent>(shuttle, out var docking))
            return;

        if (args.Index < 0 || args.Index > docking.Destinations.Count)
            return;

        var dest = docking.Destinations[args.Index];
        var map = dest.Map;

        // can't FTL if its already there
        if (map == Transform(shuttle).MapID)
            return;

        if (FindLargestGrid(map) is not {} grid)
            return;

        Log.Debug($"{ToPrettyString(args.Actor):user} is FTL-docking {ToPrettyString(shuttle):shuttle} to {ToPrettyString(grid):grid}");

        _shuttle.FTLToDock(shuttle, Comp<ShuttleComponent>(shuttle), grid, priorityTag: docking.DockTag);
    }

    private const string MiningShuttlePath = "/Maps/_Lavaland/mining.yml";

    /// <summary>
    /// Load a new mining shuttle if it still doesn't exist
    /// </summary>
    private void OnCallShuttle(Entity<DockingConsoleComponent> ent, ref DockingConsoleShuttleCheckMessage args)
    {
        if (ent.Comp.Shuttle != null || UpdateShuttle(ent) || HasComp<DockingShuttleComponent>(ent.Comp.Shuttle))
            return;

        _mapSystem.CreateMap(out var dummyMap);
        _mapLoader.TryLoad(dummyMap, MiningShuttlePath, out _);

        // Find the target
        var targetMap = Transform(ent).MapID;
        if (FindLargestGrid(targetMap) is not {} grid)
            return;

        // Find the mining shuttle
        var shuttle = _mapMan.GetAllGrids(dummyMap).FirstOrNull(x => HasComp<DockingShuttleComponent>(x));
        if (!TryComp<DockingShuttleComponent>(shuttle, out var docking))
        {
            Log.Error("Failed to call Mining shuttle since it failed to load.");
            return;
        }

        // Add the station of the calling console
        var shuttleUid = ent.Comp.Shuttle;
        if (!TryComp<DockingShuttleComponent>(shuttleUid, out var shuttleComp))
            return;
        if (shuttleComp.Station == null)
        {
            var targetUid = Transform(ent).MapUid;

            if (targetUid == null)
                return;

            RaiseLocalEvent(shuttleUid.Value, new ShuttleAddStationEvent(targetUid.Value, targetMap), false);
        }

        // Finally FTL
        _shuttle.FTLToDock(shuttle.Value, Comp<ShuttleComponent>(shuttle.Value), grid, priorityTag: docking.DockTag);
        UpdateShuttle(ent);
        UpdateUI(ent);
        Dirty(ent);

        // shitcode because funny
        Timer.Spawn(TimeSpan.FromSeconds(15), () => _mapMan.DeleteMap(dummyMap));
    }

    private EntityUid? FindLargestGrid(MapId map)
    {
        EntityUid? largestGrid = null;
        var largestSize = 0f;

        var query = EntityQueryEnumerator<MapGridComponent, TransformComponent>();
        while (query.MoveNext(out var gridUid, out var grid, out var xform))
        {
            if (xform.MapID != map)
                continue;

            if (HasComp<BecomesStationComponent>(gridUid) ||
                HasComp<LavalandStationComponent>(gridUid))
                return gridUid;

            var size = grid.LocalAABB.Size.LengthSquared();
            if (size < largestSize)
                continue;

            largestSize = size;
            largestGrid = gridUid;
        }

        return largestGrid;
    }

    /// <summary>
    /// Tries to connect to some mining shuttle on init.
    /// Returns true on success.
    /// </summary>
    public bool UpdateShuttle(Entity<DockingConsoleComponent> ent)
    {
        var hadShuttle = ent.Comp.HasShuttle;

        ent.Comp.Shuttle = FindShuttle(ent.Comp.ShuttleWhitelist);
        ent.Comp.HasShuttle = ent.Comp.Shuttle != null;

        if (ent.Comp.HasShuttle != hadShuttle)
            Dirty(ent);

        return ent.Comp.HasShuttle;
    }

    private EntityUid? FindShuttle(EntityWhitelist whitelist)
    {
        var query = EntityQueryEnumerator<DockingShuttleComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (_whitelist.IsValid(whitelist, uid))
                return uid;
        }

        return null;
    }
}
