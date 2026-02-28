using System.Linq;
using Content.Goobstation.Shared.MisandryBox.Thunderdome;
using Content.Server.Destructible;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Fluids.Components;
using Content.Shared.Item;
using Content.Server.Preferences.Managers;
using Content.Shared.Destructible;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Robust.Server.Audio;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

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
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;



    private const string RulePrototype = "ThunderdomeRule";
    private EntityUid? _ruleEntity;

    private readonly Dictionary<ICommonSession, ThunderdomeLoadoutEui> _activeEuis = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnding);
        SubscribeLocalEvent<ThunderdomeRuleComponent, RuleLoadedGridsEvent>(OnGridsLoaded);
        SubscribeNetworkEvent<ThunderdomeJoinRequestEvent>(OnJoinRequest);
        SubscribeNetworkEvent<ThunderdomeLeaveRequestEvent>(OnLeaveRequest);
        SubscribeLocalEvent<ThunderdomePlayerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ThunderdomeOriginalBodyComponent, MobStateChangedEvent>(OnOriginalBodyStateChanged);
        SubscribeNetworkEvent<ThunderdomeRevivalAcceptEvent>(OnRevivalAccept);
        SubscribeLocalEvent<ThunderdomePlayerComponent, SuicideGhostEvent>(OnSuicideAttempt);
        //SubscribeLocalEvent<DestructibleComponent, DestructionAttemptEvent>(OnDestructionAttempt); //todo damage cancel for destruction
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

    private void EnsureRule()
    {
        if (_ruleEntity != null)
            return;

        if (!_ticker.StartGameRule(RulePrototype, out var ruleEntity))
            return;

        _ruleEntity = ruleEntity;
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

    // todo this kinda works but still does damage triggers finish this later ig
    /*
    private void OnDestructionAttempt(EntityUid uid,
        DestructibleComponent component,
        ref DestructionAttemptEvent args)
    {
        if (_ruleEntity == null
            || !TryComp<ThunderdomeRuleComponent>(_ruleEntity.Value, out var rule)
            || !rule.Active)
            return;
        var xform = Transform(uid);
        if (xform.GridUid == null
            || !rule.ArenaGrids.Contains(xform.GridUid.Value))
            return;
        args.Cancel();
    }*/

    private void OnGridsLoaded(EntityUid uid, ThunderdomeRuleComponent component, ref RuleLoadedGridsEvent args)
    {
        component.ArenaMap = args.Map;
        component.ArenaGrids.UnionWith(args.Grids);
        component.Active = true;
        BroadcastPlayerCount(component);
    }

    private void OnJoinRequest(ThunderdomeJoinRequestEvent ev, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;

        if (!_cfg.GetCVar(ThunderdomeCVars.ThunderdomeEnabled))
            return;

        EnsureRule();

        if (_ruleEntity == null
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

        GhostDomePlayer(uid, component, rule, true);

        QueueDel(uid);

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

        _mind.UnVisit(mindId, mindComp); // we are ghost, go to original
        _mind.Visit(mindId, mob, mindComp); // original now visits new body
        mindComp.PreventGhosting = true; // ghosting is wackier than suicide

        rule.Players.Add(GetNetEntity(mob));

        _activeEuis.Remove(session);

        BroadcastPlayerCount(rule);
    }

    private void CleanUpPlayer(EntityUid uid, ThunderdomePlayerComponent tdPlayer, ThunderdomeRuleComponent rule, bool playSound, SoundPathSpecifier sound)
    {

        if (!TryComp<VisitingMindComponent>(uid, out var visitingMind)
            || visitingMind.MindId == null
            || !_mind.TryGetMind(uid, out var mindId, out var mindComp)
            || !_transform.TryGetMapOrGridCoordinates(uid, out var deathCoords))
            return;

        mindComp.PreventGhosting = false;
        rule.Players.Remove(GetNetEntity(uid));

        if (mindId != default)
        {
            var ghost = Spawn("MobObserver", deathCoords.Value);
            _mind.MoveVisitingEntity(uid, ghost, visitingMind, mindComp); // we do ts so we dont have to unvisit then visit again and reload maps.

            if (TryComp<GhostComponent>(ghost, out var ghostComp))
                _ghost.SetCanReturnToBody((ghost, ghostComp), true);

            _playerManager.TryGetSessionById(mindComp.UserId, out var session);
            _playerManager.SetAttachedEntity(session, ghost);

            // namesetting ugly but i cant use spawnghost due to transfer and i cba to make this a method
            if (!string.IsNullOrWhiteSpace(mindComp.CharacterName))
                _meta.SetEntityName(ghost, FormattedMessage.EscapeText(mindComp.CharacterName)); // Goob Sanitize Text
            else if (mindComp.UserId is { } userId && _playerManager.TryGetSessionById(userId, out session))
                _meta.SetEntityName(ghost, FormattedMessage.EscapeText(session.Name)); // Goob Sanitize Text
        }
        if (playSound) // admin erase sound default.
        {
            var name = Identity.Entity(uid, EntityManager);
            _popup.PopupCoordinates(Loc.GetString("thunderdome-leave-01", ("user", name)),
                deathCoords.Value,
                PopupType.LargeCaution);
            var filter = Filter.Pvs(deathCoords.Value, 1, EntityManager, _playerManager);
            var audioParams = new AudioParams().WithVolume(3);
            _audio.PlayStatic(sound, filter, deathCoords.Value, true, audioParams);
        }

        QueueDel(uid);
        BroadcastPlayerCount(rule);
    }

    // todo: handle ghost command
    private void OnSuicideAttempt(EntityUid uid, ThunderdomePlayerComponent tdPlayer, ref SuicideGhostEvent args)
    {
        if (tdPlayer.RuleEntity == null
            || !TryComp<ThunderdomeRuleComponent>(tdPlayer.RuleEntity.Value, out var rule)
            || args.Victim != uid
            )
            return;

        GhostDomePlayer(uid, tdPlayer, rule);
        args.Handled = true;
    }

    private void GhostDomePlayer(
        EntityUid uid,
        ThunderdomePlayerComponent tdPlayer,
        ThunderdomeRuleComponent rule,
        bool bypassPenalty = false,
        bool playSound = true,
        SoundPathSpecifier? sound = null
        )
    {
        if (!bypassPenalty) // todo does nothing but players suiciding could just instarespawn in thunderdome which is prob bad
        {
            tdPlayer.TimePenalty = +rule.BaseTimePenalty;
            var remaining = tdPlayer.RespawnTimer - _timing.CurTime + TimeSpan.FromSeconds(tdPlayer.TimePenalty);
            tdPlayer.RespawnTimer = remaining;
        }
        sound ??= new SoundPathSpecifier("/Audio/Effects/pop_high.ogg");

        CleanUpPlayer(uid, tdPlayer, rule, playSound, sound);
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
        if (args.NewMobState is MobState.Dead or MobState.Invalid || args.OldMobState == MobState.Alive)
            return;

        if (!_playerManager.TryGetSessionById(component.Owner, out var session)
            || session.AttachedEntity is not { Valid: true })
            return;

        RaiseNetworkEvent(new ThunderdomeRevivalOfferEvent(), session.Channel);
    }

    private void OnRevivalAccept(ThunderdomeRevivalAcceptEvent ev, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;

        if (session.AttachedEntity is not { Valid: true } currentEntity
            || !TryComp<ThunderdomePlayerComponent>(currentEntity, out var tdPlayer))
            return;

        var originalBody = tdPlayer.OriginalBody;

        if (originalBody == null || !Exists(originalBody)
            || !TryComp<MobStateComponent>(originalBody, out var mobState)
            || mobState.CurrentState == MobState.Dead)
            return;

        if (!_mind.TryGetMind(currentEntity, out var mindId, out _))
            return;

        if (tdPlayer.RuleEntity != null
            && TryComp<ThunderdomeRuleComponent>(tdPlayer.RuleEntity.Value, out var rule))
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
        if (rule.ArenaMap is not { } map)
            return;

        var items = new HashSet<Entity<ItemComponent>>();
        _lookup.GetEntitiesOnMap(map, items);

        foreach (var (uid, _) in items)
        {
            if (_container.IsEntityInContainer(uid))
                continue;

            QueueDel(uid);
        }

        var puddles = new HashSet<Entity<PuddleComponent>>();
        _lookup.GetEntitiesOnMap(map, puddles);

        foreach (var (uid, _) in puddles)
        {
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
