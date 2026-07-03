using Content.Shared._starcup.Station.Components;
using Content.Server.Chat.Systems;
using Content.Server.Players;
using Content.Server.Respawn;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Collections;
using Robust.Shared.Localization;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Server._starcup.Station.Systems;

public sealed class CharonSupplyDropSchedulerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SpecialRespawnSystem _respawn = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    /// <summary>
    /// Drops will try to land within this many tiles of a randomly chosen player (or the station origin).
    /// </summary>
    private const float DropRadiusTiles = 30f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CharonSupplyDropSchedulerComponent, StationPostInitEvent>(OnStationPostInit);
        SubscribeLocalEvent<GameRuleStartedEvent>(OnGameRuleStarted);
    }

    private void OnStationPostInit(EntityUid uid, CharonSupplyDropSchedulerComponent component, ref StationPostInitEvent args)
    {
        component.NextDrop = _timing.CurTime + component.Interval;
    }

    private void OnGameRuleStarted(ref GameRuleStartedEvent args)
    {
        if (args.RuleId != "CharonSupplyDrop")
            return;

        if (!TryGetDropStation(out var station))
            return;

        TrySpawnDrop(station!.Value, announce: true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CharonSupplyDropSchedulerComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.NextDrop == null || _timing.CurTime < component.NextDrop)
                continue;

            component.NextDrop = _timing.CurTime + component.Interval;
            TrySpawnDrop(uid, announce: true);
        }
    }

    private bool TryGetDropStation(out EntityUid? station)
    {
        // Prefer Charon stations; fall back to the first available station.
        var query = EntityQueryEnumerator<CharonSupplyDropSchedulerComponent, StationDataComponent>();
        while (query.MoveNext(out var uid, out _, out _))
        {
            station = uid;
            return true;
        }

        foreach (var candidate in _station.GetStations())
        {
            station = candidate;
            return true;
        }

        station = null;
        return false;
    }

    private void TrySpawnDrop(EntityUid stationUid, bool announce = false)
    {
        var grid = _station.GetLargestGrid(stationUid);
        if (grid == null)
            return;

        var xform = Transform(grid.Value);
        if (xform.MapUid == null)
            return;

        if (!TryComp<CharonSupplyDropSchedulerComponent>(stationUid, out var scheduler))
            return;

        var spawners = scheduler.DropSpawners;
        if (spawners.Count == 0)
            return;

        var center = GetDropCenter(grid.Value, xform.MapUid.Value);
        if (!TryFindRandomTileNear(grid.Value, xform.MapUid.Value, center, DropRadiusTiles, out var coords))
            return;

        var pod = _random.Pick(spawners);
        Spawn(pod, coords);

        if (announce)
        {
            _chat.DispatchGlobalAnnouncement(
                Loc.GetString("station-event-charon-supply-drop-announcement"),
                colorOverride: Color.FromHex("#18abf5"));
        }
    }

    private Vector2 GetDropCenter(EntityUid grid, EntityUid mapUid)
    {
        var gridXform = Transform(grid);

        // Try to center on a random player currently on the station grid.
        var players = new ValueList<EntityUid>();
        var query = EntityQueryEnumerator<ActorComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.GridUid == grid)
                players.Add(uid);
        }

        if (players.Count > 0)
        {
            var player = _random.Pick(players);
            var playerPos = _transform.GetWorldPosition(Transform(player));
            return playerPos;
        }

        // Fall back to the station origin.
        return _transform.GetWorldPosition(gridXform);
    }

    private bool TryFindRandomTileNear(EntityUid grid, EntityUid mapUid, Vector2 centerWorld, float radiusTiles, out EntityCoordinates coords)
    {
        coords = EntityCoordinates.Invalid;

        // First try to land reasonably close to the chosen center.
        for (var attempt = 0; attempt < 30; attempt++)
        {
            if (!_respawn.TryFindRandomTile(grid, mapUid, 10, out var candidate, false))
                continue;

            var candidateWorld = _transform.ToMapCoordinates(candidate).Position;
            if ((candidateWorld - centerWorld).Length() <= radiusTiles)
            {
                coords = candidate;
                return true;
            }
        }

        // Fallback: return the tile nearest to the center.
        if (!_respawn.TryFindRandomTile(grid, mapUid, 10, out coords, false))
            return false;

        return true;
    }
}
