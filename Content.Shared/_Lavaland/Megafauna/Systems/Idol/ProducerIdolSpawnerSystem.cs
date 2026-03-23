using Content.Shared._Lavaland.Megafauna.Components.Idol;
using Content.Shared._Lavaland.Megafauna.Events.Idol;
using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Systems.Idol;

/// <summary>
/// Handles idol-spawning for the Producer megafauna across its three phases.
///
/// Phase 1
///   One idol alive at a time. The action fires only when no idol is currently
///   alive. Idols are drawn from a shuffled copy of the pool so no prototype
///   repeats until every entry has been used once, then the pool resets.
///
/// Phase 2
///   A randomly chosen named group is spawned. The action is blocked until
///   every entity in the previous group is dead.
///
/// Phase 3
///   The initial fire spawns the entire pool at once. After that
///   watches for dead idols and re-queues each one individually with a cooldown
///   timer, then spawns it when the timer expires.
/// </summary>
public sealed class ProducerIdolSpawner : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IdolProducerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<IdolProducerComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<IdolProducerComponent, ProducerSingleIdolSpawnEvent>(OnSingleSpawn);
        SubscribeLocalEvent<IdolProducerComponent, ProducerGroupIdolSpawnEvent>(OnGroupSpawn);
        SubscribeLocalEvent<IdolProducerComponent, ProducerReplenishIdolSpawnEvent>(OnReplenishSpawn);
    }

    private void OnInit(EntityUid uid, IdolProducerComponent comp, ComponentInit args)
    {
        if (comp.SingleSpawnAction != null)
            _actions.AddAction(uid, ref comp.SingleSpawnActionEntity, comp.SingleSpawnAction);

        if (comp.GroupSpawnAction != null)
            _actions.AddAction(uid, ref comp.GroupSpawnActionEntity, comp.GroupSpawnAction);

        if (comp.ReplenishSpawnAction != null)
            _actions.AddAction(uid, ref comp.ReplenishSpawnActionEntity, comp.ReplenishSpawnAction);

        // Pre-fill the phase-1 shuffle deck so it is ready on the first action fire.
        RefillPhase1Deck(comp);
    }

    private void OnShutdown(EntityUid uid, IdolProducerComponent comp, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, comp.SingleSpawnActionEntity);
        _actions.RemoveAction(uid, comp.GroupSpawnActionEntity);
        _actions.RemoveAction(uid, comp.ReplenishSpawnActionEntity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<IdolProducerComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            // Only relevant during phase 3.
            if (comp.Phase3RespawnTimers.Count == 0)
                continue;

            // Check each waiting idol; spawn any whose timer has elapsed.
            var ready = new List<int>();
            foreach (var (index, spawnAt) in comp.Phase3RespawnTimers)
            {
                if (now >= spawnAt)
                    ready.Add(index);
            }

            foreach (var index in ready)
            {
                comp.Phase3RespawnTimers.Remove(index);
                SpawnPhase3Idol(uid, comp, index);
            }

            // Also detect newly dead phase-3 idols and queue their respawn.
            var deadIndices = new List<int>();
            foreach (var (index, entity) in comp.Phase3LiveIdols)
            {
                if (IsDead(entity))
                    deadIndices.Add(index);
            }

            foreach (var index in deadIndices)
            {
                comp.Phase3LiveIdols.Remove(index);

                // Don't double-queue.
                if (!comp.Phase3RespawnTimers.ContainsKey(index))
                    comp.Phase3RespawnTimers[index] = now + TimeSpan.FromSeconds(comp.ReplenishCooldown);
            }
        }
    }

    private void OnSingleSpawn(EntityUid uid, IdolProducerComponent comp, ProducerSingleIdolSpawnEvent args)
    {
        if (args.Handled)
            return;

        // Block if the current idol is still alive.
        if (comp.Phase1LiveIdol.HasValue && !IsDead(comp.Phase1LiveIdol.Value))
            return;

        comp.Phase1LiveIdol = null;

        // If the deck is empty, refill and reshuffle.
        if (comp.Phase1RemainingIndices.Count == 0)
            RefillPhase1Deck(comp);

        if (comp.Phase1RemainingIndices.Count == 0)
            return; // Pool is empty; shouldn't happen if YAML is configured correctly.

        // Draw from the back of the shuffled list (cheap removal).
        var drawIndex = comp.Phase1RemainingIndices.Count - 1;
        var poolIndex = comp.Phase1RemainingIndices[drawIndex];
        comp.Phase1RemainingIndices.RemoveAt(drawIndex);

        var proto = comp.SingleIdolPool[poolIndex];
        comp.Phase1LiveIdol = SpawnLinkedIdol(proto, RandomAdjacentPos(uid, comp), uid);

        args.Handled = true;
    }

    private void OnGroupSpawn(EntityUid uid, IdolProducerComponent comp, ProducerGroupIdolSpawnEvent args)
    {
        if (args.Handled)
            return;

        if (comp.IdolGroupPool.Count == 0)
            return;

        // Prune the live-group list; block if any member is still alive.
        comp.Phase2LiveGroup.RemoveAll(IsDead);
        if (comp.Phase2LiveGroup.Count > 0)
            return;

        var group = _random.Pick(comp.IdolGroupPool);
        foreach (var proto in group)
        {
            var spawned = SpawnLinkedIdol(proto, RandomAdjacentPos(uid, comp), uid);
            comp.Phase2LiveGroup.Add(spawned);
        }

        args.Handled = true;
    }

    private void OnReplenishSpawn(EntityUid uid, IdolProducerComponent comp, ProducerReplenishIdolSpawnEvent args)
    {
        if (args.Handled)
            return;

        if (comp.SingleIdolPool.Count == 0)
            return;

        // Spawn every idol in the pool that isn't already alive.
        for (var i = 0; i < comp.SingleIdolPool.Count; i++)
        {
            if (comp.Phase3LiveIdols.TryGetValue(i, out var existing) && !IsDead(existing))
                continue; // Already alive, skip.

            SpawnPhase3Idol(uid, comp, i);
        }

        args.Handled = true;
    }

    /// <summary>
    /// Spawns the idol
    /// </summary>
    private void SpawnPhase3Idol(EntityUid uid, IdolProducerComponent comp, int poolIndex)
    {
        var proto = comp.SingleIdolPool[poolIndex];
        var spawned = SpawnLinkedIdol(proto, RandomAdjacentPos(uid, comp), uid);
        comp.Phase3LiveIdols[poolIndex] = spawned;
    }

    /// <summary>
    /// Spawns an idol prototype and sets
    /// so the idol knows which Producer to damage when it dies.
    /// </summary>
    private EntityUid SpawnLinkedIdol(EntProtoId proto, EntityCoordinates pos, EntityUid producer)
    {
        var spawned = SpawnAtPosition(proto, pos);
        if (TryComp<IdolComponent>(spawned, out var idol))
            idol.Producer = producer;
        return spawned;
    }

    /// <summary>
    /// Returns coordinates offset from the Producer in a random direction.
    /// </summary>
    private EntityCoordinates RandomAdjacentPos(EntityUid uid, IdolProducerComponent comp)
    {
        if (!TryComp<TransformComponent>(uid, out var xform))
            return new EntityCoordinates(uid, Vector2.Zero);

        var directions = new[]
        {
            new Vector2( 0,  1),
            new Vector2( 0, -1),
            new Vector2( 1,  0),
            new Vector2(-1,  0),
            new Vector2( 1,  1).Normalized(),
            new Vector2( 1, -1).Normalized(),
            new Vector2(-1,  1).Normalized(),
            new Vector2(-1, -1).Normalized(),
        };

        var dir = _random.Pick(directions) * comp.SpawnOffset;
        return xform.Coordinates.Offset(dir);
    }

    /// <summary>
    /// Returns true if the entity no longer exists or is terminating/dead.
    /// Used as a predicate for RemoveAll and for polling live-idol dictionaries.
    /// </summary>
    private bool IsDead(EntityUid entity)
    {
        return !_entityManager.EntityExists(entity)
            || _entityManager.GetComponent<MetaDataComponent>(entity).EntityLifeStage >= EntityLifeStage.Terminating;
    }

    /// <summary>
    /// Fills the pool with
    /// a freshly shuffled list of all pool indices.
    /// </summary>
    private void RefillPhase1Deck(IdolProducerComponent comp)
    {
        comp.Phase1RemainingIndices.Clear();
        for (var i = 0; i < comp.SingleIdolPool.Count; i++)
            comp.Phase1RemainingIndices.Add(i);

        _random.Shuffle(comp.Phase1RemainingIndices);
    }
}
