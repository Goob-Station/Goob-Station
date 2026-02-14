using Content.Goobstation.Shared.MisandryBox.Thunderdome;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.GameTicking.Rules;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Fluids.Components;
using Content.Shared.Item;
using Content.Server.Preferences.Managers;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Preferences;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.MisandryBox.Thunderdome;

public sealed class ThunderdomeRuleSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;

    private const string RulePrototype = "ThunderdomeRule";
    private EntityUid? _ruleEntity;

    private readonly Dictionary<ICommonSession, ThunderdomeLoadoutEui> _activeEuis = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnding);
        SubscribeLocalEvent<ThunderdomeRuleComponent, RuleLoadedGridsEvent>(OnGridsLoaded);
        SubscribeNetworkEvent<ThunderdomeJoinRequestEvent>(OnJoinRequest);
        SubscribeNetworkEvent<ThunderdomeLeaveRequestEvent>(OnLeaveRequest);
        SubscribeLocalEvent<ThunderdomePlayerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ThunderdomeOriginalBodyComponent, MobStateChangedEvent>(OnOriginalBodyStateChanged);
        SubscribeNetworkEvent<ThunderdomeRevivalAcceptEvent>(OnRevivalAccept);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_ruleEntity != null && TryComp<ThunderdomeRuleComponent>(_ruleEntity.Value, out var rule) && rule.Active)
        {
            var now = _timing.CurTime;
            if (now >= rule.NextCleanup)
            {
                rule.NextCleanup = now + rule.CleanupInterval;
                SweepLooseItems(rule);
            }
        }
    }

    private void OnRoundStarting(RoundStartingEvent ev)
    {
        if (!_cfg.GetCVar(ThunderdomeCVars.ThunderdomeEnabled))
            return;

        if (!_ticker.StartGameRule(RulePrototype, out var ruleEntity))
        {
            Log.Error("Thunderdome: Failed to start game rule.");
            return;
        }

        _ruleEntity = ruleEntity;
        Log.Info("Thunderdome: Game rule started successfully.");
    }

    private void OnRoundEnding(RoundRestartCleanupEvent ev)
    {
        foreach (var eui in _activeEuis.Values)
            eui.Close();
        _activeEuis.Clear();

        if (_ruleEntity == null)
            return;

        if (TryComp<ThunderdomeRuleComponent>(_ruleEntity.Value, out var rule))
        {
            var query = EntityQueryEnumerator<ThunderdomePlayerComponent>();
            while (query.MoveNext(out var uid, out _))
            {
                QueueDel(uid);
            }

            rule.Players.Clear();
            rule.Active = false;
        }

        var bodyQuery = EntityQueryEnumerator<ThunderdomeOriginalBodyComponent>();
        while (bodyQuery.MoveNext(out var uid, out _))
        {
            RemComp<ThunderdomeOriginalBodyComponent>(uid);
        }

        _ruleEntity = null;
    }

    private void OnGridsLoaded(EntityUid uid, ThunderdomeRuleComponent component, ref RuleLoadedGridsEvent args)
    {
        component.ArenaMap = args.Map;
        component.ArenaGrids.AddRange(args.Grids);
        component.Active = true;
        Log.Info($"Thunderdome: Arena loaded on map {args.Map} with {args.Grids.Count} grid(s). Arena is now active.");
        BroadcastPlayerCount(component);
    }

    private void OnJoinRequest(ThunderdomeJoinRequestEvent ev, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;

        if (!_cfg.GetCVar(ThunderdomeCVars.ThunderdomeEnabled)
            || _ruleEntity == null
            || !TryComp<ThunderdomeRuleComponent>(_ruleEntity.Value, out var rule)
            || !rule.Active)
            return;

        if (session.AttachedEntity is not { Valid: true } ghostEntity
            || !HasComp<GhostComponent>(ghostEntity)
            || HasComp<ThunderdomePlayerComponent>(ghostEntity))
            return;

        _activeEuis.Remove(session);

        var eui = new ThunderdomeLoadoutEui(this, _ruleEntity.Value, session);
        _euiManager.OpenEui(eui, session);
        _activeEuis[session] = eui;
    }

    private void OnLeaveRequest(ThunderdomeLeaveRequestEvent ev, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;

        if (session.AttachedEntity is not { Valid: true } entity
            || !TryComp<ThunderdomePlayerComponent>(entity, out var tdPlayer))
            return;

        LeaveThunderdome(entity, tdPlayer, session);
    }

    private void OnMobStateChanged(EntityUid uid, ThunderdomePlayerComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead
            || component.RuleEntity == null
            || !TryComp<ThunderdomeRuleComponent>(component.RuleEntity.Value, out var rule))
            return;

        component.Deaths++;
        component.CurrentStreak = 0;

        if (args.Origin is { } killer && TryComp<ThunderdomePlayerComponent>(killer, out var killerComp))
        {
            killerComp.Kills++;
            killerComp.CurrentStreak++;

            if (_mind.TryGetMind(killer, out _, out var killerMind) && killerMind.UserId is { } killerUserId)
            {
                rule.Kills.TryGetValue(killerUserId, out var existingKills);
                rule.Kills[killerUserId] = existingKills + 1;
                CheckKillStreak(killerComp, rule);
            }
        }

        _mind.TryGetMind(uid, out var mindId, out var deadMind);

        if (deadMind?.UserId is { } deadUserId)
        {
            rule.Deaths.TryGetValue(deadUserId, out var existingDeaths);
            rule.Deaths[deadUserId] = existingDeaths + 1;
        }

        rule.Players.Remove(GetNetEntity(uid));
        var deathCoords = _transform.GetMapCoordinates(uid);
        QueueDel(uid);

        if (mindId != default)
        {
            var ghost = Spawn("MobObserver", deathCoords);
            _mind.TransferTo(mindId, ghost);
        }

        BroadcastPlayerCount(rule);
    }

    public void SpawnPlayer(ICommonSession session, EntityUid ruleEntity, int weaponIdx)
    {
        if (!TryComp<ThunderdomeRuleComponent>(ruleEntity, out var rule)
            || !rule.Active
            || session.AttachedEntity is not { Valid: true } ghostEntity)
            return;

        var spawnCoords = GetRandomSpawnPoint(rule);
        if (spawnCoords == null || !_mind.TryGetMind(ghostEntity, out var mindId, out var mindComp))
            return;

        HumanoidCharacterProfile? profile = null;
        if (mindComp.UserId is { } userId && _prefs.TryGetCachedPreferences(userId, out var prefs))
            profile = prefs.SelectedCharacter as HumanoidCharacterProfile;

        var originalBody = mindComp.OwnedEntity != ghostEntity ? mindComp.OwnedEntity : null;

        var mob = _stationSpawning.SpawnPlayerMob(spawnCoords.Value, null, profile, null);
        _stationSpawning.EquipStartingGear(mob, rule.Gear);
        SpawnLoadoutItems(mob, weaponIdx, rule);

        var tdPlayer = EnsureComp<ThunderdomePlayerComponent>(mob);
        tdPlayer.OriginalBody = originalBody;
        tdPlayer.RuleEntity = ruleEntity;
        tdPlayer.WeaponSelection = weaponIdx;

        if (originalBody is { Valid: true } body && !HasComp<ThunderdomeOriginalBodyComponent>(body))
        {
            var marker = EnsureComp<ThunderdomeOriginalBodyComponent>(body);
            if (mindComp.UserId is { } ownerId)
                marker.Owner = ownerId;
        }

        _mind.TransferTo(mindId, mob);
        rule.Players.Add(GetNetEntity(mob));

        if (ghostEntity.Valid && ghostEntity != mob)
            QueueDel(ghostEntity);

        _activeEuis.Remove(session);

        BroadcastPlayerCount(rule);
    }

    private void LeaveThunderdome(EntityUid entity, ThunderdomePlayerComponent tdPlayer, ICommonSession session)
    {
        if (tdPlayer.RuleEntity == null
            || !TryComp<ThunderdomeRuleComponent>(tdPlayer.RuleEntity.Value, out var rule))
            return;

        rule.Players.Remove(GetNetEntity(entity));
        var coords = _transform.GetMapCoordinates(entity);
        QueueDel(entity);

        if ((session.AttachedEntity == null || !Exists(session.AttachedEntity))
            && _mind.TryGetMind(entity, out var mindId, out _))
        {
            var ghost = Spawn("MobObserver", coords);
            _mind.TransferTo(mindId, ghost);
        }

        BroadcastPlayerCount(rule);
    }

    private void OnOriginalBodyStateChanged(EntityUid uid, ThunderdomeOriginalBodyComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Dead or MobState.Invalid)
            return;

        if (!_playerManager.TryGetSessionById(component.Owner, out var session)
            || session.AttachedEntity is not { Valid: true })
            return;

        RaiseNetworkEvent(new ThunderdomeRevivalOfferEvent(), session.Channel);
    }

    private void OnRevivalAccept(ThunderdomeRevivalAcceptEvent ev, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;

        if (session.AttachedEntity is not { Valid: true } currentEntity)
            return;

        EntityUid? originalBody = null;

        if (TryComp<ThunderdomePlayerComponent>(currentEntity, out var tdPlayer))
            originalBody = tdPlayer.OriginalBody;

        if (originalBody == null || !Exists(originalBody)
            || !TryComp<MobStateComponent>(originalBody, out var mobState)
            || mobState.CurrentState == MobState.Dead)
            return;

        if (!_mind.TryGetMind(currentEntity, out var mindId, out _))
            return;

        if (TryComp<ThunderdomePlayerComponent>(currentEntity, out var tdComp)
            && tdComp.RuleEntity != null
            && TryComp<ThunderdomeRuleComponent>(tdComp.RuleEntity.Value, out var rule))
        {
            rule.Players.Remove(GetNetEntity(currentEntity));
            BroadcastPlayerCount(rule);
            }

        _mind.TransferTo(mindId, originalBody.Value);
        RemComp<ThunderdomeOriginalBodyComponent>(originalBody.Value);
        QueueDel(currentEntity);
    }

    private void SweepLooseItems(ThunderdomeRuleComponent rule)
    {
        var itemQuery = EntityQueryEnumerator<ItemComponent, TransformComponent>();
        while (itemQuery.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.GridUid is not { } grid || !rule.ArenaGrids.Contains(grid))
                continue;

            if (_container.IsEntityInContainer(uid))
                continue;

            QueueDel(uid);
        }

        var puddleQuery = EntityQueryEnumerator<PuddleComponent, TransformComponent>();
        while (puddleQuery.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.GridUid is not { } grid || !rule.ArenaGrids.Contains(grid))
                continue;

            QueueDel(uid);
        }
    }

    private void SpawnLoadoutItems(EntityUid mob, int weaponIdx, ThunderdomeRuleComponent rule)
    {
        if (rule.WeaponLoadouts.Count == 0)
            return;

        weaponIdx = Math.Clamp(weaponIdx, 0, rule.WeaponLoadouts.Count - 1);
        _stationSpawning.EquipStartingGear(mob, rule.WeaponLoadouts[weaponIdx].Gear);
    }

    private EntityCoordinates? GetRandomSpawnPoint(ThunderdomeRuleComponent rule)
    {
        if (rule.ArenaMap == null)
            return null;

        var spawns = new List<EntityCoordinates>();
        var query = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var spawn, out var xform))
        {
            if (spawn.SpawnType != SpawnPointType.LateJoin)
                continue;

            if (xform.GridUid is not { } grid || !rule.ArenaGrids.Contains(grid))
                continue;

            spawns.Add(xform.Coordinates);
        }

        if (spawns.Count == 0)
        {
            var fallbackQuery = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
            while (fallbackQuery.MoveNext(out _, out _, out var xform))
            {
                if (xform.GridUid is not { } grid || !rule.ArenaGrids.Contains(grid))
                    continue;

                spawns.Add(xform.Coordinates);
            }
        }

        return spawns.Count > 0 ? _random.Pick(spawns) : null;
    }

    private void CheckKillStreak(ThunderdomePlayerComponent player, ThunderdomeRuleComponent rule)
    {
        var streak = player.CurrentStreak;
        string? message = streak switch
        {
            3 => Loc.GetString("thunderdome-streak-3"),
            5 => Loc.GetString("thunderdome-streak-5"),
            7 => Loc.GetString("thunderdome-streak-7"),
            10 => Loc.GetString("thunderdome-streak-10"),
            _ => null
        };

        if (message == null)
            return;

        var ev = new ThunderdomeAnnouncementEvent(message);
        foreach (var netEntity in rule.Players)
        {
            if (!TryGetEntity(netEntity, out var playerEntity))
                continue;

            RaiseNetworkEvent(ev, playerEntity.Value);
        }
    }

    private void BroadcastPlayerCount(ThunderdomeRuleComponent rule)
    {
        var ev = new ThunderdomePlayerCountEvent(rule.Players.Count);
        foreach (var session in _playerManager.Sessions)
        {
            if (session.AttachedEntity is { Valid: true })
                RaiseNetworkEvent(ev, session.Channel);
        }
    }

    public ThunderdomeLoadoutEuiState GetLoadoutState(ThunderdomeRuleComponent rule)
    {
        var weapons = new List<ThunderdomeLoadoutOption>();
        for (var i = 0; i < rule.WeaponLoadouts.Count; i++)
        {
            var loadout = rule.WeaponLoadouts[i];
            weapons.Add(new ThunderdomeLoadoutOption
            {
                Index = i,
                Name = Loc.GetString(loadout.Name),
                Description = string.IsNullOrEmpty(loadout.Description) ? string.Empty : Loc.GetString(loadout.Description),
                Category = Loc.GetString(loadout.Category),
                SpritePrototype = loadout.Sprite,
            });
        }

        return new ThunderdomeLoadoutEuiState(weapons, rule.Players.Count);
    }
}
