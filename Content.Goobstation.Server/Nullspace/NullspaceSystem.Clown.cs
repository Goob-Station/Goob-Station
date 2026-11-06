using System.Linq;
using System.Numerics;
using Content.Goobstation.Server.Nullspace.HTN;
using Content.Goobstation.Shared.Nullspace;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks.Operators.Specific;
using Content.Server.NPC.Systems;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Nullspace;

public sealed partial class NullspaceSystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private MovementSpeedModifierSystem _speed = default!;
    [Dependency] private NPCSystem _npc = default!;
    [Dependency] private HTNSystem _htn = default!;

    private static readonly EntProtoId ClownPrototype = "MobNullspaceClown";
    private static readonly TimeSpan StuckInterval = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan FeedTime = TimeSpan.FromSeconds(3);

    private const float Cell = NullspaceParams.Cell;
    private const float ClownAcceleration = 20f;
    private const int RelocatePathMin = 12;
    private const int RelocatePathMax = 24;
    private const float StuckDistance = 1f;

    private void UpdateClown(Entity<NullspaceInstanceComponent, NullspaceMapComponent> ent, NullspaceMaze maze, float time)
    {
        var instance = ent.Comp1;
        if (instance.Clown is not { } clown)
        {
            if (time >= instance.ClownSpawnAt && RandomActivePlayer(instance) is { } victim)
                SpawnClown(ent, maze, victim);
            return;
        }

        if (TerminatingOrDeleted(clown)
            || !TryComp<NullspaceClownComponent>(clown, out var state)
            || !TryComp<HTNComponent>(clown, out var htn))
        {
            return;
        }

        var now = _timing.CurTime;
        if (instance.ClownResumeAt is { } resume)
        {
            if (now < resume)
                return;

            instance.ClownResumeAt = null;
            _npc.SetBlackboard(clown, NullspaceBlackboard.Chasing, false);
            SetClownState(clown, NullspaceClownState.Hunt, htn.Blackboard.GetValueOrDefault<TimeSpan>(NullspaceBlackboard.SpawnTime, EntityManager));
            _htn.SetHTNEnabled((clown, htn), true);
        }

        var clownTiles = _transform.GetWorldPosition(clown);
        if (NearestActivePlayer(instance, clownTiles) is not { } target)
            return;

        if (!htn.Blackboard.TryGetValue<EntityUid>(NullspaceBlackboard.HuntTarget, out var current, EntityManager) || current != target)
            _npc.SetBlackboard(clown, NullspaceBlackboard.HuntTarget, target);

        if (state.State is NullspaceClownState.Chase or NullspaceClownState.Caught or NullspaceClownState.Peek)
            return;

        var targetPos = _transform.GetWorldPosition(target) * Cell;
        var targetChunk = ChunkOf(CellOf(targetPos / Cell));
        var clownChunk = ChunkOf(CellOf(clownTiles));
        var tooFar = Math.Max(Math.Abs(targetChunk.X - clownChunk.X), Math.Abs(targetChunk.Y - clownChunk.Y)) > 1;

        var stuck = false;
        if (now >= instance.ClownCheckAt)
        {
            stuck = !htn.Blackboard.ContainsKey(NullspaceBlackboard.SpawnIdle)
                && (clownTiles - instance.ClownCheckPosition).Length() < StuckDistance;
            instance.ClownCheckPosition = clownTiles;
            instance.ClownCheckAt = now + StuckInterval;
        }

        if (tooFar || stuck)
            RelocateClown(ent, maze, (clown, htn), targetPos);
    }

    private EntityUid? RandomActivePlayer(NullspaceInstanceComponent instance)
    {
        var active = instance.Players.Where(IsHuntable).ToList();
        return active.Count > 0 ? _random.Pick(active) : null;
    }

    private EntityUid? NearestActivePlayer(NullspaceInstanceComponent instance, Vector2 from)
    {
        EntityUid? best = null;
        var bestDistance = float.MaxValue;
        foreach (var body in instance.Players)
        {
            if (!IsHuntable(body))
                continue;

            var distance = (_transform.GetWorldPosition(body) - from).LengthSquared();
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = body;
        }

        return best;
    }

    private bool IsHuntable(EntityUid body)
    {
        return !TerminatingOrDeleted(body) && TryComp<NullspacePlayerComponent>(body, out var player) && !player.Frozen;
    }

    private void RelocateClown(Entity<NullspaceInstanceComponent, NullspaceMapComponent> ent, NullspaceMaze maze, Entity<HTNComponent> clown, Vector2 targetPos)
    {
        if (!TryPickClownCell(ent.Comp1, maze, targetPos, RelocatePathMin, RelocatePathMax, out var cell))
            return;

        var position = cell + new Vector2(0.5f, 0.5f);
        _transform.SetCoordinates(clown, new EntityCoordinates(ent.Owner, position));
        _htn.SetHTNEnabled(clown, false);
        _htn.SetHTNEnabled(clown, true);
        ent.Comp1.ClownCheckPosition = position;
        ent.Comp1.ClownCheckAt = _timing.CurTime + StuckInterval;
    }

    private bool TryPickClownCell(NullspaceInstanceComponent instance, NullspaceMaze maze, Vector2 targetPos, int min, int max, out Vector2i cell)
    {
        var targetCell = CellOf(targetPos / Cell);
        var targetChunk = ChunkOf(targetCell);
        var candidates = NullspacePathfinding.CellsAtDistance(maze, targetCell, min, max)
            .Where(c =>
            {
                var chunk = ChunkOf(c);
                return Math.Max(Math.Abs(chunk.X - targetChunk.X), Math.Abs(chunk.Y - targetChunk.Y)) <= 1;
            })
            .ToList();

        var hidden = candidates.Where(c => !SeenByAnyone(instance, maze, c)).ToList();
        var pool = hidden.Count > 0 ? hidden : candidates;
        if (pool.Count == 0)
        {
            cell = default;
            return false;
        }

        cell = _random.Pick(pool);
        return true;
    }

    private bool SeenByAnyone(NullspaceInstanceComponent instance, NullspaceMaze maze, Vector2i cell)
    {
        foreach (var body in instance.Players)
        {
            if (TerminatingOrDeleted(body))
                continue;

            var tiles = _transform.GetWorldPosition(body);
            if (maze.LineClear(cell.X + 0.5f, cell.Y + 0.5f, tiles.X, tiles.Y))
                return true;
        }

        return false;
    }

    private void SpawnClown(Entity<NullspaceInstanceComponent, NullspaceMapComponent> ent, NullspaceMaze maze, EntityUid victim)
    {
        var victimPos = _transform.GetWorldPosition(victim) * Cell;
        if (!TryPickClownCell(ent.Comp1, maze, victimPos, NullspaceParams.ClownSpawnPathMin, NullspaceParams.ClownSpawnPathMax, out var cell)
            && !TryPickClownCell(ent.Comp1, maze, victimPos, RelocatePathMin, 50, out cell))
        {
            return;
        }

        var spawn = cell + new Vector2(0.5f, 0.5f);
        StreamChunks((ent.Owner, ent.Comp1, ent.Comp2, Comp<MapGridComponent>(ent)), maze, true, cell);

        var clown = Spawn(ClownPrototype, new EntityCoordinates(ent.Owner, spawn));
        ent.Comp1.Clown = clown;
        ent.Comp1.ClownCheckPosition = spawn;
        ent.Comp1.ClownCheckAt = _timing.CurTime + StuckInterval;

        var now = _timing.CurTime;
        _npc.SetBlackboard(clown, NullspaceBlackboard.HuntTarget, victim);
        _npc.SetBlackboard(clown, NullspaceBlackboard.SpawnTime, now);
        _npc.SetBlackboard(clown, NullspaceBlackboard.NextHonk, now + TimeSpan.FromSeconds(_random.NextFloat(14f, 22f)));
        _npc.SetBlackboard(clown, NullspaceBlackboard.SpawnIdle, _random.NextFloat(6f, 10f));
        _npc.SetBlackboard(clown, NullspaceBlackboard.NextPeek, now + TimeSpan.FromSeconds(_random.NextFloat(30f, 50f)));

        foreach (var body in ent.Comp1.Players)
        {
            if (!TryComp<NullspaceTrackerComponent>(body, out var tracker)
                || !_player.TryGetSessionById(tracker.User, out var session))
            {
                continue;
            }

            _pvs.AddSessionOverride(clown, session);

            var playerPos = _transform.GetWorldPosition(body) * Cell;
            var angle = MathF.Atan2(spawn.Y * Cell - playerPos.Y, spawn.X * Cell - playerPos.X) + _random.NextFloat(-0.8f, 0.8f);
            var chimePos = playerPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * _random.NextFloat(12f, 18f);
            var chime = _audio.PlayStatic(NullspaceAudio.Chime,
                Filter.SinglePlayer(session),
                ToCoordinates(ent.Owner, chimePos),
                true,
                NullspaceAudio.Positional(1.6f, _random.NextFloat(0.84f, 0.92f), 18f, 1.2f, 60f));
            Unoccluded(chime);
            ApplyReverb(chime);
        }
    }

    public bool SetClownState(EntityUid clown, NullspaceClownState state, TimeSpan spawned, EntityUid? target = null)
    {
        if (!TryComp<NullspaceClownComponent>(clown, out var comp))
            return false;

        if (comp.State == state && comp.Target == target)
            return false;

        var changed = comp.State != state;
        comp.State = state;
        comp.Target = target;
        Dirty(clown, comp);

        if (!changed)
            return false;

        var speed = ClownSpeed(state, (float) (_timing.CurTime - spawned).TotalSeconds) / Cell;
        _speed.ChangeBaseSpeed(clown, speed, speed, ClownAcceleration);
        return true;
    }

    public void Stinger(EntityUid quarry)
    {
        if (_player.TryGetSessionByEntity(quarry, out var session))
            _audio.PlayGlobal(NullspaceAudio.Stinger, session, NullspaceAudio.Global(0.75f));
    }

    private static float ClownSpeed(NullspaceClownState state, float aliveSeconds)
    {
        return state switch
        {
            NullspaceClownState.Chase => MathF.Min(NullspaceParams.ClownChaseSpeedCap,
                NullspaceParams.ClownChaseSpeed + NullspaceParams.ClownChaseRampPerMin * aliveSeconds / 60f),
            NullspaceClownState.Search => NullspaceParams.ClownSearchSpeed,
            NullspaceClownState.Caught or NullspaceClownState.Peek => 0f,
            _ => NullspaceParams.ClownHuntSpeed,
        };
    }

    private void Hear(Entity<NullspaceInstanceComponent> instance, NullspaceMaze maze, Vector2 position, float radius)
    {
        if (instance.Comp.Clown is not { } clown
            || !TryComp<NullspaceClownComponent>(clown, out var state)
            || !HasComp<HTNComponent>(clown)
            || state.State is NullspaceClownState.Chase or NullspaceClownState.Caught)
        {
            return;
        }

        var clownPos = _transform.GetWorldPosition(clown) * Cell;
        if ((position - clownPos).Length() > radius)
            return;

        var goal = CellOf(position / Cell);
        var path = NullspacePathfinding.FindPath(maze, CellOf(clownPos / Cell), goal, 1500);
        if (path == null || path.Count * Cell > radius * 1.5f)
            return;

        _npc.SetBlackboard(clown, NullspaceBlackboard.SearchCoordinates,
            new EntityCoordinates(instance.Owner, goal + new Vector2(0.5f, 0.5f)));
        _npc.SetBlackboard(clown, NullspaceBlackboard.SearchUntil,
            _timing.CurTime + TimeSpan.FromSeconds(NullspaceParams.ClownSearchTime));
        _npc.SetBlackboard(clown, NullspaceBlackboard.SearchFresh, true);
    }

    [SubscribeLocalEvent]
    private void OnClownHtnEvent(Entity<NullspaceClownComponent> clown, ref HTNRaisedEvent args)
    {
        if (args.Args is NullspaceSilenceRequest
            && args.Target is { } listener
            && TryComp<NullspaceTrackerComponent>(listener, out var listenerTracker))
        {
            SendSilence(listener, listenerTracker);
            return;
        }

        if (args.Args is not NullspaceCatchEvent
            || args.Target is not { } target
            || !TryGetMap(clown, out var map)
            || !TryComp<NullspaceInstanceComponent>(map, out var instance)
            || !instance.Players.Contains(target)
            || !TryComp<NullspacePlayerComponent>(target, out var player)
            || !TryComp<NullspaceTrackerComponent>(target, out var tracker)
            || player.Frozen)
        {
            return;
        }

        player.Frozen = true;
        Dirty(target, player);
        tracker.LeaveAt = _timing.CurTime + CaughtTimeout;

        SetClownState(clown, NullspaceClownState.Caught, TimeSpan.Zero, target);
        if (TryComp<HTNComponent>(clown, out var htn))
            htn.Enabled = false;

        instance.ClownResumeAt = _timing.CurTime + FeedTime;
    }
}
