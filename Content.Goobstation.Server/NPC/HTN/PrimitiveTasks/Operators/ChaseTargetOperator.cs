using System.Numerics;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Server.NPC.Systems;
using Content.Shared.Examine;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class ChaseTargetOperator : HTNOperator, IHtnConditionalShutdown
{
    [Dependency] private IEntityManager _entManager = default!;
    [Dependency] private IGameTiming _timing = default!;

    private ExamineSystemShared _examine = default!;
    private NPCSteeringSystem _steering = default!;
    private SharedTransformSystem _transform = default!;

    private const string LastSeenTimeKey = "ChaseLastSeenTime";
    private const string LastSeenPositionKey = "ChaseLastSeenPosition";
    private const string LastSeenVelocityKey = "ChaseLastSeenVelocity";

    [DataField]
    public HTNPlanState ShutdownState { get; private set; } = HTNPlanState.TaskFinished;

    [DataField]
    public string TargetKey = "Target";

    [DataField]
    public string RangeKey = "MeleeRange";

    [DataField]
    public float LoseSightAfter = 1.5f;

    [DataField]
    public float Prediction = 1.5f;

    [DataField]
    public string? LostCoordinatesKey;

    [DataField]
    public string? LostUntilKey;

    [DataField]
    public float Memory = 12f;

    [DataField]
    public string? LostFreshKey;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _examine = sysManager.GetEntitySystem<ExamineSystemShared>();
        _steering = sysManager.GetEntitySystem<NPCSteeringSystem>();
        _transform = sysManager.GetEntitySystem<SharedTransformSystem>();
    }

    public override void Startup(NPCBlackboard blackboard)
    {
        base.Startup(blackboard);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager))
            return;

        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var steering = _steering.Register(owner, new EntityCoordinates(target, Vector2.Zero));
        steering.Range = blackboard.GetValueOrDefault<float>(RangeKey, _entManager);
        blackboard.SetValue(LastSeenTimeKey, _timing.CurTime);
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)
            || !_entManager.TryGetComponent<TransformComponent>(target, out var targetXform))
        {
            return HTNOperatorStatus.Failed;
        }

        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var now = _timing.CurTime;
        var vision = blackboard.GetValueOrDefault<float>(blackboard.GetVisionRadiusKey(_entManager), _entManager);
        var targetPos = _transform.GetMapCoordinates(target, targetXform);

        if (_examine.InRangeUnOccluded(owner, target, vision + 0.5f))
        {
            blackboard.SetValue(LastSeenTimeKey, now);
            blackboard.SetValue(LastSeenPositionKey, targetPos);
            blackboard.SetValue(LastSeenVelocityKey,
                _entManager.TryGetComponent<PhysicsComponent>(target, out var physics) ? physics.LinearVelocity : Vector2.Zero);

            var range = blackboard.GetValueOrDefault<float>(RangeKey, _entManager);
            var ownerPos = _transform.GetMapCoordinates(owner);
            return ownerPos.MapId == targetPos.MapId && (targetPos.Position - ownerPos.Position).Length() < range
                ? HTNOperatorStatus.Finished
                : HTNOperatorStatus.Continuing;
        }

        var lastSeen = blackboard.GetValueOrDefault<TimeSpan>(LastSeenTimeKey, _entManager);
        if ((now - lastSeen).TotalSeconds <= LoseSightAfter)
            return HTNOperatorStatus.Continuing;

        Remember(blackboard, targetPos, now);
        return HTNOperatorStatus.Failed;
    }

    private void Remember(NPCBlackboard blackboard, MapCoordinates fallback, TimeSpan now)
    {
        if (LostCoordinatesKey == null)
            return;

        var seen = blackboard.TryGetValue<MapCoordinates>(LastSeenPositionKey, out var position, _entManager)
            ? position
            : fallback;
        var velocity = blackboard.GetValueOrDefault<Vector2>(LastSeenVelocityKey, _entManager);
        var guess = new MapCoordinates(seen.Position + velocity * Prediction, seen.MapId);

        blackboard.SetValue(LostCoordinatesKey, _transform.ToCoordinates(guess));

        if (LostUntilKey != null)
            blackboard.SetValue(LostUntilKey, now + TimeSpan.FromSeconds(Memory));

        if (LostFreshKey != null)
            blackboard.SetValue(LostFreshKey, true);
    }

    public void ConditionalShutdown(NPCBlackboard blackboard)
    {
        _steering.Unregister(blackboard.GetValue<EntityUid>(NPCBlackboard.Owner));
    }
}
