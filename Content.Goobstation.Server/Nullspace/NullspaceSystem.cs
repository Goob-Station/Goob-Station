using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Nullspace;
using Content.Server.Mind;
using Content.Shared.Gravity;
using Robust.Server.GameStates;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Nullspace;

public sealed partial class NullspaceSystem : SharedNullspaceSystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private MetaDataSystem _meta = default!;
    [Dependency] private PvsOverrideSystem _pvs = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private static readonly EntProtoId PlayerPrototype = "MobNullspacePlayer";
    private static readonly TimeSpan CaughtTimeout = TimeSpan.FromSeconds(4);
    private static readonly Vector2i[] SpawnFacing = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };
    private const float MaxPitch = 1.45f;
    private const int JoinPathMin = 6;
    private const int JoinPathMax = 14;

    private readonly List<EntityUid> _ending = new();
    private readonly List<EntityUid> _leaving = new();

    public override void Initialize()
    {
        base.Initialize();
        _player.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _player.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    public bool TryEnter(ICommonSession session, float? clownDelay, [NotNullWhen(false)] out string? error)
    {
        error = null;
        if (session.AttachedEntity is not { } attached)
        {
            error = "cmd-nullspace-not-in-game";
            return false;
        }

        if (HasComp<NullspacePlayerComponent>(attached))
        {
            error = "cmd-nullspace-already";
            return false;
        }

        if (!_mind.TryGetMind(session.UserId, out var mindId, out var mind))
        {
            error = "cmd-nullspace-no-mind";
            return false;
        }

        if (mind.VisitingEntity != null)
            _mind.UnVisit(mindId.Value, mind);

        var instance = FindInstance() ?? CreateInstance(clownDelay);
        var maze = GetMaze(instance.Comp2);
        var cell = PickEntryCell(instance, maze);
        StreamChunks(instance, maze, true, cell);

        var yaw = Angle.Zero;
        foreach (var dir in SpawnFacing)
        {
            if (maze.IsWall(cell + dir))
                continue;

            yaw = new Angle(MathF.Atan2(-dir.X, dir.Y));
            break;
        }

        var body = Spawn(PlayerPrototype, new EntityCoordinates(instance, cell + new Vector2(0.5f, 0.5f)));
        var player = Comp<NullspacePlayerComponent>(body);
        player.Yaw = yaw;
        player.PrevYaw = yaw;
        _transform.SetLocalRotation(body, yaw + Math.PI);

        var tracker = EnsureComp<NullspaceTrackerComponent>(body);
        tracker.User = session.UserId;
        tracker.Map = instance;
        instance.Comp1.Players.Add(body);

        if (instance.Comp1.Clown is { } clown && !TerminatingOrDeleted(clown))
            _pvs.AddSessionOverride(clown, session);

        _mind.Visit(mindId.Value, body, mind);
        return true;
    }

    private Entity<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent>? FindInstance()
    {
        var query = EntityQueryEnumerator<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent>();
        while (query.MoveNext(out var uid, out var instance, out var map, out var grid))
        {
            if (!TerminatingOrDeleted(uid) && instance.Players.Count > 0)
                return (uid, instance, map, grid);
        }

        return null;
    }

    private Entity<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent> CreateInstance(float? clownDelay)
    {
        var mapUid = _map.CreateMap(out _);
        _meta.SetEntityName(mapUid, "nullspace");
        var grid = EnsureComp<MapGridComponent>(mapUid);

        var gravity = EnsureComp<GravityComponent>(mapUid);
        gravity.Enabled = true;
        gravity.Inherent = true;
        Dirty(mapUid, gravity);

        var map = EnsureComp<NullspaceMapComponent>(mapUid);
        map.Seed = _random.Next();
        Dirty(mapUid, map);

        var instance = EnsureComp<NullspaceInstanceComponent>(mapUid);
        instance.ClownSpawnAt = clownDelay ?? _random.NextFloat(NullspaceParams.ClownSpawnMin, NullspaceParams.ClownSpawnMax);
        return (mapUid, instance, map, grid);
    }

    private Vector2i PickEntryCell(Entity<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent> instance, NullspaceMaze maze)
    {
        var others = instance.Comp1.Players.Where(p => !TerminatingOrDeleted(p)).ToList();
        if (others.Count == 0)
            return new Vector2i(1, 1);

        var near = CellOf(_transform.GetWorldPosition(_random.Pick(others)));
        var cells = NullspacePathfinding.CellsAtDistance(maze, near, JoinPathMin, JoinPathMax);
        return cells.Count > 0 ? _random.Pick(cells) : near;
    }

    public void RemovePlayer(EntityUid body)
    {
        if (!TryComp<NullspaceTrackerComponent>(body, out var tracker))
            return;

        if (TryComp<NullspaceInstanceComponent>(tracker.Map, out var instance))
            instance.Players.Remove(body);

        if (_mind.TryGetMind(tracker.User, out var mindId, out var mind) && mind.VisitingEntity == body)
            _mind.UnVisit(mindId.Value, mind);

        QueueDel(body);
    }

    public void End(EntityUid mapUid)
    {
        if (!TryComp<NullspaceInstanceComponent>(mapUid, out var instance) || TerminatingOrDeleted(mapUid))
            return;

        foreach (var body in instance.Players.ToList())
        {
            RemovePlayer(body);
        }

        QueueDel(mapUid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _ending.Clear();
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<NullspaceInstanceComponent, NullspaceMapComponent, MapGridComponent>();
        while (query.MoveNext(out var uid, out var instance, out var map, out var grid))
        {
            _leaving.Clear();
            foreach (var body in instance.Players)
            {
                if (TerminatingOrDeleted(body)
                    || TryComp<NullspaceTrackerComponent>(body, out var tracker) && tracker.LeaveAt is { } leave && now >= leave)
                {
                    _leaving.Add(body);
                }
            }

            foreach (var body in _leaving)
            {
                instance.Players.Remove(body);
                RemovePlayer(body);
            }

            if (instance.Players.Count == 0)
            {
                _ending.Add(uid);
                continue;
            }

            var ent = (uid, instance, map, grid);
            var maze = GetMaze(map);
            StreamChunks(ent, maze, false);
            ProcessChunkQueues(ent);

            if (map.StartTime is not { } start)
                continue;

            var time = (float) (now - start).TotalSeconds;
            foreach (var body in instance.Players)
            {
                UpdateSteps((uid, instance), maze, body, frameTime);
            }

            UpdateClown((uid, instance, map), maze, time);
            UpdateLights((uid, instance, map), maze, NullspaceParams.Darkness(time));
            UpdateHorror(ent, maze);
        }

        foreach (var uid in _ending)
        {
            End(uid);
        }
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus != SessionStatus.Disconnected)
            return;

        _leaving.Clear();
        var query = EntityQueryEnumerator<NullspaceTrackerComponent>();
        while (query.MoveNext(out var uid, out var tracker))
        {
            if (tracker.User == args.Session.UserId)
                _leaving.Add(uid);
        }

        foreach (var body in _leaving)
        {
            RemovePlayer(body);
        }
    }

    [SubscribeNetworkEvent]
    private void OnLook(NullspaceLookEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } uid
            || !TryComp<NullspacePlayerComponent>(uid, out var player)
            || !float.IsFinite(ev.Yaw)
            || !float.IsFinite(ev.Pitch)
            || player.Frozen)
        {
            return;
        }

        player.Yaw = new Angle(ev.Yaw);
        _transform.SetLocalRotation(uid, player.Yaw + Math.PI);

        var pitch = Math.Clamp(ev.Pitch, -MaxPitch, MaxPitch);
        if (MathF.Abs(pitch - player.Pitch) < 0.01f)
            return;

        player.Pitch = pitch;
        Dirty(uid, player);
    }

    [SubscribeNetworkEvent]
    private void OnStart(NullspaceStartEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } uid
            || !HasComp<NullspacePlayerComponent>(uid)
            || !TryGetMap(uid, out var map)
            || map.Comp.StartTime != null)
        {
            return;
        }

        map.Comp.StartTime = _timing.CurTime;
        Dirty(map);
    }

    private void UpdateSteps(Entity<NullspaceInstanceComponent> instance, NullspaceMaze maze, EntityUid body, float frameTime)
    {
        if (!TryComp<NullspaceTrackerComponent>(body, out var tracker)
            || !TryComp<NullspacePlayerComponent>(body, out var player)
            || player.Frozen)
        {
            return;
        }

        var meters = _transform.GetWorldPosition(body) * Cell;
        if (tracker.LastPosition is not { } last)
        {
            tracker.LastPosition = meters;
            return;
        }

        tracker.LastPosition = meters;
        var moved = (meters - last).Length();
        if (moved > Cell)
            return;

        var stride = player.Sprinting ? NullspaceParams.SprintStride : NullspaceParams.WalkStride;
        tracker.StepDistance += moved;
        if (tracker.StepDistance < stride)
            return;

        tracker.StepDistance %= stride;
        var speed = moved / MathF.Max(frameTime, 1e-4f);
        if (speed <= 0.4f)
            return;

        StepSound(body, tracker, player.Sprinting);

        var radius = player.Sprinting
            ? NullspaceParams.NoiseSprint
            : speed > 1f ? NullspaceParams.NoiseWalk : NullspaceParams.NoiseCreep;
        Hear(instance, maze, meters, radius);
    }

    private void StepSound(EntityUid body, NullspaceTrackerComponent tracker, bool sprinting)
    {
        var filter = Filter.Pvs(body, entityManager: EntityManager);
        if (_player.TryGetSessionById(tracker.User, out var session))
            filter.RemovePlayer(session);

        var step = _audio.PlayEntity(sprinting ? NullspaceAudio.Run : NullspaceAudio.Step,
            filter,
            body,
            true,
            NullspaceAudio.Positional(sprinting ? 0.75f : 0.42f, _random.NextFloat(0.92f, 1.04f), 1.5f, 1f, 30f));
        ApplyReverb(step);
    }
}
