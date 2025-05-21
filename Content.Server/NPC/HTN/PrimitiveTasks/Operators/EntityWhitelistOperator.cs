using Content.Shared.Whitelist;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class EntityWhitelistOperator : HTNOperator, IHtnConditionalShutdown
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    private EntityLookupSystem _entityLookup = default!;
    private SharedTransformSystem _transform = default!;
    private EntityWhitelistSystem _whitelist = default!;

    [DataField]
    public HTNPlanState ShutdownState { get; private set; } = HTNPlanState.TaskFinished;

    [DataField(required: true)]
    public EntityWhitelist Whitelist = default!;

    [DataField]
    public string RangeKey = NPCBlackboard.SecuritronPatrolRange;

    [DataField(required: true)]
    public string TargetKey = string.Empty;

    [DataField(required: true)]
    public string TargetMoveKey = string.Empty;

    [DataField(required: true)]
    public string ListKey = string.Empty;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _entityLookup = sysManager.GetEntitySystem<EntityLookupSystem>();
        _transform = sysManager.GetEntitySystem<SharedTransformSystem>();
        _whitelist = sysManager.GetEntitySystem<EntityWhitelistSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard, CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<float>(RangeKey, out var range, _entManager))
            return (false, null);

        if (!blackboard.TryGetValue<List<EntityUid>>(ListKey, out var entities, _entManager) || entities.Count <= 0)
            entities = GetEntities(owner, range);

        return (true, new Dictionary<string, object>()
        {
            {TargetKey, entities[0]},
            {TargetMoveKey, _entManager.GetComponent<TransformComponent>(entities[0]).Coordinates},
            {ListKey, entities}
        });
    }

    private List<EntityUid> GetEntities(EntityUid owner, float range)
    {
        var ownerPos = _entManager.GetComponent<TransformComponent>(owner).Coordinates.Position;

        SortedDictionary<float, HashSet<EntityUid>> sortedEntities = new();

        foreach (var entity in _entityLookup.GetEntitiesInRange(owner, range))
        {
            if (_whitelist.IsWhitelistFail(Whitelist, entity))
                continue;

            var entPos = _entManager.GetComponent<TransformComponent>(entity).Coordinates.Position;

            var distance = Vector2.Subtract(ownerPos, entPos).Length();

            if (sortedEntities.ContainsKey(distance))
            {
                sortedEntities[distance].Add(entity);
                continue;
            }

            HashSet<EntityUid> entList = new();
            entList.Add(entity);

            sortedEntities.Add(distance, entList);
        }

        List<EntityUid> entities = new();

        foreach (var entityList in sortedEntities.Values)
        {
            foreach (var entity in entityList)
            {
                entities.Add(entity);
            }
        }

        return entities;
    }

    public void ConditionalShutdown(NPCBlackboard blackboard)
    {
        if (!blackboard.TryGetValue<List<EntityUid>>(ListKey, out var entities, _entManager) || entities.Count <= 0)
            return;

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager))
            return;

        entities.Remove(target);
        blackboard.SetValue(ListKey, entities);
    }
}
