using Content.Server._CorvaxGoob.Deathmatch_CS.Components;
using Content.Server.Administration.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Maps;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._CorvaxGoob.Deathmatch_CS.Systems;

public sealed class CSRuleSystem : GameRuleSystem<CSRuleComponent>
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly IGameMapManager _gameMapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    private List<Session> _sessions = new();
    public List<Session> Sessions => _sessions;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GameRunLevelChangedEvent>(MapClearing);
        SubscribeLocalEvent<GameRuleStartedEvent>(RoundStart);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(RoundEnd);

        SubscribeLocalEvent<FighterComponent, MobStateChangedEvent>(OnKillReported);
        SubscribeLocalEvent<FighterComponent, PlayerDetachedEvent>(PlayerHasDisconnected);
        SubscribeLocalEvent<FighterComponent, EraseEvent>(EraseАPlayer);
        SubscribeLocalEvent<FighterComponent, EntityTerminatingEvent>(DeleteАPlayer);
    }

    public void CreateNewSession()
    {
        var query = EntityQueryEnumerator<CSRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uId, out var csRuleC, out var gRuleC))
        {
            if (!GameTicker.IsGameRuleActive(uId, gRuleC))
                return;

            if (0 > csRuleC.NumberOfSessions || csRuleC.NumberOfSessions > 20)
                csRuleC.NumberOfSessions = 2; //No.

            if ((_sessions?.Count ?? 0) < csRuleC.NumberOfSessions)
            {
                Session newSession = new();
                GameMapPrototype? protoMap;

                if (!csRuleC.RandomArena)
                    protoMap = _gameMapManager.GetSelectedMap();
                else
                {
                    var maps = _gameMapManager.CurrentlyEligibleMaps().ToList();
                    protoMap = _random.Pick(maps);
                }

                AddMap(out newSession.MapId, protoMap);
                _sessions?.Add(newSession);
            }
        }
    }
    private void AddMap(out MapId mapId, GameMapPrototype? mapProto)
    {
        if (mapProto == null)
        {
            _gameMapManager.SelectMapByConfigRules();
            mapProto = _gameMapManager.GetSelectedMap();
        }
        GameTicker.LoadGameMap(mapProto!, out MapId mapIdproxy);
        mapId = mapIdproxy;
        _map.InitializeMap(mapId);
    }

    private void MapClearing(GameRunLevelChangedEvent ev)
    {
        var query = EntityQueryEnumerator<CSRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uId, out _, out var gRuleC))
        {
            if (!GameTicker.IsGameRuleActive(uId, gRuleC))
                return;

            var activeMapIds = new HashSet<MapId>(_sessions.Select(s => s.MapId));
            foreach (var mapId in _map.GetAllMapIds())
            {
                if (_map.MapExists(mapId) && !activeMapIds.Contains(mapId))
                {
                    _map.DeleteMap(mapId);
                }
            }
        }
    }

    private void OnKillReported(EntityUid uid, FighterComponent isFighterComp, MobStateChangedEvent args)
    {
        if (_sessions == null)
            return;
        if (MobState.Dead != args.NewMobState)
            return;

        RemovingFromSession(uid, isFighterComp);
    }

    private void RemovingFromSession(EntityUid uid, FighterComponent isFighterComp)
    {
        foreach (var session in _sessions)
        {
            if (!session.Players.Contains(uid))
                continue;

            session.Players.Remove(uid);

            foreach (var playerUid in session.Players)
            {
                if (EntityManager.TryGetComponent(playerUid, out FighterComponent? teammatesComp))
                    if (teammatesComp.Command == isFighterComp.Command)
                        return;
            }

            var query2 = EntityQueryEnumerator<FighterComponent, GhostRoleComponent, TransformComponent, MindContainerComponent>();

            // остались ли незанятые роли
            while (query2.MoveNext(out var guid, out var timmFighterComp, out _, out var xform, out var mindContC))
            {
                if (xform.MapID == session.MapId)
                    if (mindContC.Mind == null &&
                        _mobStateSystem.IsAlive(guid) &&
                        timmFighterComp.Command == isFighterComp.Command)
                        return;
            }

            _sessions.Remove(session);
            CreateNewSession();

            if (_map.TryGetMap(session.MapId, out var uid2))
                QueueDel(uid2);

            return;
        }
    }

    private void RoundStart(ref GameRuleStartedEvent _)
    {
        CreateNewSession();
    }

    private void RoundEnd(RoundRestartCleanupEvent _)
    {
        _sessions.Clear();
    }

    private void PlayerHasDisconnected(EntityUid uid, FighterComponent isFighterComp, PlayerDetachedEvent args)
    {
        RemovingFromSession(uid, isFighterComp);
    }

    private void EraseАPlayer(EntityUid uid, FighterComponent isFighterComp, EraseEvent args)
    {
        RemovingFromSession(uid, isFighterComp);
    }

    private void DeleteАPlayer(EntityUid uid, FighterComponent isFighterComp, EntityTerminatingEvent args)
    {
        RemovingFromSession(uid, isFighterComp);
    }
}

public sealed class Session
{
    public MapId MapId;
    public List<EntityUid> Players = new();
}
