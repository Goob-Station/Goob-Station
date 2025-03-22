using System.Numerics;
using System.Threading;
using Content.Server.Afk;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Revolutionary.Components;
using Content.Shared._Goobstation.CCVar;
using Content.Shared._Goobstation.MisandryBox;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server._Goobstation.MisandryBox.BSOEnforcement;

/// <summary>
/// BSO roleplay enforcement.
///
/// This system activates only when a BSO is present (configurable) and there is any other command member who is alive and on the same map (configurable).
/// When he is - start a 2 minute (configurable via cvar, threshold * timespan) timer where a BSO has to be in range of at least 15 (also configurable) tiles from any nearest command member.
/// Grace period at spawn is double the timer. (not configurable)
///
/// Should the timer run its course - automatically start action (configurable).
/// (configurable)
/// </summary>
public sealed partial class BSOAutoFailSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly IConfigurationManager _confMan = default!;
    [Dependency] private readonly IAfkManager _afk = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    private Dictionary<EntityUid, int> _apostles = [];
    private string _apostleJobProto = null!;

    private bool _enabled = default!;
    private bool _grace = default!;
    private int _range = default!;
    private bool _mapRelevance = default!;
    private float _timespan = default!;
    private int _threshold = default!;
    private BSOEnforcementPunishmentEnum _punishment = default!;
    private int _bantime = default!;

    private CancellationTokenSource _cts = default!;


    public override void Initialize()
    {
        base.Initialize();

        _cts = new CancellationTokenSource();
        CvarSubs();

        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(Cleanup);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawn);
    }

    private void CvarSubs()
    {
        Subs.CVar(_confMan, GoobCVars.BSOEnforcement, value => _enabled = value, true);
        Subs.CVar(_confMan, GoobCVars.BSOJobProto, value => _apostleJobProto = value, true);
        Subs.CVar(_confMan, GoobCVars.BSORange, value => _range = value, true);
        Subs.CVar(_confMan, GoobCVars.BSOMapRelevance, value => _mapRelevance = value, true);
        Subs.CVar(_confMan, GoobCVars.BSOGrace, value => _grace = value, true);
        Subs.CVar(_confMan, GoobCVars.BSOTime, value => _timespan = value, true);
        Subs.CVar(_confMan, GoobCVars.BSOThreshold, value => _threshold = value, true);
        Subs.CVar(_confMan, GoobCVars.BSOPunishment, value => Enum.TryParse(value, out _punishment), true);
        Subs.CVar(_confMan, GoobCVars.BSORoleBanTime, value => _bantime = value, true);
    }

    // This doesn't narrow to CommandStaffComp in the case if 1. BSO isn't command. 2. Apostle is not BSO
    private void OnSpawn(PlayerSpawnCompleteEvent ev)
    {
        var mindId = _mind.GetMind(ev.Mob);

        if (!mindId.HasValue)
            return;

        var mindEntity = new Entity<MindComponent>(mindId.Value, Comp<MindComponent>(mindId.Value));

        TryAssignApostle(mindEntity);
    }


    // Have I not chosen you, the Twelve?
    private void OnAssigned(RulePlayerJobsAssignedEvent ev)
    {
        if (!_enabled)
            return;

        _cts = new CancellationTokenSource();
        var humans = _mind.GetAliveHumans();

        // No BSO
        if (!AssignApostle(humans))
            return;

        // No command other than BSO
        if (!CheckCommand())
            return;

        Timer.Spawn(TimeSpan.FromSeconds(_grace ? _threshold * _timespan * 2 : 0), Grace);
    }

    private void Grace()
    {
        Timer.SpawnRepeating(TimeSpan.FromSeconds(_timespan), Recheck, _cts.Token);
    }

    // Yet one of you is a Devil.
    private bool AssignApostle(HashSet<Entity<MindComponent>> humans)
    {
        foreach (var human in humans)
        {
            TryAssignApostle(human);
        }

        return _apostles.Count != 0;
    }

    private void TryAssignApostle(Entity<MindComponent> human)
    {
        if (!_job.MindTryGetJobId(human.Owner, out var jobId))
            return;

        if (jobId?.Id != _apostleJobProto || !human.Comp.OwnedEntity.HasValue)
            return;

        _apostles.TryAdd(human.Comp.OwnedEntity.Value, 0);
    }

    private void Recheck()
    {
        if (!CheckCommand())
            _cts.Cancel(); // It's fucked, we have no command - blueshield can now furioso

        CheckProximity();
        CheckCounts();
    }

    private void CheckProximity()
    {
        var apostlePositions = new Dictionary<EntityUid, Vector2>();
        var commandStaffPositions = new Dictionary<EntityUid, Vector2>();
        var commandStaffMaps = new HashSet<MapId>();

        var q = EntityQueryEnumerator<CommandStaffComponent, TransformComponent>();
        while (q.MoveNext(out var uid, out _, out var form))
        {
            var mapPos = _transform.GetMapCoordinates(uid, form).Position;
            var mapId = form.MapID;

            if (_apostles.ContainsKey(uid))
            {
                apostlePositions[uid] = mapPos;
            }
            else
            {
                commandStaffPositions[uid] = mapPos;
                commandStaffMaps.Add(mapId);
            }
        }

        foreach (var apostle in _apostles)
        {
            if (!apostlePositions.TryGetValue(apostle.Key, out var apoPos))
                continue;

            var apoForm = Comp<TransformComponent>(apostle.Key);

            // Skip if we care about maps and there are no command on the same map as BSO.
            if (_mapRelevance && !commandStaffMaps.Contains(apoForm.MapID))
                continue;

            var inRange = false;
            foreach (var (staffUid, staffPos) in commandStaffPositions)
            {
                var diff = staffPos - apoPos;
                if (diff.Length() < _range)
                {
                    inRange = true;
                    break;
                }
            }

            switch (inRange)
            {
                case false when ShouldIncrementTally(apostle.Key):
                    _apostles[apostle.Key] += 1;
                    break;
                case true:
                    _apostles[apostle.Key] = Math.Max(0, _apostles[apostle.Key] - 1);
                    break;
            }
        }
    }

    private void CheckCounts()
    {
        foreach (var apostle in _apostles)
        {
            if (apostle.Value >= _threshold / 2)
            {
                _popup.PopupEntity(Loc.GetString("bso-enforcement-warn", ("time", (_threshold-apostle.Value)*_timespan)), apostle.Key, apostle.Key, PopupType.MediumCaution);
            }

            if (apostle.Value == _threshold)
            {
                _popup.PopupEntity(Loc.GetString("bso-enforcement-threshold"),  apostle.Key, PopupType.MediumCaution);
                ThresholdReached(apostle.Key);
            }
        }
    }

    private bool CheckCommand()
    {
        var q = EntityQueryEnumerator<CommandStaffComponent>();

        // We have command if...
        while (q.MoveNext(out var uid, out var comp))
        {
            // They're not BSO
            if (_apostles.ContainsKey(uid))
                continue;

            // They're not AFK
            if (_player.TryGetSessionByEntity(uid, out var session) && _afk.IsAfk(session))
                continue;

            // They're not dead
            if (TryComp<MindComponent>(_mind.GetMind(uid), out var mindcomp) && _mind.IsCharacterDeadIc(mindcomp))
                continue;

            return true;
        }

        return false;
    }

    private bool ShouldIncrementTally(EntityUid key)
    {
        // Being out of reach of command should not count if...

        // We are afk or do not have a session for some reason
        if (!_player.TryGetSessionByEntity(key, out var sesh) || _afk.IsAfk(sesh))
            return false;

        // We are not even in game yet
        if (sesh.Status != SessionStatus.InGame)
            return false;

        // We are dead
        if (!_mind.TryGetMind(key, out _, out var comp) || _mind.IsCharacterDeadIc(comp))
            return false;

        // Its just fucked up, all of it.
        if (!CheckCommand())
            return false;

        return true;
    }

    private void Cleanup(RoundRestartCleanupEvent ev)
    {
        _apostles.Clear();
        _cts.Cancel();
    }
}
